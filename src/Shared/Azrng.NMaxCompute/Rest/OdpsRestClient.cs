using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text;
using Azrng.NMaxCompute.Accounts;
using Microsoft.Extensions.Logging;

namespace Azrng.NMaxCompute.Rest;

/// <summary>
/// ODPS REST 客户端：负责签名注入、HTTP 调用、重试、错误解析、V4→V1 降级
/// </summary>
public sealed class OdpsRestClient
{
    private const string UserAgentTemplate = "Azrng.NMaxCompute.Direct/{0} ({1}; {2})";

    private static readonly string UserAgent =
        string.Format(UserAgentTemplate,
            typeof(OdpsRestClient).Assembly.GetName().Version?.ToString() ?? "0.1.0",
            RuntimeInformation.FrameworkDescription,
            RuntimeInformation.OSDescription);

    private static readonly HashSet<int> RetryableStatusCodes = new() { 502, 503, 504 };

    /// <summary>
    /// V4 不被服务端支持时返回的错误关键字，触发降级
    /// </summary>
    private static readonly string[] V4RejectedMarkers =
    {
        "need ak v3 support",
        "accesskey acl denied",
        "ODPS-0410051",
        "invalid or missing"
    };

    private readonly HttpClient _httpClient;
    private readonly IAccount _account;
    private readonly string _endpoint;
    private readonly int _retryTimes;
    private readonly ILogger? _logger;

    public OdpsRestClient(HttpClient httpClient, IAccount account, string endpoint, ILogger? logger = null, int retryTimes = 4)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _account = account ?? throw new ArgumentNullException(nameof(account));
        _endpoint = (endpoint ?? throw new ArgumentNullException(nameof(endpoint))).TrimEnd('/');
        _retryTimes = retryTimes;
        _logger = logger;

        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);
    }

    /// <summary>
    /// 发送请求
    /// </summary>
    public async Task<OdpsResponse> SendAsync(OdpsRequest request, CancellationToken cancellationToken = default)
    {
        return await SendInternalAsync(request, allowV4Downgrade: true, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// 流式发送请求：成功（2xx）时返回未读完的响应体流，供 Tunnel 等大块二进制场景按需读取。
    /// <para>
    /// 不做重试与 V4→V1 自动降级（流式响应消费语义复杂）；失败时读完整 body 抛 <see cref="OdpsException"/>。
    /// 上层如需重试/降级，应自行在捕获后重发。
    /// </para>
    /// </summary>
    public async Task<OdpsStreamResponse> SendStreamingAsync(OdpsRequest request, CancellationToken cancellationToken = default)
    {
        _account.Sign(request);

        using var httpMessage = new HttpRequestMessage(new HttpMethod(request.Method), BuildUrl(request));
        ApplyHeaders(httpMessage, request);

        if (request.Body is { Length: > 0 } body)
        {
            httpMessage.Content = new ByteArrayContent(body);
        }

        // 注意：不 using httpResponse —— 成功时交给 OdpsStreamResponse 持有并最终释放
        var httpResponse = await _httpClient.SendAsync(httpMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);

        if (httpResponse.IsSuccessStatusCode)
        {
            var headers = CollectHeaders(httpResponse);
            var stream = await httpResponse.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            return new OdpsStreamResponse((int)httpResponse.StatusCode, headers, stream, httpResponse);
        }

        // 失败：读完整 body 后抛异常
        var bodyBytes = await httpResponse.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
        var bodyText = bodyBytes.Length > 0 ? Encoding.UTF8.GetString(bodyBytes) : string.Empty;
        httpResponse.Dispose();
        throw BuildException((int)httpResponse.StatusCode, bodyText);
    }

    private static void ApplyHeaders(HttpRequestMessage httpMessage, OdpsRequest request)
    {
        foreach (var (key, value) in request.Headers)
        {
            if (string.Equals(key, "Content-Type", StringComparison.OrdinalIgnoreCase))
            {
                httpMessage.Content ??= new ByteArrayContent(Array.Empty<byte>());
                httpMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(value);
            }
            else if (!httpMessage.Headers.TryAddWithoutValidation(key, value))
            {
                httpMessage.Content ??= new ByteArrayContent(Array.Empty<byte>());
                httpMessage.Content?.Headers.TryAddWithoutValidation(key, value);
            }
        }
    }

    private static Dictionary<string, string> CollectHeaders(HttpResponseMessage httpResponse)
    {
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var h in httpResponse.Headers)
            headers[h.Key] = string.Join(",", h.Value);
        foreach (var h in httpResponse.Content.Headers)
            headers[h.Key] = string.Join(",", h.Value);
        return headers;
    }

    private async Task<OdpsResponse> SendInternalAsync(OdpsRequest request, bool allowV4Downgrade, CancellationToken cancellationToken)
    {
        var attempts = Math.Max(1, _retryTimes);
        Exception? lastException = null;

        for (var attempt = 0; attempt < attempts; attempt++)
        {
            try
            {
                var response = await SendCoreAsync(request, cancellationToken).ConfigureAwait(false);

                if (response.IsSuccess)
                    return response;

                var bodyText = response.BodyText ?? string.Empty;

                // V4→V1 降级
                if (allowV4Downgrade && _account is CloudAccount cloud && cloud.CanDowngradeToV1 && IsV4Rejected(response.StatusCode, bodyText))
                {
                    _logger?.LogWarning("ODPS V4 signature rejected (status={Status}), retrying with V1. Response: {Body}", response.StatusCode, bodyText);
                    cloud.SignWithV1(request);
                    return await SendInternalAsync(request, allowV4Downgrade: false, cancellationToken).ConfigureAwait(false);
                }

                // 重试
                if (RetryableStatusCodes.Contains(response.StatusCode) && IsRetryableMethod(request.Method) && attempt < attempts - 1)
                {
                    lastException = BuildException(response.StatusCode, bodyText);
                    _logger?.LogWarning("ODPS request failed (status={Status}), retrying {Attempt}/{Total}. Body: {Body}", response.StatusCode, attempt + 1, attempts, bodyText);
                    await Task.Delay(CalculateBackoff(attempt), cancellationToken).ConfigureAwait(false);
                    continue;
                }

                throw BuildException(response.StatusCode, bodyText);
            }
            catch (OdpsException)
            {
                throw;
            }
            catch (HttpRequestException ex) when (IsRetryableMethod(request.Method) && attempt < attempts - 1)
            {
                lastException = ex;
                _logger?.LogWarning(ex, "HTTP request failed, retrying {Attempt}/{Total}", attempt + 1, attempts);
                await Task.Delay(CalculateBackoff(attempt), cancellationToken).ConfigureAwait(false);
            }
        }

        throw lastException ?? new OdpsException("ODPS request failed after retries", 0);
    }

    private async Task<OdpsResponse> SendCoreAsync(OdpsRequest request, CancellationToken cancellationToken)
    {
        _account.Sign(request);

        using var httpMessage = new HttpRequestMessage(new HttpMethod(request.Method), BuildUrl(request));

        // 注入头（HttpClient 会自动加上 User-Agent）
        foreach (var (key, value) in request.Headers)
        {
            if (string.Equals(key, "Content-Type", StringComparison.OrdinalIgnoreCase))
            {
                httpMessage.Content ??= new ByteArrayContent(Array.Empty<byte>());
                httpMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(value);
            }
            else if (!httpMessage.Headers.TryAddWithoutValidation(key, value))
            {
                httpMessage.Content ??= new ByteArrayContent(Array.Empty<byte>());
                httpMessage.Content?.Headers.TryAddWithoutValidation(key, value);
            }
        }

        if (request.Body is { Length: > 0 } body)
        {
            httpMessage.Content ??= new ByteArrayContent(Array.Empty<byte>());
            // 用 ByteArrayContent 替换现有空 content（保留上面的 headers）
            var newContent = new ByteArrayContent(body);
            foreach (var h in httpMessage.Content.Headers)
                newContent.Headers.TryAddWithoutValidation(h.Key, h.Value);
            httpMessage.Content = newContent;
        }

        using var httpResponse = await _httpClient.SendAsync(httpMessage, cancellationToken).ConfigureAwait(false);

        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var h in httpResponse.Headers)
            headers[h.Key] = string.Join(",", h.Value);
        foreach (var h in httpResponse.Content.Headers)
            headers[h.Key] = string.Join(",", h.Value);

        var bodyBytes = await httpResponse.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
        var bodyText = bodyBytes.Length > 0 ? Encoding.UTF8.GetString(bodyBytes) : string.Empty;

        return new OdpsResponse
        {
            StatusCode = (int)httpResponse.StatusCode,
            Headers = headers,
            BodyBytes = bodyBytes,
            BodyText = bodyText
        };
    }

    private string BuildUrl(OdpsRequest request)
    {
        var url = new StringBuilder(_endpoint).Append(request.Path);

        if (request.Query.Count > 0)
        {
            url.Append('?');
            for (var i = 0; i < request.Query.Count; i++)
            {
                if (i > 0) url.Append('&');
                var (key, value) = request.Query[i];
                url.Append(Uri.EscapeDataString(key));
                if (value.Length > 0)
                {
                    url.Append('=').Append(Uri.EscapeDataString(value));
                }
            }
        }

        return url.ToString();
    }

    private static bool IsRetryableMethod(string method) => method is "GET" or "HEAD" or "DELETE";

    private static TimeSpan CalculateBackoff(int attempt)
    {
        var seconds = Math.Min(120, 0.1 * Math.Pow(2, attempt));
        return TimeSpan.FromSeconds(seconds);
    }

    private static bool IsV4Rejected(int statusCode, string body)
    {
        if (statusCode < 400 || statusCode == 502 || statusCode == 503 || statusCode == 504)
            return false;

        foreach (var marker in V4RejectedMarkers)
        {
            if (body.IndexOf(marker, StringComparison.OrdinalIgnoreCase) >= 0)
                return true;
        }
        return false;
    }

    private static OdpsException BuildException(int statusCode, string body)
    {
        var error = OdpsError.TryParse(statusCode, body);
        return new OdpsException(
            error?.Message ?? body,
            statusCode,
            error?.Code,
            error?.RequestId,
            error?.HostId);
    }
}

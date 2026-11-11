using Azrng.AspNetCore.Core.AuditLog;
using Azrng.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Azrng.AspNetCore.Core.Middleware;

/// <summary>
/// 审计日志中间件
/// </summary>
public class AuditLogMiddleware
{
    private readonly RequestDelegate _next;
    private readonly AuditLogOptions _options;

    public AuditLogMiddleware(RequestDelegate next, IOptions<AuditLogOptions>? options = null)
    {
        _next = next;
        _options = options?.Value ?? new AuditLogOptions();
    }

    public async Task Invoke(HttpContext context)
    {
        var configuration = context.RequestServices.GetRequiredService<IConfiguration>();
        var request = context.Request;
        var path = request.Path.ToString();

        #region 前置检查

        // 检查是否应该记录日志
        if (!ShouldLogRequest(request, path))
        {
            await _next(context);
            return;
        }

        // 检查是否需要忽略的路由
        if (_options.IgnoreRoutePrefix.Count > 0)
        {
            if (_options.IgnoreRoutePrefix.Any(p => path.Contains(p, StringComparison.CurrentCultureIgnoreCase)))
            {
                await _next(context);
                return;
            }
        }

        #endregion

        #region 记录请求日志相关信息

        var stopwatch = Stopwatch.StartNew();
        var startTime = DateTime.Now;
        var respBody = string.Empty;

        // 敏感头脱敏后再序列化，避免 Authorization/Cookie 等凭证进入日志
        var reqHeaders = request.Headers.ToDictionary(
            x => x.Key,
            x => IsSensitiveHeader(x.Key)
                ? _options.SensitiveHeaderMask
                : string.Join(";", x.Value.ToList()));

        var reqBody = await GetRequestBodyAsync(request);

        // 响应完成回调内统一计算 EndTime/Elapsed，避免与 finally 的赋值顺序产生竞态
        context.Response.OnCompleted(() =>
        {
            stopwatch.Stop();
            var endTime = DateTime.Now;

            try
            {
                var serviceName = configuration.GetValue<string>("ServiceName") ?? "CommonService";

                // 获取日志服务
                var loggerService = context.RequestServices.GetService<ILoggerService>();
                if (loggerService == null)
                {
                    var loggerFactory = context.RequestServices.GetService<ILoggerFactory>();
                    var jsonSerializer = context.RequestServices.GetService<IJsonSerializer>();
                    loggerService = new DefaultLoggerService(loggerFactory, jsonSerializer);
                }

                loggerService.Write(new AuditLogInfo
                {
                    ServiceName = serviceName,
                    TraceId = Activity.Current != null ? Activity.Current.TraceId.ToString() : context.TraceIdentifier,
                    ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
                    StartTime = startTime,
                    EndTime = endTime,
                    LogLevel = context.Response.StatusCode == 200 ? LogLevel.Information : LogLevel.Error,
                    Route = request.Path,
                    HttpMethod = request.Method,
                    RequestBody = reqBody,
                    ResponseBody = respBody,
                    RawData = JsonSerializer.Serialize(reqHeaders),
                    StatusCode = context.Response.StatusCode,
                    UserId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                    UserName = context.User.FindFirst(ClaimTypes.Name)?.Value,
                    UserAgent = request.Headers["User-Agent"],
                    IpAddress = request.Headers["X-Real-IP"],
                    AliasName = "AuditLog"
                });
            }
            catch (Exception)
            {
                // 兜底：日志写入失败不应影响已完成的响应连接
            }
            return Task.CompletedTask;
        });

        var originalBodyStream = context.Response.Body;
        // 捕获流写通响应并限量捕获日志文本，替换整包 MemoryStream 缓冲，避免大响应整体驻留内存
        using var captureStream = new BodyCaptureStream(originalBodyStream, _options.MaxResponseBodySize);
        context.Response.Body = captureStream;

        try
        {
            await _next(context);

            respBody = FormatCapturedResponseBody(captureStream);
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }

        #endregion
    }

    /// <summary>
    /// 判断是否应该记录请求日志
    /// </summary>
    private bool ShouldLogRequest(HttpRequest request, string path)
    {
        // 检查是否只记录 API 路由
        if (_options.LogOnlyApiRoutes &&
            !path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        // 检查 HTTP 方法
        if (_options.IncludeHttpMethods.Count > 0)
        {
            return _options.IncludeHttpMethods.Contains(
                request.Method.ToUpperInvariant(),
                StringComparer.OrdinalIgnoreCase);
        }

        return true;
    }

    /// <summary>
    /// 获取请求体
    /// </summary>
    private async Task<string> GetRequestBodyAsync(HttpRequest request)
    {
        return request.Method.ToUpperInvariant() switch
        {
            "POST" or "PUT" or "PATCH" => await ReadRequestBodyAsync(request),
            "GET" or "DELETE" => request.QueryString.HasValue ? request.QueryString.Value! : "",
            _ => ""
        };
    }

    /// <summary>
    /// 读取请求体：multipart/二进制内容跳过，长度受 MaxRequestBodySize 限制
    /// </summary>
    private async Task<string> ReadRequestBodyAsync(HttpRequest request)
    {
        if (IsBinaryContent(request))
        {
            return "[二进制内容，不记录请求体]";
        }

        try
        {
            request.EnableBuffering();
            request.Body.Position = 0;

            using var reader = new StreamReader(
                request.Body,
                leaveOpen: true);

            var (body, truncated) = await ReadCappedAsync(reader, _options.MaxRequestBodySize);
            request.Body.Position = 0;

            return truncated
                ? $"[请求体过大，仅记录前 {_options.MaxRequestBodySize} 字符]\n" + FormatBody(body)
                : FormatBody(body);
        }
        catch
        {
            return "[无法读取请求体]";
        }
    }

    /// <summary>
    /// 从捕获流生成日志响应体文本（UTF-8 解码，超限标注截断）
    /// </summary>
    private string FormatCapturedResponseBody(BodyCaptureStream stream)
    {
        try
        {
            var body = Encoding.UTF8.GetString(stream.CapturedBytes);
            return stream.CaptureTruncated
                ? $"[响应体过大，仅记录前 {_options.MaxResponseBodySize} 字符]\n" + FormatBody(body)
                : FormatBody(body);
        }
        catch
        {
            return "[无法读取响应体]";
        }
    }

    /// <summary>
    /// 判断请求头是否在敏感头名单中（不区分大小写）
    /// </summary>
    private bool IsSensitiveHeader(string headerName)
    {
        return _options.SensitiveHeaders.Contains(headerName, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 判断是否为不记录正文的二进制/多部分请求
    /// </summary>
    private static bool IsBinaryContent(HttpRequest request)
    {
        var contentType = request.ContentType;
        return contentType != null &&
               (contentType.StartsWith("multipart/", StringComparison.OrdinalIgnoreCase)
                || contentType.StartsWith("application/octet-stream", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// 限量读取文本流：最多读取 maxChars 字符，返回内容与是否发生截断
    /// </summary>
    private static async Task<(string Body, bool Truncated)> ReadCappedAsync(StreamReader reader, int maxChars)
    {
        var builder = new StringBuilder();
        var buffer = new char[4096];
        int read;
        while ((read = await reader.ReadBlockAsync(buffer, 0, buffer.Length)) > 0)
        {
            var remaining = maxChars - builder.Length;
            if (remaining <= 0)
                return (builder.ToString(), true);

            var take = Math.Min(read, remaining);
            builder.Append(buffer, 0, take);
            if (take < read)
                return (builder.ToString(), true);
        }

        return (builder.ToString(), false);
    }

    /// <summary>
    /// 响应体捕获流：写通底层响应流，仅捕获前 maxCaptureBytes 字节用于日志，避免大响应整包驻留内存
    /// </summary>
    private sealed class BodyCaptureStream : Stream
    {
        private readonly Stream _inner;
        private readonly int _maxCaptureBytes;
        private readonly MemoryStream _capture = new();

        public BodyCaptureStream(Stream inner, int maxCaptureBytes)
        {
            _inner = inner;
            _maxCaptureBytes = maxCaptureBytes;
        }

        /// <summary>
        /// 已捕获的字节
        /// </summary>
        public byte[] CapturedBytes => _capture.ToArray();

        /// <summary>
        /// 捕获是否因超过上限被截断
        /// </summary>
        public bool CaptureTruncated { get; private set; }

        public override bool CanRead => false;

        public override bool CanSeek => _inner.CanSeek;

        public override bool CanWrite => true;

        public override long Length => _inner.Length;

        public override long Position { get => _inner.Position; set => _inner.Position = value; }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _inner.Write(buffer, offset, count);
            Capture(buffer.AsSpan(offset, count));
        }

        public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            await _inner.WriteAsync(buffer, cancellationToken);
            Capture(buffer.Span);
        }

        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            await _inner.WriteAsync(buffer, offset, count, cancellationToken);
            Capture(buffer.AsSpan(offset, count));
        }

        public override void Flush() => _inner.Flush();

        public override async Task FlushAsync(CancellationToken cancellationToken)
        {
            await _inner.FlushAsync(cancellationToken);
        }

        public override long Seek(long offset, SeekOrigin origin) => _inner.Seek(offset, origin);

        public override void SetLength(long value) => _inner.SetLength(value);

        public override int Read(byte[] buffer, int offset, int count)
        {
            // 审计中间件包装的是下游的响应写入流，不存在读取场景
            throw new NotSupportedException();
        }

        private void Capture(ReadOnlySpan<byte> span)
        {
            if (span.IsEmpty)
                return;

            var remaining = (int)(_maxCaptureBytes - _capture.Length);
            if (remaining <= 0)
            {
                // 捕获已满且仍有新数据写入，标记截断
                CaptureTruncated = true;
                return;
            }

            if (span.Length <= remaining)
            {
                _capture.Write(span);
            }
            else
            {
                _capture.Write(span[..remaining]);
                CaptureTruncated = true;
            }
        }
    }

    /// <summary>
    /// 格式化请求/响应体
    /// </summary>
    private string FormatBody(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
            return body;

        // 如果启用格式化且是有效的 JSON，尝试美化输出
        if (_options.FormatJson)
        {
            var trimmed = body.TrimStart();
            if (trimmed.StartsWith('{') || trimmed.StartsWith('['))
            {
                try
                {
                    var jsonDoc = JsonDocument.Parse(body);
                    return JsonSerializer.Serialize(jsonDoc, new JsonSerializerOptions
                    {
                        WriteIndented = true
                    });
                }
                catch
                {
                    // 不是有效的 JSON，返回原始内容
                    return body;
                }
            }
        }

        return body;
    }
}

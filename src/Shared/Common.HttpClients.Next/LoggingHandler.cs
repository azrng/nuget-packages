using Common.HttpClients.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Common.HttpClients
{
    /// <summary>
    /// HTTP 请求日志处理器，按命名客户端的 <see cref="HttpClientOptions"/> 记录请求/响应审计日志并执行敏感信息脱敏。
    /// </summary>
    public class LoggingHandler : DelegatingHandler
    {
        private readonly string _clientName;
        private readonly ILogger<LoggingHandler> _logger;
        private readonly IOptionsMonitor<HttpClientOptions> _optionsMonitor;
        private readonly IHttpContextAccessor? _httpContextAccessor;
        private readonly IHttpLogRedactor? _customRedactor;

        /// <summary>
        /// 缓存 options 快照对应的默认 redactor，避免每次请求重新构建敏感集合
        /// </summary>
        private static readonly ConditionalWeakTable<HttpClientOptions, DefaultHttpLogRedactor> DefaultRedactorCache = new();

        /// <summary>
        /// 初始化 <see cref="LoggingHandler"/> 的新实例
        /// </summary>
        /// <param name="clientName">命名客户端名称</param>
        /// <param name="logger">日志记录器</param>
        /// <param name="optionsMonitor">命名 options 监视器</param>
        /// <param name="httpContextAccessor">HTTP 上下文访问器（可选）</param>
        /// <param name="logRedactor">用户自定义日志脱敏器（可选，未注册时使用默认实现）</param>
        public LoggingHandler(string clientName,
                              ILogger<LoggingHandler> logger,
                              IOptionsMonitor<HttpClientOptions> optionsMonitor,
                              IHttpContextAccessor? httpContextAccessor = null,
                              IHttpLogRedactor? logRedactor = null)
        {
            _clientName = clientName ?? throw new ArgumentNullException(nameof(clientName));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _optionsMonitor = optionsMonitor ?? throw new ArgumentNullException(nameof(optionsMonitor));
            _httpContextAccessor = httpContextAccessor;
            _customRedactor = logRedactor;
        }

        /// <summary>
        /// 发送 HTTP 请求并记录审计日志
        /// </summary>
        /// <param name="request">HTTP 请求消息</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>HTTP 响应消息</returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var options = _optionsMonitor.Get(_clientName);
            var skipLogging = !options.AuditLog || ShouldSkipLogging(request);
            var startTime = DateTime.UtcNow;
            var traceId = AddOrGetTraceId(request);

            if (!skipLogging)
            {
                LogRequestStart(request, traceId);
            }

            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

            if (!skipLogging)
            {
                await LogAuditAsync(request, response, startTime, traceId, options).ConfigureAwait(false);
            }

            return response;
        }

        /// <summary>
        /// 添加或获取追踪 ID
        /// </summary>
        private string AddOrGetTraceId(HttpRequestMessage request)
        {
            string? traceId = null;

            if (request.Headers.TryGetValues(HttpClientHeaderNames.TraceId, out var traceIds))
            {
                traceId = traceIds.FirstOrDefault();
            }

            if (string.IsNullOrEmpty(traceId) && _httpContextAccessor?.HttpContext != null)
            {
                var httpContext = _httpContextAccessor.HttpContext;

                if (httpContext.Request.Headers.TryGetValue(HttpClientHeaderNames.TraceId, out var contextTraceId))
                {
                    traceId = contextTraceId.FirstOrDefault();
                }

                if (string.IsNullOrEmpty(traceId))
                {
                    traceId = httpContext.TraceIdentifier;
                }
            }

            if (string.IsNullOrEmpty(traceId))
            {
                traceId = Guid.NewGuid().ToString("N");
            }

            if (!request.Headers.Contains(HttpClientHeaderNames.TraceId))
            {
                request.Headers.Add(HttpClientHeaderNames.TraceId, traceId);
            }

            return traceId;
        }

        /// <summary>
        /// 记录请求开始日志
        /// </summary>
        private void LogRequestStart(HttpRequestMessage request, string traceId)
        {
            _logger.LogInformation("Http请求开始 TraceId:{TraceId} Url:{Url} Method:{Method}",
                traceId, request.RequestUri, request.Method);
        }

        /// <summary>
        /// 记录完整的审计日志（包括请求和响应）
        /// </summary>
        private async Task LogAuditAsync(HttpRequestMessage request,
                                          HttpResponseMessage response,
                                          DateTime startTime,
                                          string traceId,
                                          HttpClientOptions options)
        {
            try
            {
                var redactor = GetRedactor(options);

                var reqHeader = ReadRequestHeader(request, redactor, options);
                var reqContent = TruncateContent(
                    await ReadRequestContentAsync(request, redactor, options).ConfigureAwait(false),
                    options.MaxRequestBodyLength);

                var respHeader = ReadResponseHeader(response, redactor, options);
                var respContent = TruncateContent(
                    await ReadResponseContentAsync(request, response, redactor, options).ConfigureAwait(false),
                    options.MaxOutputResponseLength);
                var statusCode = response.StatusCode.ToString();
                var elapsed = DateTime.UtcNow - startTime;

                _logger.LogInformation(
                    "Http请求审计日志.TraceId:{TraceId} Url:{Url} Method:{Method} StatusCode:{StatusCode} 耗时:{ElapsedMs}ms\n" +
                    "RequestHeader:{RequestHeader}\nRequestContent:{RequestContent}\n" +
                    "ResponseHeader:{ResponseHeader}\nResponseContent:{ResponseContent}",
                    traceId, request.RequestUri, request.Method, statusCode, elapsed.TotalMilliseconds,
                    reqHeader, reqContent, respHeader, respContent);
            }
            catch (Exception logEx)
            {
                _logger.LogWarning(logEx, "记录 Http 审计日志时发生错误");
            }
        }

        /// <summary>
        /// 获取当前 options 对应的脱敏器。用户注册了自定义 redactor 时优先使用，否则使用默认 redactor（按 options 缓存）。
        /// </summary>
        private IHttpLogRedactor GetRedactor(HttpClientOptions options)
        {
            if (_customRedactor != null)
            {
                return _customRedactor;
            }

            return DefaultRedactorCache.GetValue(options, static opt => new DefaultHttpLogRedactor(opt));
        }

        /// <summary>
        /// 判断是否应该跳过日志记录
        /// </summary>
        private static bool ShouldSkipLogging(HttpRequestMessage request)
        {
            if (request.Headers.Contains(HttpClientHeaderNames.SkipLogger))
            {
                return true;
            }

            if (request.Headers.TryGetValues(HttpClientHeaderNames.Logger, out var values))
            {
                var level = values.FirstOrDefault()?.ToLowerInvariant();
                if (level == "none" || level == "skip")
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 截断过长的内容
        /// </summary>
        private static string? TruncateContent(string? content, int limit)
        {
            if (string.IsNullOrEmpty(content))
            {
                return content;
            }

            if (limit > 0 && content.Length > limit)
            {
                var truncatedContent = content.Substring(0, limit);
                return $"{truncatedContent}... [内容已截断，总长度：{content.Length} 字符]";
            }

            return content;
        }

        /// <summary>
        /// 读取请求内容并应用脱敏
        /// </summary>
        private async Task<string?> ReadRequestContentAsync(HttpRequestMessage request, IHttpLogRedactor redactor, HttpClientOptions options)
        {
            try
            {
                var content = request.Content;
                if (content == null)
                {
                    return null;
                }

                if (content is MultipartFormDataContent)
                {
                    return "[multipart form-data content skipped]";
                }

                var contentStr = await content.ReadAsStringAsync().ConfigureAwait(false);
                return options.EnableLogRedaction ? redactor.RedactContent(contentStr) : contentStr;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "读取请求内容失败");
                return "filter_read_request_content_error";
            }
        }

        /// <summary>
        /// 读取响应内容并应用脱敏
        /// </summary>
        private async Task<string?> ReadResponseContentAsync(HttpRequestMessage request,
                                                               HttpResponseMessage response,
                                                               IHttpLogRedactor redactor,
                                                               HttpClientOptions options)
        {
            try
            {
                if (request.Options.TryGetValue(HttpClientRequestOptionKeys.SkipResponseBodyAudit, out var skipBodyAudit) &&
                    skipBodyAudit)
                {
                    return "[response body skipped for streaming request]";
                }

                var content = response.Content;
                if (content == null)
                {
                    return null;
                }

                if (IsBinaryMediaType(content.Headers.ContentType?.MediaType))
                {
                    return string.Empty;
                }

                var str = await content.ReadAsStringAsync().ConfigureAwait(false);
                return options.EnableLogRedaction ? redactor.RedactContent(str) : str;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "读取响应内容失败");
                return "filter_read_response_content_error";
            }
        }

        /// <summary>
        /// 读取请求头并应用脱敏
        /// </summary>
        private string ReadRequestHeader(HttpRequestMessage request, IHttpLogRedactor redactor, HttpClientOptions options)
        {
            try
            {
                var headerMap = CollectHeaders(request.Headers, request.Content?.Headers);
                var redacted = options.EnableLogRedaction ? redactor.RedactHeaders(headerMap) : headerMap;
                return JsonHelper.ToJson(redacted);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "读取请求头失败");
                return "filter_read_request_header_error";
            }
        }

        /// <summary>
        /// 读取响应头并应用脱敏
        /// </summary>
        private string ReadResponseHeader(HttpResponseMessage response, IHttpLogRedactor redactor, HttpClientOptions options)
        {
            try
            {
                var headerMap = CollectHeaders(response.Headers, response.Content?.Headers);
                var redacted = options.EnableLogRedaction ? redactor.RedactHeaders(headerMap) : headerMap;
                return JsonHelper.ToJson(redacted);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "读取响应头失败");
                return "filter_read_response_header_error";
            }
        }

        private static Dictionary<string, string> CollectHeaders(IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers,
                                                                  IEnumerable<KeyValuePair<string, IEnumerable<string>>>? contentHeaders)
        {
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var header in headers)
            {
                map[header.Key] = string.Join(",", header.Value);
            }

            if (contentHeaders != null)
            {
                foreach (var header in contentHeaders)
                {
                    map[header.Key] = string.Join(",", header.Value);
                }
            }

            return map;
        }

        /// <summary>
        /// 判断 Content-Type 是否为二进制格式
        /// </summary>
        private static bool IsBinaryMediaType(string? mediaType)
        {
            if (string.IsNullOrWhiteSpace(mediaType))
            {
                return false;
            }

            return mediaType.StartsWith("image/", StringComparison.OrdinalIgnoreCase)
                   || mediaType.StartsWith("video/", StringComparison.OrdinalIgnoreCase)
                   || mediaType.StartsWith("audio/", StringComparison.OrdinalIgnoreCase)
                   || mediaType.Contains("octet-stream", StringComparison.OrdinalIgnoreCase)
                   || mediaType.Contains("pdf", StringComparison.OrdinalIgnoreCase)
                   || mediaType.Contains("zip", StringComparison.OrdinalIgnoreCase)
                   || mediaType.Contains("gzip", StringComparison.OrdinalIgnoreCase)
                   || mediaType.Contains("rar", StringComparison.OrdinalIgnoreCase)
                   || mediaType.Contains("7z", StringComparison.OrdinalIgnoreCase)
                   || mediaType.Contains("tar", StringComparison.OrdinalIgnoreCase);
        }
    }
}

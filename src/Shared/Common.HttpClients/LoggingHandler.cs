using Common.HttpClients.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Common.HttpClients
{
    /// <summary>
    /// 日志处理
    /// </summary>
    public class LoggingHandler : DelegatingHandler
    {
        private static readonly HashSet<string> SensitiveHeaderNames = new(StringComparer.OrdinalIgnoreCase)
                                                                       {
                                                                           "Authorization",
                                                                           "Proxy-Authorization",
                                                                           "Cookie",
                                                                           "Set-Cookie",
                                                                           "X-Api-Key",
                                                                           "Api-Key",
                                                                           "X-Auth-Token"
                                                                       };

        private static readonly Regex JsonSensitiveValuePattern = new(
            "(\"(?:password|passwd|pwd|secret|token|access_token|refresh_token|client_secret|api[_-]?key)\"\\s*:\\s*\")([^\"]*)(\")",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex KvSensitiveValuePattern = new(
            "\\b(password|passwd|pwd|secret|token|access_token|refresh_token|client_secret|api[_-]?key)=([^&\\s]+)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex BearerValuePattern = new(
            "(Bearer\\s+)[A-Za-z0-9\\-._~+/]+=*",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private readonly ILogger<LoggingHandler> _logger;
        private readonly HttpClientOptions _httpConfig;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LoggingHandler(ILogger<LoggingHandler> logger, IOptions<HttpClientOptions> options,
                              IHttpContextAccessor httpContextAccessor = null)
        {
            _logger = logger;
            _httpConfig = options.Value;
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var startTime = DateTime.UtcNow;
            var traceId = AddOrGetTraceId(request);
            LogRequestStart(request, traceId);

            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            await LogAuditAsync(request, response, startTime, traceId).ConfigureAwait(false);
            return response;
        }

        /// <summary>
        /// 添加或获取追踪ID
        /// </summary>
        private string AddOrGetTraceId(HttpRequestMessage request)
        {
            string traceId = null;

            // 1. 首先尝试从请求头中获取
            if (request.Headers.TryGetValues("X-Trace-Id", out var traceIds))
            {
                traceId = traceIds.FirstOrDefault();
            }

            // 2. 如果没有，尝试从HttpContext中获取
            if (string.IsNullOrEmpty(traceId) && _httpContextAccessor?.HttpContext != null)
            {
                var httpContext = _httpContextAccessor.HttpContext;

                // 尝试从当前请求头中获取
                if (httpContext.Request.Headers.TryGetValue("X-Trace-Id", out var contextTraceId))
                {
                    traceId = contextTraceId.FirstOrDefault();
                }

                if (string.IsNullOrEmpty(traceId))
                {
                    traceId = httpContext.TraceIdentifier;
                }
            }

            // 3. 如果还是没有，生成一个新的
            if (string.IsNullOrEmpty(traceId))
            {
                traceId = Guid.NewGuid().ToString("N");
            }

            // 4. 将追踪ID添加到请求头中
            if (!request.Headers.Contains("X-Trace-Id"))
            {
                request.Headers.Add("X-Trace-Id", traceId);
            }

            return traceId;
        }

        /// <summary>
        /// 记录请求开始日志
        /// </summary>
        private void LogRequestStart(HttpRequestMessage request, string traceId)
        {
            try
            {
                if (!_httpConfig.AuditLog || ShouldSkipLogging(request))
                {
                    return;
                }

                _logger.LogInformation("Http请求开始 TraceId:{TraceId} Url:{Url} Method:{Method}",
                    traceId, request.RequestUri, request.Method);
            }
            catch (Exception logEx)
            {
                _logger.LogError(logEx, "记录Http请求开始日志时发生错误");
            }
        }

        private async Task LogAuditAsync(HttpRequestMessage request, HttpResponseMessage response, DateTime startTime, string traceId)
        {
            try
            {
                if (!_httpConfig.AuditLog || ShouldSkipLogging(request))
                {
                    return;
                }

                var reqHeader = ReadRequestHeader(request);
                var reqContent = await ReadRequestContentAsync(request).ConfigureAwait(false);

                var respHeader = ReadResponseHeader(response);
                var respContent = await ReadResponseContentAsync(response).ConfigureAwait(false);
                var statusCode = response.StatusCode.ToString();
                var finalRespContent = TruncateResponseContent(respContent);
                var elapsed = DateTime.UtcNow - startTime;

                _logger.LogInformation(
                    "Http请求审计日志.TraceId:{TraceId} Url:{Url} Method:{Method} StatusCode:{StatusCode} 耗时:{ElapsedMs}ms\n" +
                    "RequestHeader:{RequestHeader}\nRequestContent:{RequestContent}\n" +
                    "ResponseHeader:{ResponseHeader}\nResponseContent:{ResponseContent}",
                    traceId, request.RequestUri, request.Method, statusCode, elapsed.TotalMilliseconds,
                    reqHeader, reqContent, respHeader, finalRespContent);
            }
            catch (Exception logEx)
            {
                // 记录日志失败，避免影响主流程
                _logger.LogError(logEx, "记录Http审计日志时发生错误");
            }
        }

        private bool ShouldSkipLogging(HttpRequestMessage request)
        {
            if (request.Headers.Contains("X-Skip-Logger"))
            {
                return true;
            }

            if (request.Headers.TryGetValues("X-Logger", out var values))
            {
                var level = values.FirstOrDefault()?.ToLowerInvariant();
                if (level == "none" || level == "skip")
                {
                    return true;
                }
            }

            return false;
        }

        private string TruncateResponseContent(string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                return content;
            }

            if (_httpConfig.MaxOutputResponseLength > 0 && content.Length > _httpConfig.MaxOutputResponseLength)
            {
                var truncatedContent = content.Substring(0, _httpConfig.MaxOutputResponseLength);
                return $"{truncatedContent}... [内容已截断，总长度：{content.Length} 字符]";
            }

            return content;
        }

        private async Task<string> ReadRequestContentAsync(HttpRequestMessage context)
        {
            try
            {
                var content = context.Content;
                if (content == null)
                {
                    return null;
                }

                if (content is MultipartFormDataContent)
                {
                    return "...";
                }

                var contentStr = await content.ReadAsStringAsync().ConfigureAwait(false);
                return ApplyRedaction(contentStr);
            }
            catch (Exception)
            {
                return "filter_read_request_content_error";
            }
        }

        private async Task<string> ReadResponseContentAsync(HttpResponseMessage context)
        {
            try
            {
                var content = context.Content;
                if (content == null)
                {
                    return null;
                }

                var headerValue = string.Join("", content.Headers.SelectMany(t => t.Value).ToList());
                if (ContainsBinaryContentType(headerValue))
                {
                    return string.Empty;
                }

                var str = await content.ReadAsStringAsync().ConfigureAwait(false);
                str = ApplyRedaction(str);

                try
                {
                    return Regex.Unescape(str);
                }
                catch (Exception)
                {
                    return str;
                }
            }
            catch (Exception)
            {
                return "filter_read_response_content_error";
            }
        }

        private string ReadRequestHeader(HttpRequestMessage context)
        {
            try
            {
                var headerMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                foreach (var header in context.Headers)
                {
                    headerMap[header.Key] = string.Join(",", header.Value);
                }

                if (context.Content != null)
                {
                    foreach (var header in context.Content.Headers)
                    {
                        headerMap[header.Key] = string.Join(",", header.Value);
                    }
                }

                return JsonHelper.ToJson(RedactHeaders(headerMap));
            }
            catch (Exception)
            {
                return "filter_read_request_header_error";
            }
        }

        private string ReadResponseHeader(HttpResponseMessage context)
        {
            try
            {
                var headerMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                if (context != null)
                {
                    foreach (var header in context.Headers)
                    {
                        headerMap[header.Key] = string.Join(",", header.Value);
                    }

                    if (context.Content != null)
                    {
                        foreach (var header in context.Content.Headers)
                        {
                            headerMap[header.Key] = string.Join(",", header.Value);
                        }
                    }
                }

                return JsonHelper.ToJson(RedactHeaders(headerMap));
            }
            catch (Exception)
            {
                return "filter_read_response_header_error";
            }
        }

        private IDictionary<string, string> RedactHeaders(IDictionary<string, string> headers)
        {
            if (!_httpConfig.EnableLogRedaction || headers == null || headers.Count == 0)
            {
                return headers;
            }

            var redacted = new Dictionary<string, string>(headers, StringComparer.OrdinalIgnoreCase);
            foreach (var key in redacted.Keys.ToList())
            {
                if (SensitiveHeaderNames.Contains(key))
                {
                    redacted[key] = "***";
                }
            }

            return redacted;
        }

        private string ApplyRedaction(string content)
        {
            if (!_httpConfig.EnableLogRedaction || string.IsNullOrEmpty(content))
            {
                return content;
            }

            var redacted = JsonSensitiveValuePattern.Replace(content, "$1***$3");
            redacted = KvSensitiveValuePattern.Replace(redacted, "$1=***");
            redacted = BearerValuePattern.Replace(redacted, "$1***");
            return redacted;
        }

        private static bool ContainsBinaryContentType(string headerValue)
        {
            if (string.IsNullOrWhiteSpace(headerValue))
            {
                return false;
            }

            return headerValue.Contains("image", StringComparison.OrdinalIgnoreCase) ||
                   headerValue.Contains("video", StringComparison.OrdinalIgnoreCase) ||
                   headerValue.Contains("audio", StringComparison.OrdinalIgnoreCase) ||
                   headerValue.Contains("octet-stream", StringComparison.OrdinalIgnoreCase) ||
                   headerValue.Contains("pdf", StringComparison.OrdinalIgnoreCase) ||
                   headerValue.Contains("rar", StringComparison.OrdinalIgnoreCase) ||
                   headerValue.Contains("7z", StringComparison.OrdinalIgnoreCase) ||
                   headerValue.Contains("tar", StringComparison.OrdinalIgnoreCase) ||
                   headerValue.Contains("gz", StringComparison.OrdinalIgnoreCase) ||
                   headerValue.Contains("zip", StringComparison.OrdinalIgnoreCase);
        }
    }
}

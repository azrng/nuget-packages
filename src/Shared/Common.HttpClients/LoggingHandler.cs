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
    /// HTTP请求日志处理器，负责记录请求/响应审计日志和敏感信息脱敏
    /// </summary>
    public class LoggingHandler : DelegatingHandler
    {
        private static readonly string[] DefaultSensitiveHeaderNames =
        {
            "Authorization",
            "Proxy-Authorization",
            "Cookie",
            "Set-Cookie",
            "X-Api-Key",
            "Api-Key",
            "X-Auth-Token"
        };

        private static readonly string[] DefaultSensitiveFieldNames =
        {
            "password",
            "passwd",
            "pwd",
            "secret",
            "token",
            "access_token",
            "refresh_token",
            "client_secret",
            "api_key",
            "api-key"
        };

        private static readonly Regex BearerValuePattern = new(
            "(Bearer\\s+)[A-Za-z0-9\\-._~+/]+=*",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private readonly ILogger<LoggingHandler> _logger;
        private readonly HttpClientOptions _httpConfig;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HashSet<string> _sensitiveHeaderNames;
        private readonly Regex _jsonSensitiveValuePattern;
        private readonly Regex _kvSensitiveValuePattern;

        /// <summary>
        /// 初始化 <see cref="LoggingHandler"/> 的新实例
        /// </summary>
        /// <param name="logger">日志记录器</param>
        /// <param name="options">HTTP配置选项</param>
        /// <param name="httpContextAccessor">HTTP上下文访问器（可选）</param>
        public LoggingHandler(ILogger<LoggingHandler> logger, IOptions<HttpClientOptions> options,
                              IHttpContextAccessor httpContextAccessor = null)
        {
            _logger = logger;
            _httpConfig = options.Value;
            _httpContextAccessor = httpContextAccessor;

            _sensitiveHeaderNames = BuildSensitiveHeaders(_httpConfig.AdditionalSensitiveHeaders);
            var sensitiveFieldPattern = BuildSensitiveFieldPattern(_httpConfig.AdditionalSensitiveFields);
            _jsonSensitiveValuePattern = new Regex(
                "(\"(?:" + sensitiveFieldPattern + ")\"\\s*:\\s*\")([^\"]*)(\")",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);
            _kvSensitiveValuePattern = new Regex(
                "\\b(" + sensitiveFieldPattern + ")=([^&\\s]+)",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }

        /// <summary>
        /// 发送HTTP请求并记录审计日志
        /// </summary>
        /// <param name="request">HTTP请求消息</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>HTTP响应消息</returns>
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
        /// <param name="request">HTTP请求消息</param>
        /// <param name="traceId">追踪ID</param>
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

        /// <summary>
        /// 记录完整的审计日志（包括请求和响应）
        /// </summary>
        /// <param name="request">HTTP请求消息</param>
        /// <param name="response">HTTP响应消息</param>
        /// <param name="startTime">请求开始时间</param>
        /// <param name="traceId">追踪ID</param>
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
                var respContent = await ReadResponseContentAsync(request, response).ConfigureAwait(false);
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

        /// <summary>
        /// 判断是否应该跳过日志记录
        /// </summary>
        /// <param name="request">HTTP请求消息</param>
        /// <returns>如果应该跳过日志则返回true</returns>
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

        /// <summary>
        /// 截断过长的响应内容
        /// </summary>
        /// <param name="content">原始内容</param>
        /// <returns>截断后的内容或原始内容</returns>
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

        /// <summary>
        /// 读取请求内容并应用脱敏
        /// </summary>
        /// <param name="context">HTTP请求消息</param>
        /// <returns>脱敏后的请求内容</returns>
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

        /// <summary>
        /// 读取响应内容并应用脱敏
        /// </summary>
        /// <param name="request">HTTP请求消息（用于检查跳过标志）</param>
        /// <param name="context">HTTP响应消息</param>
        /// <returns>脱敏后的响应内容</returns>
        private async Task<string> ReadResponseContentAsync(HttpRequestMessage request, HttpResponseMessage context)
        {
            try
            {
                if (request != null &&
                    request.Options.TryGetValue(HttpClientRequestOptionKeys.SkipResponseBodyAudit, out var skipBodyAudit) &&
                    skipBodyAudit)
                {
                    return "[response body skipped for streaming request]";
                }

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

        /// <summary>
        /// 读取请求头并应用脱敏
        /// </summary>
        /// <param name="context">HTTP请求消息</param>
        /// <returns>脱敏后的请求头JSON字符串</returns>
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

        /// <summary>
        /// 读取响应头并应用脱敏
        /// </summary>
        /// <param name="context">HTTP响应消息</param>
        /// <returns>脱敏后的响应头JSON字符串</returns>
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

        /// <summary>
        /// 对请求头进行脱敏处理
        /// </summary>
        /// <param name="headers">原始请求头字典</param>
        /// <returns>脱敏后的请求头字典</returns>
        private IDictionary<string, string> RedactHeaders(IDictionary<string, string> headers)
        {
            if (!_httpConfig.EnableLogRedaction || headers == null || headers.Count == 0)
            {
                return headers;
            }

            var redacted = new Dictionary<string, string>(headers, StringComparer.OrdinalIgnoreCase);
            foreach (var key in redacted.Keys.ToList())
            {
                if (_sensitiveHeaderNames.Contains(key))
                {
                    redacted[key] = "***";
                }
            }

            return redacted;
        }

        /// <summary>
        /// 对内容应用敏感信息脱敏
        /// </summary>
        /// <param name="content">原始内容</param>
        /// <returns>脱敏后的内容</returns>
        private string ApplyRedaction(string content)
        {
            if (!_httpConfig.EnableLogRedaction || string.IsNullOrEmpty(content))
            {
                return content;
            }

            var redacted = _jsonSensitiveValuePattern.Replace(content, "$1***$3");
            redacted = _kvSensitiveValuePattern.Replace(redacted, "$1=***");
            redacted = BearerValuePattern.Replace(redacted, "$1***");
            return redacted;
        }

        /// <summary>
        /// 构建敏感请求头集合
        /// </summary>
        /// <param name="additionalHeaders">额外的敏感请求头</param>
        /// <returns>包含默认和自定义敏感请求头的集合</returns>
        private static HashSet<string> BuildSensitiveHeaders(ICollection<string> additionalHeaders)
        {
            var result = new HashSet<string>(DefaultSensitiveHeaderNames, StringComparer.OrdinalIgnoreCase);
            if (additionalHeaders == null)
            {
                return result;
            }

            foreach (var header in additionalHeaders)
            {
                if (string.IsNullOrWhiteSpace(header))
                {
                    continue;
                }

                result.Add(header.Trim());
            }

            return result;
        }

        /// <summary>
        /// 构建敏感字段正则表达式模式
        /// </summary>
        /// <param name="additionalFields">额外的敏感字段名</param>
        /// <returns>用于匹配敏感字段的正则表达式模式</returns>
        private static string BuildSensitiveFieldPattern(ICollection<string> additionalFields)
        {
            var allFields = new HashSet<string>(DefaultSensitiveFieldNames, StringComparer.OrdinalIgnoreCase);
            if (additionalFields != null)
            {
                foreach (var field in additionalFields)
                {
                    if (string.IsNullOrWhiteSpace(field))
                    {
                        continue;
                    }

                    allFields.Add(field.Trim());
                }
            }

            var escaped = allFields.Select(Regex.Escape).ToArray();
            return escaped.Length == 0 ? "a^" : string.Join("|", escaped);
        }

        /// <summary>
        /// 检查内容类型是否为二进制格式
        /// </summary>
        /// <param name="headerValue">内容类型头值</param>
        /// <returns>如果是二进制内容类型则返回true</returns>
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

using Common.HttpClients.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
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
            // 记录请求开始时间
            var startTime = DateTime.UtcNow;

            // 添加或获取追踪ID
            var traceId = AddOrGetTraceId(request);

            // 记录请求开始日志
            LogRequestStart(request, traceId);

            // 调用基类的 SendAsync 方法发送请求
            var response = await base.SendAsync(request, cancellationToken);

            // 记录完整的审计日志（包含响应）
            await LogAuditAsync(request, response, startTime, traceId);

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
                    return;

                _logger.LogInformation($"Http请求开始.TraceId：{traceId} Url：{request.RequestUri} Method：{request.Method}");
            }
            catch (Exception logEx)
            {
                _logger.LogError(logEx, $"记录Http请求开始日志时发生错误 {logEx.Message}");
            }
        }

        /// <summary>
        /// 记录审计日志
        /// </summary>
        private async Task LogAuditAsync(HttpRequestMessage request, HttpResponseMessage response, DateTime startTime, string traceId)
        {
            try
            {
                // 检查是否启用审计日志
                if (!_httpConfig.AuditLog)
                    return;

                // 检查是否跳过日志记录
                if (ShouldSkipLogging(request))
                    return;

                var reqHeader = ReadRequestHeader(request);
                var reqContent = await ReadRequestContentAsync(request).ConfigureAwait(false);

                var respHeader = ReadResponseHeader(response);
                var respContent = await ReadResponseContentAsync(response).ConfigureAwait(false);
                var statusCode = response.StatusCode.ToString();

                // 检查响应内容长度，如果超过阈值则截断
                var finalRespContent = TruncateResponseContent(respContent);

                // 计算请求耗时
                var elapsed = DateTime.UtcNow - startTime;

                _logger.LogInformation(
                    $"Http请求审计日志.TraceId：{traceId} Url：{request.RequestUri} Method：{request.Method} StatusCode：{statusCode} 耗时：{elapsed.TotalMilliseconds}ms" +
                    $"{Environment.NewLine} RequestHeader：{reqHeader} " +
                    $"{Environment.NewLine} RequestContent：{reqContent} " +
                    $"{Environment.NewLine} ResponseHeader：{respHeader} " +
                    $"{Environment.NewLine} ResponseContent：{finalRespContent}");
            }
            catch (Exception logEx)
            {
                // 记录日志失败，避免影响主流程
                _logger.LogError(logEx, $"记录Http审计日志时发生错误 {logEx.Message}");
            }
        }

        /// <summary>
        /// 检查是否应该跳过日志记录
        /// </summary>
        /// <param name="request">HTTP请求</param>
        /// <returns>true表示跳过日志记录</returns>
        private bool ShouldSkipLogging(HttpRequestMessage request)
        {
            // 检查请求头中是否包含跳过日志的标记
            if (request.Headers.Contains("X-Skip-Logger"))
                return true;

            // 检查请求头中是否包含跳过日志的标记（值）
            if (request.Headers.TryGetValues("X-Logger", out var values))
            {
                var level = values.FirstOrDefault()?.ToLowerInvariant();
                if (level == "none" || level == "skip")
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 截断响应内容，避免日志过长
        /// </summary>
        /// <param name="content">原始响应内容</param>
        /// <returns>截断后的响应内容</returns>
        private string TruncateResponseContent(string content)
        {
            if (string.IsNullOrEmpty(content))
                return content;

            // 如果内容长度超过配置的最大长度，则截断
            if (_httpConfig.MaxOutputResponseLength > 0 && content.Length > _httpConfig.MaxOutputResponseLength)
            {
                var truncatedContent = content.Substring(0, _httpConfig.MaxOutputResponseLength);
                return $"{truncatedContent}... [内容已截断，总长度：{content.Length} 字符]";
            }

            return content;
        }

        /// <summary>
        /// 读取请求内容
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private async Task<string> ReadRequestContentAsync(HttpRequestMessage context)
        {
            try
            {
                var content = context.Content;
                if (content == null) return null;

                if (content is MultipartFormDataContent)
                    return "...";
                return await content.ReadAsStringAsync().ConfigureAwait(false);
            }
            catch (Exception)
            {
                return "filter_read_request_content_error";
            }
        }

        /// <summary>
        /// 读取响应内容
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private async Task<string> ReadResponseContentAsync(HttpResponseMessage context)
        {
            try
            {
                var content = context.Content;

                var headerValue = string.Join("", content.Headers.SelectMany(t => t.Value).ToList());

                if (headerValue.Contains("image") ||
                    headerValue.Contains("video") ||
                    headerValue.Contains("audio") ||
                    headerValue.Contains("octet-stream") ||
                    headerValue.Contains("pdf") ||
                    headerValue.Contains("rar") ||
                    headerValue.Contains("7z") ||
                    headerValue.Contains("tar") ||
                    headerValue.Contains("gz") ||
                    headerValue.Contains("zip"))
                    return string.Empty;

                var str = await content.ReadAsStringAsync().ConfigureAwait(false);

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
                return JsonHelper.ToJson(context.Headers);
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
                return JsonHelper.ToJson(context?.Headers);
            }
            catch (Exception)
            {
                return "filter_read_response_header_error";
            }
        }
    }
}
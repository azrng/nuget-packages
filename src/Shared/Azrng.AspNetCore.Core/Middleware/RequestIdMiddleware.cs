using Microsoft.AspNetCore.Http;

namespace Azrng.AspNetCore.Core.Middleware
{
    /// <summary>
    /// 请求Id传递中间件(将请求 ID 写入响应头 X-RequestId 并同步 TraceIdentifier，便于调用方与日志关联)
    /// </summary>
    public class RequestIdMiddleware
    {
        private readonly RequestDelegate _next;

        private const string RequestIdHeader = "X-RequestId";

        /// <summary>
        /// 请求 ID 最大长度，超长视为非法并丢弃，防止超长值进入日志
        /// </summary>
        private const int MaxRequestIdLength = 64;

        public RequestIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var requestId = ResolveRequestId(context);
            context.TraceIdentifier = requestId;
            context.Response.Headers[RequestIdHeader] = requestId;

            await _next(context);
        }

        /// <summary>
        /// 入站请求 ID 仅在通过合法性校验时沿用，否则回退到宿主已生成的 TraceIdentifier
        /// </summary>
        private static string ResolveRequestId(HttpContext context)
        {
            if (context.Request.Headers.TryGetValue(RequestIdHeader, out var header))
            {
                // 多值头只取第一个，避免 ToString 把多个值拼成 "a, b"
                var candidate = header.FirstOrDefault();

                if (IsValidRequestId(candidate))
                {
                    return candidate;
                }
            }

            // 沿用宿主生成的 TraceIdentifier（.NET 3.0+ 由 Activity 体系支撑），保持与诊断 traceId 一致
            return context.TraceIdentifier;
        }

        /// <summary>
        /// 请求 ID 白名单校验：字母数字与 - _ . 组合，防止任意字符串注入日志或伪造请求关联
        /// </summary>
        private static bool IsValidRequestId(string? requestId)
        {
            if (string.IsNullOrWhiteSpace(requestId) || requestId.Length > MaxRequestIdLength)
            {
                return false;
            }

            foreach (var c in requestId)
            {
                var isAllowed = c is (>= 'a' and <= 'z') or (>= 'A' and <= 'Z') or (>= '0' and <= '9') or '-' or '_' or '.';
                if (!isAllowed)
                {
                    return false;
                }
            }

            return true;
        }
    }
}

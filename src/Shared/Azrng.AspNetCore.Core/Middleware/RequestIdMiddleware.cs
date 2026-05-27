using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace Azrng.AspNetCore.Core.Middleware
{
    /// <summary>
    /// 请求Id传递中间件(会将入参请求头的请求ID原样返回到响应头中)
    /// </summary>
    public class RequestIdMiddleware
    {
        private readonly RequestDelegate _next;

        private const string _requestIdHeader = "X-RequestId";

        public RequestIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var requestId = GetRequestId(context);
            var requestIdFeature = context.Features.Get<IHttpRequestIdentifierFeature>();
            if (requestIdFeature != null)
            {
                requestIdFeature.TraceIdentifier = requestId;
            }

            context.TraceIdentifier = requestId;
            context.Response.Headers[_requestIdHeader] = requestId;

            await _next(context);
        }

        private static string GetRequestId(HttpContext context)
        {
            if (context.Request.Headers.TryGetValue(_requestIdHeader, out var header))
            {
                var requestId = header.ToString();
                if (!string.IsNullOrWhiteSpace(requestId))
                {
                    return requestId;
                }
            }

            return Guid.NewGuid().ToString("N");
        }
    }
}

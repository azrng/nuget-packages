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
            var requestIdFeature = context.Features.Get<IHttpRequestIdentifierFeature>();
            if (requestIdFeature?.TraceIdentifier != null)
            {
                if (context.Request.Headers.TryGetValue(_requestIdHeader, out var header))
                {
                    requestIdFeature.TraceIdentifier = header;
                }

                context.Response.Headers[_requestIdHeader] = requestIdFeature.TraceIdentifier;
            }

            await _next(context);
        }
    }
}
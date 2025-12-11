using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using System.IO.Compression;
using System.Text;

namespace Azrng.AspNetCore.Core.Extension
{
    /// <summary>
    /// Http 拓展类
    /// </summary>
    public static class HttpContextExtensions
    {
        /// <summary>
        /// 获取 Action 特性
        /// </summary>
        /// <typeparam name="TAttribute"></typeparam>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public static TAttribute? GetMetadata<TAttribute>(this HttpContext httpContext)
            where TAttribute : class
        {
            return httpContext.GetEndpoint()?.Metadata?.GetMetadata<TAttribute>();
        }

        /// <summary>
        /// 获取 控制器/Action 描述器
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public static ControllerActionDescriptor? GetControllerActionDescriptor(this HttpContext httpContext)
        {
            return httpContext.GetEndpoint()?.Metadata?.FirstOrDefault(u => u is ControllerActionDescriptor) as
                ControllerActionDescriptor;
        }

        /// <summary>
        /// 设置响应头 Tokens
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="accessToken"></param>
        /// <param name="refreshToken"></param>
        public static void SetTokensOfResponseHeaders(this HttpContext httpContext, string accessToken,
            string? refreshToken = null)
        {
            httpContext.Response.Headers["access-token"] = accessToken;
            if (!string.IsNullOrWhiteSpace(refreshToken))
            {
                httpContext.Response.Headers["x-access-token"] = refreshToken;
            }
        }

        /// <summary>
        /// 通过Key获取HttpContext.User.Claims中的信息
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="key">Claim关键字</param>
        /// <returns></returns>
        public static string? GetClaimByUser(this HttpContext httpContext, string key)
        {
            return httpContext.User.Claims.Where(t => t.Type == key).Select(t => t.Value).FirstOrDefault();
        }

        /// <summary>
        /// 获取完整URL信息
        /// </summary>
        /// <returns></returns>
        public static string GetUrl(this HttpRequest httpRequest)
        {
            return httpRequest.GetBaseUrl() + $"{httpRequest.Path}{httpRequest.QueryString}";
        }

        /// <summary>
        /// 获取基础URL信息
        /// </summary>
        /// <returns></returns>
        public static string GetBaseUrl(this HttpRequest httpRequest)
        {
            var url = $"{httpRequest.Scheme}://{httpRequest.Host.Host}";

            if (httpRequest.Host.Port != null)
            {
                url += $":{httpRequest.Host.Port}";
            }

            return url;
        }

        /// <summary>
        /// 获取本机 IPv4地址
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string? GetLocalIpAddressToIPv4(this HttpContext context)
        {
            return context.Connection.LocalIpAddress?.MapToIPv4()?.ToString();
        }

        /// <summary>
        /// 获取本机 IPv6地址
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string? GetLocalIpAddressToIPv6(this HttpContext context)
        {
            return context.Connection.LocalIpAddress?.MapToIPv6()?.ToString();
        }

        /// <summary>
        /// 获取远程 IPv4地址
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string? GetRemoteIpAddressToIPv4(this HttpContext context)
        {
            return context.Connection.RemoteIpAddress?.MapToIPv4()?.ToString();
        }

        /// <summary>
        /// 获取远程 IPv6地址
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string? GetRemoteIpAddressToIPv6(this HttpContext context)
        {
            return context.Connection.RemoteIpAddress?.MapToIPv6()?.ToString();
        }

        /// <summary>
        /// 获取来源地址
        /// </summary>
        /// <param name="request"></param>
        /// <param name="refererHeaderKey"></param>
        /// <returns></returns>
        public static string? GetRefererUrlAddress(this HttpRequest request, string refererHeaderKey = "Referer")
        {
            return request.Headers[refererHeaderKey].ToString();
        }

        /// <summary>
        /// 读取 Body 内容
        /// </summary>
        /// <param name="httpRequest"></param>
        /// <remarks>需先在 Startup 的 Configure 中注册 app.EnableBuffering()</remarks>
        /// <returns></returns>
        public static async Task<string?> ReadBodyContentAsync(this HttpRequest? httpRequest)
        {
            if (httpRequest == null) return default;

            httpRequest.Body.Seek(0, SeekOrigin.Begin);

            string requestContent;

            var contentEncoding = httpRequest.Headers.ContentEncoding.FirstOrDefault();

            if (contentEncoding != null && contentEncoding.Equals("gzip", StringComparison.OrdinalIgnoreCase))
            {
                using var requestBody = new MemoryStream();
                await httpRequest.Body.CopyToAsync(requestBody);
                httpRequest.Body.Position = 0;

                requestBody.Position = 0;

                await using var decompressedStream = new GZipStream(requestBody, CompressionMode.Decompress);
                using var sr = new StreamReader(decompressedStream, Encoding.UTF8);
                requestContent = await sr.ReadToEndAsync();
            }
            else
            {
                await using var requestBody = new MemoryStream();
                await httpRequest.Body.CopyToAsync(requestBody);
                httpRequest.Body.Position = 0;

                requestBody.Position = 0;

                using var requestReader = new StreamReader(requestBody);
                requestContent = await requestReader.ReadToEndAsync();
            }

            httpRequest.Body.Seek(0, SeekOrigin.Begin); //读取到Body后将索引设置到开头

            return requestContent;

        }

        /// <summary>
        /// 判断是否是 WebSocket 请求
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static bool IsWebSocketRequest(this HttpContext context)
        {
            return context.WebSockets.IsWebSocketRequest || context.Request.Path == "/ws";
        }
    }
}
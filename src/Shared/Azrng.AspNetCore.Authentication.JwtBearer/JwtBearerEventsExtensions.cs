using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;

namespace Azrng.AspNetCore.Authentication.JwtBearer
{
    /// <summary>
    /// JwtBearerEvents 辅助扩展。
    /// </summary>
    public static class JwtBearerEventsExtensions
    {
        /// <summary>
        /// 默认未授权响应体。
        /// </summary>
        public const string DefaultUnauthorizedResponseMessage = "{\"isSuccess\":false,\"message\":\"您无权访问该接口，请确保已经登录\",\"code\":\"401\"}";

        /// <summary>
        /// 添加 Token 过期响应头处理。
        /// </summary>
        public static JwtBearerEvents UseTokenExpiredHeader(
            this JwtBearerEvents events,
            string headerName = "Token-Expired",
            string headerValue = "true")
        {
            ArgumentNullException.ThrowIfNull(events);
            if (string.IsNullOrWhiteSpace(headerName))
                throw new ArgumentException("响应头名称不能为空", nameof(headerName));

            var previous = events.OnAuthenticationFailed;
            events.OnAuthenticationFailed = async context =>
            {
                if (previous is not null)
                    await previous(context);

                if (context.Exception is SecurityTokenExpiredException && !context.Response.HasStarted)
                    context.Response.Headers.Append(headerName, headerValue);
            };

            return events;
        }

        /// <summary>
        /// 添加自定义 401 JSON 响应处理。
        /// </summary>
        public static JwtBearerEvents UseUnauthorizedJsonResponse(
            this JwtBearerEvents events,
            string responseJson = DefaultUnauthorizedResponseMessage,
            string contentType = "application/json;charset=utf-8",
            int statusCode = StatusCodes.Status401Unauthorized)
        {
            ArgumentNullException.ThrowIfNull(events);
            ArgumentNullException.ThrowIfNull(responseJson);
            if (string.IsNullOrWhiteSpace(contentType))
                throw new ArgumentException("响应内容类型不能为空", nameof(contentType));

            var previous = events.OnChallenge;
            events.OnChallenge = async context =>
            {
                if (previous is not null)
                    await previous(context);

                if (context.Response.HasStarted)
                    return;

                context.HandleResponse();
                context.Response.ContentType = contentType;
                context.Response.StatusCode = statusCode;

                await context.Response.WriteAsync(responseJson);
            };

            return events;
        }

        /// <summary>
        /// 添加 Azrng 预置的 JWT Bearer 响应处理：Token 过期响应头和 401 JSON 响应体。
        /// </summary>
        public static JwtBearerEvents UseAzrngJwtBearerDefaultResponses(this JwtBearerEvents events)
        {
            ArgumentNullException.ThrowIfNull(events);

            return events.UseTokenExpiredHeader()
                         .UseUnauthorizedJsonResponse();
        }
    }
}

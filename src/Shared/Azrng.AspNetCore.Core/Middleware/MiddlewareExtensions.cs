using Azrng.AspNetCore.Core.Middleware;

namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// 中间件扩展
    /// </summary>
    public static class MiddlewareExtensions
    {
        /// <summary>
        /// 使用全局异常处理中间件
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseCustomExceptionMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CustomExceptionMiddleware>();
        }

        /// <summary>
        /// 请求Id传递中间件(会将入参请求头的请求ID原样返回到响应头中)
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseRequestIdMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestIdMiddleware>();
        }

        /// <summary>
        /// 显示所有服务中间件  需要先注册服务：services.AddShowAllServices();
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseShowAllServicesMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ShowAllServicesMiddleware>();
        }

        /// <summary>
        /// 使用自动审计日志
        /// </summary>
        /// <param name="app"></param>
        /// <param name="ignoreFilterRoutes">要忽略的路由</param>
        /// <returns></returns>
        public static IApplicationBuilder UseAutoAuditLog(this IApplicationBuilder app, List<string>? ignoreFilterRoutes = null)
        {
            return app.UseMiddleware<AuditLogMiddleware>(ignoreFilterRoutes ?? new List<string>());
        }
    }
}
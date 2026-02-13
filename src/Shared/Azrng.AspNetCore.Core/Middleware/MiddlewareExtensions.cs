using Azrng.AspNetCore.Core.AuditLog;
using Azrng.AspNetCore.Core.Middleware;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// 中间件扩展
/// </summary>
public static class MiddlewareExtensions
{
    /// <summary>
    /// 使用全局异常处理中间件
    /// </summary>
    [Obsolete("改为使用UseGlobalException")]
    public static IApplicationBuilder UseCustomExceptionMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<CustomExceptionMiddleware>();
    }

    /// <summary>
    /// 使用全局异常处理中间件
    /// </summary>
    public static IApplicationBuilder UseGlobalException(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<CustomExceptionMiddleware>();
    }

    /// <summary>
    /// 请求Id传递中间件(会将入参请求头的请求ID原样返回到响应头中)
    /// </summary>
    public static IApplicationBuilder UseRequestIdMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestIdMiddleware>();
    }

    /// <summary>
    /// 显示所有服务中间件  需要先注册服务：services.AddShowAllServices();
    /// </summary>
    public static IApplicationBuilder UseShowAllServicesMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ShowAllServicesMiddleware>();
    }

    /// <summary>
    /// 使用自动审计日志
    /// </summary>
    /// <param name="app">应用程序构建器</param>
    /// <param name="ignoreFilterRoutes">要忽略的路由（已过时，请使用 ConfigureAuditLogOptions 配置）</param>
    /// <returns>应用程序构建器</returns>
    public static IApplicationBuilder UseAutoAuditLog(this IApplicationBuilder app, List<string>? ignoreFilterRoutes = null)
    {
        // 为了向后兼容，如果传入了 ignoreFilterRoutes，创建配置选项
        if (ignoreFilterRoutes != null && ignoreFilterRoutes.Count > 0)
        {
            var options = new AuditLogOptions { IgnoreRoutePrefix = ignoreFilterRoutes };
            return app.UseMiddleware<AuditLogMiddleware>(new OptionsWrapper<AuditLogOptions>(options));
        }

        return app.UseMiddleware<AuditLogMiddleware>();
    }

    /// <summary>
    /// 使用自动审计日志并配置选项
    /// </summary>
    /// <param name="app">应用程序构建器</param>
    /// <param name="configureOptions">配置选项委托</param>
    /// <returns>应用程序构建器</returns>
    public static IApplicationBuilder UseAutoAuditLog(this IApplicationBuilder app, Action<AuditLogOptions> configureOptions)
    {
        var options = new AuditLogOptions();
        configureOptions?.Invoke(options);
        return app.UseMiddleware<AuditLogMiddleware>(new OptionsWrapper<AuditLogOptions>(options));
    }
}

/// <summary>
/// OptionsWrapper 用于包装单个选项实例
/// </summary>
internal class OptionsWrapper<TOptions> : IOptions<TOptions> where TOptions : class, new()
{
    public OptionsWrapper(TOptions value)
    {
        Value = value ?? new TOptions();
    }

    public TOptions Value { get; }
}

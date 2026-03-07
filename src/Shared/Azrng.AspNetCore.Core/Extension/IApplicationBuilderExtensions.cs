using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// IApplicationBuilder扩展
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// 使用 CORS 策略
    /// </summary>
    /// <param name="app">应用程序构建器</param>
    /// <param name="policyName">策略名称，默认为 "DefaultCors"</param>
    /// <returns>应用程序构建器</returns>
    /// <remarks>
    /// 使用示例：
    /// <code>
    /// // 使用默认策略
    /// app.UseCorsPolicy();
    ///
    /// // 使用指定策略
    /// app.UseCorsPolicy("MyCustomPolicy");
    /// </code>
    /// </remarks>
    public static IApplicationBuilder UseCorsPolicy(this IApplicationBuilder app, string policyName = "DefaultCors")
    {
        app.UseCors(policyName);
        return app;
    }

    /// <summary>
    /// 使用 any 跨域配置（快捷方法，兼容旧版本）
    /// </summary>
    /// <param name="app">应用程序构建器</param>
    /// <returns>应用程序构建器</returns>
    /// <remarks>
    /// 此方法使用 "AnyCors" 策略名称，需要与 <see cref="ServiceCollectionExtensions.AddAnyCors"/> 配合使用。
    /// </remarks>
    [Obsolete("建议使用 UseCorsPolicy 方法，以获得更好的灵活性和可配置性。")]
    public static IApplicationBuilder UseAnyCors(this IApplicationBuilder app)
    {
        app.UseCors("AnyCors");
        return app;
    }

    /// <summary>
    /// 启用 Body 重复读功能
    /// </summary>
    /// <remarks>须在 app.UseRouting() 之前注册</remarks>
    /// <param name="app"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseRequestBodyRepetitionRead(this IApplicationBuilder app)
    {
        return app.Use(async (context, next) =>
        {
            context.Request.EnableBuffering();
            await next.Invoke();
        });
    }
}
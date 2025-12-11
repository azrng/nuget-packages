using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// IApplicationBuilder扩展
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// 使用any 跨域配置
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseAnyCors(this IApplicationBuilder app)
    {
        app.UseCors("any");
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
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Text;

namespace Microsoft.AspNetCore.Builder;

public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// 使用swagger配置
    /// </summary>
    /// <param name="app"></param>
    /// <param name="onlyDevelopmentEnabled">只有开发环境启用</param>
    /// <param name="setupAction"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseDefaultSwagger(this IApplicationBuilder app, bool onlyDevelopmentEnabled = false,
                                                        Action<SwaggerOptions>? setupAction = null,
                                                        Action<SwaggerUIOptions>? action = null)
    {
        // 开发环境下启用Swagger
        if (onlyDevelopmentEnabled &&
            !Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")!.Equals("Development", StringComparison.OrdinalIgnoreCase))
        {
            return app;
        }

        app.UseSwagger(opts => { setupAction?.Invoke(opts); });
        app.UseSwaggerUI(c =>
        {
            if (action is null)
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebApi v1");

                //// 模型的默认扩展深度，设置为 -1 完全隐藏模型
                //c.DefaultModelsExpandDepth(-1);
                //// API文档仅展开标记
                //c.DocExpansion(DocExpansion.List);
                // API前缀设置为空
                c.RoutePrefix = string.Empty;

                // 文档页面标题
                c.DocumentTitle = "API Docs";

                //设置默认的接口文档展开方式，可选值包括None、List和Full。
                //默认值为None，表示不展开接口文档；List表示只展开接口列表；Full表示展开所有接口详情
                c.DocExpansion(DocExpansion.List);

                //控制Try It Out请求的请求持续时间（以毫秒为单位）的显示
                c.DisplayRequestDuration();

                // // 启用永久授权
                // c.EnablePersistAuthorization();

                c.SetUrlEditable();
            }
            else
            {
                //对swaggerEndpoint做一些特殊的处理
                action(c);
            }
        });

        return app;
    }

    /// <summary>
    /// 设置url为可编辑
    /// </summary>
    /// <remarks>效果 可以让url支持复制</remarks>
    /// <example>swaggerUIOptions.SetSpanEditable();</example>
    /// <param name="swaggerUiOptions"></param>
    public static void SetUrlEditable(this SwaggerUIOptions swaggerUiOptions)
    {
        var stringBuilder = new StringBuilder(swaggerUiOptions.HeadContent);
        stringBuilder.Append(@"
                    <script type='text/javascript'>
                    window.addEventListener('load', function () {
                        setTimeout(() => {
                            let createElement = window.ui.React.createElement
                            ui.React.createElement = function () {
                                let array = Array.from(arguments)
                                if (array.length == 3) {
                                    if (array[0] == 'span' && !array[1]) {
                                        array[1] = { contentEditable: true }
                                    }
                                }

                                let ele = createElement(...array)
                                return ele
                            }
                        })
                    })
                    </script>");
        swaggerUiOptions.HeadContent = stringBuilder.ToString();
    }
}
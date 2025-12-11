using Azrng.SettingConfig.Service;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace Azrng.SettingConfig;

/// <summary>
/// 配置UI构建扩展
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// 注册配置UI中间件
    /// </summary>
    public static IApplicationBuilder UseSettingDashboard([NotNull] this IApplicationBuilder app)
    {
        using (var scope = app.ApplicationServices.CreateScope())
        {
            var dataSourceProvider = scope.ServiceProvider.GetRequiredService<IDataSourceProvider>();
            dataSourceProvider.InitAsync().GetAwaiter().GetResult();
        }

        app.UseMiddleware<AspNetCoreDashboardMiddleware>();

        return app;
    }
}
using Azrng.SettingConfig.Service;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using System.Data;
using Azrng.Dapper;
using Microsoft.Extensions.Options;

namespace Azrng.SettingConfig;

/// <summary>
/// 服务集合扩展
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 当前配置的 API 路由前缀（用于特性路由）
    /// 注意：特性类无法使用依赖注入，因此使用静态字段传递配置
    /// </summary>
    internal static string CurrentApiRoutePrefix = "/api/platform";

    /// <summary>
    /// 添加配置服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="setupAction">配置选项设置</param>
    /// <exception cref="ArgumentNullException">当参数为空时抛出</exception>
    public static void AddSettingConfig(this IServiceCollection services,
        Action<DashboardOptions> setupAction)
    {
        // 添加控制器支持
        services.AddControllers().AddApplicationPart(typeof(ServiceCollectionExtensions).Assembly);

        // 配置选项
        var options = new DashboardOptions();
        setupAction.Invoke(options);
        options.ParamVerify();

        // 保存路由前缀供特性使用
        CurrentApiRoutePrefix = options.ApiRoutePrefix;

        // 注册配置选项到 Options 模式（供中间件和服务使用）
        services.Configure(Options.DefaultName, setupAction);

        // 注册数据库连接
        services.AddScoped<IDbConnection>(_ => new NpgsqlConnection(options.DbConnectionString));

        // 注册仓储和服务
        services.AddDapper();
        services.AddScoped<IDataSourceProvider, PgsqlDataSourceProvider>();
        services.AddScoped<IConfigSettingService, ConfigSettingService>();
        services.AddScoped<IConfigExternalProvideService, ConfigExternalProvideService>();

        // 注册单例服务
        services.AddSingleton<ManifestResourceService>();

        // 注入内存缓存（方便使用 Redis 替换）
        services.AddDistributedMemoryCache();
    }
}
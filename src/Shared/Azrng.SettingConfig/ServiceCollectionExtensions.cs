using Azrng.SettingConfig.Service;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using System.Data;
using Azrng.Dapper;

namespace Azrng.SettingConfig;

/// <summary>
/// 服务集合扩展
/// </summary>
public static class ServiceCollectionExtensions
{
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

        // 注册配置选项（使 ApiRoutePrefix 可被访问）
        services.Configure<DashboardOptions>(config =>
        {
            config.RoutePrefix = options.RoutePrefix;
            config.ApiRoutePrefix = options.ApiRoutePrefix;
            config.DbConnectionString = options.DbConnectionString;
            config.DbSchema = options.DbSchema;
            config.PageTitle = options.PageTitle;
            config.PageDescription = options.PageDescription;
            config.ConfigCacheTime = options.ConfigCacheTime;
            config.Authorization = options.Authorization;
        });

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
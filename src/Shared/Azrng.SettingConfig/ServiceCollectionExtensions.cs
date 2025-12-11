using Azrng.SettingConfig.Repository;
using Azrng.SettingConfig.Service;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using System.Data;

namespace Azrng.SettingConfig;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 默认api前缀
    /// </summary>
    internal static string ApiRoutePrefix = "/api/platform";

    /// <summary>
    /// 添加配置服务
    /// </summary>
    /// <param name="services"></param>
    /// <param name="setupAction"></param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <returns></returns>
    public static void AddSettingConfig(this IServiceCollection services,
        Action<DashboardOptions> setupAction)
    {
        services.AddControllers().AddApplicationPart(typeof(ServiceCollectionExtensions).Assembly);

        var setting = new DashboardOptions();
        setupAction.Invoke(setting);

        setting.ParamVerify();

        services.Configure(setupAction);

        ApiRoutePrefix = setting.ApiRoutePrefix;

        services.AddScoped<IDbConnection>(_ => new NpgsqlConnection(setting.DbConnectionString));
        services.AddScoped<IDapperRepository, DapperRepository>();
        services.AddScoped<IDataSourceProvider, PgsqlDataSourceProvider>();

        services.AddScoped<IConfigSettingService, ConfigSettingService>();
        services.AddScoped<IConfigExternalProvideService, ConfigExternalProvideService>();

        services.AddSingleton<ManifestResourceService>();

        // 注入内存缓存 方便使用redis替换
        services.AddDistributedMemoryCache();
    }
}
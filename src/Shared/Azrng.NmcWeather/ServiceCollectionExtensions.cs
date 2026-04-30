using Common.HttpClients;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Azrng.NmcWeather;

/// <summary>
/// NMC Weather 依赖注入扩展。
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 注册中央气象台天气客户端。
    /// </summary>
    public static IServiceCollection AddNmcWeather(this IServiceCollection services)
    {
        return services.AddNmcWeather(_ => { });
    }

    /// <summary>
    /// 注册中央气象台天气客户端。
    /// </summary>
    public static IServiceCollection AddNmcWeather(
        this IServiceCollection services,
        Action<NmcWeatherOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        if (services.All(static descriptor => descriptor.ServiceType != typeof(IHttpHelper)))
        {
            services.AddHttpClientService();
        }

        services.Configure(configure);
        services.TryAddTransient<INmcLocationClient, NmcLocationClient>();
        services.TryAddTransient<INmcWeatherClient, NmcWeatherClient>();
        services.TryAddTransient<INmcWeatherQueryClient, NmcWeatherQueryClient>();
        return services;
    }
}

using Azrng.Core.Model;
using Azrng.EFCore.AutoAudit.Config;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Azrng.EFCore.AutoAudit;

public static class ServiceCollectionExtension
{
    /// <summary>
    /// 添加EF自动审计
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configAction"></param>
    /// <param name="databaseType"></param>
    /// <returns></returns>
    public static IServiceCollection AddEFCoreAutoAudit(this IServiceCollection services,
                                                        Action<IAuditConfigBuilder> configAction, DatabaseType databaseType)
    {
        ArgumentNullException.ThrowIfNull(configAction);

        services.TryAddScoped<AuditInterceptor>();
        AuditConfig.Configure(services, configAction, databaseType);

        services.AddTransient<IStartupFilter, AutoAuditStartupFilter>();

        return services;
    }
}
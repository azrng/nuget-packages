using Azrng.Core.Model;
using Microsoft.Extensions.DependencyInjection;

namespace Azrng.EFCore.AutoAudit.Config;

/// <summary>
/// 审计配置
/// </summary>
public static class AuditConfig
{
    /// <summary>
    /// 审计选项配置
    /// </summary>
    internal static AuditConfigOptions Options = new();

    /// <summary>
    /// 启用审计
    /// </summary>
    public static void EnableAudit()
    {
        Options.AuditEnabled = true;
    }

    /// <summary>
    /// 禁用审计
    /// </summary>
    public static void DisableAudit()
    {
        Options.AuditEnabled = false;
    }

    /// <summary>
    /// 配置审计信息
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configAction"></param>
    /// <param name="databaseType"></param>
    public static void Configure(IServiceCollection services, Action<IAuditConfigBuilder>? configAction, DatabaseType databaseType)
    {
        ArgumentNullException.ThrowIfNull(services);
        if (configAction is null)
            return;

        var builder = new AuditConfigBuilder(services);
        configAction.Invoke(builder);
        Options = builder.Build();

        Options.DatabaseType = databaseType;
    }
}
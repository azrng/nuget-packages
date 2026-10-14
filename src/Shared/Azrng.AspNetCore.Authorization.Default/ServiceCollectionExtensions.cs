using Azrng.AspNetCore.Authorization.Default;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// 基于 Endpoint 权限元数据的授权服务扩展。
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 权限授权策略名称。
    /// </summary>
    public const string PermissionPolicyName = "PermissionPolicy";

    /// <summary>
    /// 添加基于 Endpoint 权限元数据的授权服务。
    /// </summary>
    /// <typeparam name="TPermissionEvaluator">权限评估器类型。</typeparam>
    /// <param name="services">服务集合。</param>
    /// <returns>服务集合。</returns>
    public static IServiceCollection AddPermissionAuthorization<TPermissionEvaluator>(
        this IServiceCollection services)
        where TPermissionEvaluator : class, IPermissionEvaluator
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<IPermissionEvaluator, TPermissionEvaluator>();
        services.AddAuthorization(options =>
        {
            options.AddPolicy(PermissionPolicyName, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.Requirements.Add(new PermissionAuthorizationRequirement());
            });
        });

        services.AddHttpContextAccessor();
        services.TryAddEnumerable(new ServiceDescriptor(
            typeof(IAuthorizationHandler),
            typeof(PermissionAuthorizationHandler),
            ServiceLifetime.Scoped));

        return services;
    }
}

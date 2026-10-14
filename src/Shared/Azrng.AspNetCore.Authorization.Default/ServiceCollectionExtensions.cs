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
    /// 默认授权策略名称。
    /// </summary>
    public const string DefaultPolicyName = "DefaultPermissionPolicy";

    /// <summary>
    /// 仅要求用户已认证的授权策略名称。
    /// </summary>
    public const string AuthenticatedOnlyPolicyName = "AuthenticatedOnlyPolicy";

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
        return services.AddPermissionAuthorization<TPermissionEvaluator>(_ => { });
    }

    /// <summary>
    /// 添加基于 Endpoint 权限元数据的授权服务，并配置权限授权选项。
    /// </summary>
    /// <typeparam name="TPermissionEvaluator">权限评估器类型。</typeparam>
    /// <param name="services">服务集合。</param>
    /// <param name="configureOptions">权限授权选项配置。</param>
    /// <returns>服务集合。</returns>
    public static IServiceCollection AddPermissionAuthorization<TPermissionEvaluator>(
        this IServiceCollection services,
        Action<PermissionAuthorizationOptions> configureOptions)
        where TPermissionEvaluator : class, IPermissionEvaluator
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        services.Configure(configureOptions);
        services.AddScoped<IPermissionEvaluator, TPermissionEvaluator>();
        services.AddAuthorization(options =>
        {
            // 策略由两类 requirement 组成（须全部满足）：
            // 1. RequireAuthenticatedUser：要求已认证，匿名请求由框架返回 401；
            // 2. PermissionAuthorizationRequirement：由 PermissionAuthorizationHandler
            //    调用 IPermissionEvaluator 做业务权限判断。
            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddPermissionRequirement()
                .Build();

            // 默认策略和 DefaultPermissionPolicy 执行权限评估；AuthenticatedOnlyPolicy 只要求认证。
            options.DefaultPolicy = policy;
            options.AddPolicy(DefaultPolicyName, policy);
            options.AddPolicy(AuthenticatedOnlyPolicyName, authenticatedPolicy =>
                authenticatedPolicy.RequireAuthenticatedUser());
        });

        services.AddHttpContextAccessor();
        services.TryAddEnumerable(new ServiceDescriptor(
            typeof(IAuthorizationHandler),
            typeof(PermissionAuthorizationHandler),
            ServiceLifetime.Scoped));

        return services;
    }

    private static AuthorizationPolicyBuilder AddPermissionRequirement(
        this AuthorizationPolicyBuilder policyBuilder)
    {
        policyBuilder.Requirements.Add(new PermissionAuthorizationRequirement());
        return policyBuilder;
    }
}

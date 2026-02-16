using Azrng.AspNetCore.Authorization.Default;
using Microsoft.AspNetCore.Authorization;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// 基于路径的授权服务扩展
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 默认授权策略名称
    /// </summary>
    public const string DefaultPolicyName = "DefaultPermissionPolicy";

    /// <summary>
    /// 添加基于路径的授权服务
    /// </summary>
    /// <typeparam name="TPermissionService">自定义权限验证服务类型</typeparam>
    /// <param name="services">服务集合</param>
    /// <param name="allowAnonymousPaths">允许匿名访问的路径数组</param>
    /// <returns>服务集合</returns>
    /// <remarks>
    /// 此方法会注册以下服务：
    /// 1. <typeparamref name="TPermissionService"/> 作为 <see cref="IPermissionVerifyService"/> 的实现
    /// 2. <see cref="PermissionAuthorizationHandler"/> 作为授权处理器
    /// 3. <see cref="DefaultPolicyProvider"/> 作为授权策略提供器
    /// 4. HTTP 上下文访问器
    /// </remarks>
    /// <example>
    /// 示例：注册授权服务
    /// <code>
    /// services.AddPathBasedAuthorization&lt;MyPermissionService&gt;(
    ///     "/api/login",
    ///     "/api/register",
    ///     "/api/health"
    /// );
    /// </code>
    /// </example>
    public static IServiceCollection AddPathBasedAuthorization<TPermissionService>(
        this IServiceCollection services,
        params string[] allowAnonymousPaths)
        where TPermissionService : class, IPermissionVerifyService
    {
        services.AddAuthorization(options =>
        {
            var permissionRequirement = new PermissionRequirement(allowAnonymousPaths);
            options.AddPolicy(DefaultPolicyName, policy => policy.AddPermissionRequirement(permissionRequirement));
        });

        services.AddSingleton<IAuthorizationPolicyProvider, DefaultPolicyProvider>();
        services.AddHttpContextAccessor();
        services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddScoped<IPermissionVerifyService, TPermissionService>();

        return services;
    }

    /// <summary>
    /// 向授权策略添加权限需求
    /// </summary>
    /// <param name="policyBuilder">策略构造器</param>
    /// <param name="requirement">权限需求</param>
    /// <returns>策略构造器</returns>
    /// <exception cref="ArgumentNullException">当 requirement 为 null 时抛出</exception>
    private static AuthorizationPolicyBuilder AddPermissionRequirement(
        this AuthorizationPolicyBuilder policyBuilder,
        PermissionRequirement requirement)
    {
        if (requirement == null)
            throw new ArgumentNullException(nameof(requirement));

        policyBuilder.Requirements.Add(requirement);
        return policyBuilder;
    }

    /// <summary>
    /// 添加自定义授权服务（保留旧方法名以保持向后兼容）
    /// </summary>
    /// <typeparam name="TPermissionService">自定义权限验证服务类型</typeparam>
    /// <param name="services">服务集合</param>
    /// <param name="allowAnonymousPaths">允许匿名访问的路径数组</param>
    /// <returns>服务集合</returns>
    [Obsolete("请使用 AddPathBasedAuthorization 方法，此方法仅为向后兼容而保留")]
    public static IServiceCollection AddMyAuthorization<TPermissionService>(
        this IServiceCollection services,
        params string[] allowAnonymousPaths)
        where TPermissionService : class, IPermissionVerifyService
    {
        return services.AddPathBasedAuthorization<TPermissionService>(allowAnonymousPaths);
    }
}
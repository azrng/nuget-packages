using Azrng.AspNetCore.Authorization.Default;
using Microsoft.AspNetCore.Authorization;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 默认策略
    /// </summary>
    public const string DefaultPolicy = "customer";

    /// <summary>
    /// 添加自定义策略
    /// </summary>
    /// <param name="services"></param>
    /// <param name="allowAnonymousAction">允许匿名访问的action</param>
    public static IServiceCollection AddMyAuthorization<T>(this IServiceCollection services,
        params string[] allowAnonymousAction) where T : class, IPermissionVerifyService
    {
        services.AddAuthorization(options =>
        {
            // 自定义权限需求
            var permissionRequirement = new PermissionRequirement(allowAnonymousAction);
            options.AddPolicy(DefaultPolicy, policy => policy.RequirePermission(permissionRequirement));
        });
        services.AddSingleton<IAuthorizationPolicyProvider, DefaultPolicyProvider>();

        services.AddHttpContextAccessor();
        //依赖注入，将自定义的授权处理器 匹配给官方授权处理器接口，这样当系统处理授权的时候，就会直接访问我们自定义的授权处理器了。
        services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddScoped<IPermissionVerifyService, T>();

        return services;
    }


    /// <summary>
    /// 必须的权限
    /// </summary>
    /// <param name="policyBuilder">策略构造器</param>
    /// <param name="permission">自定义策略需求</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    private static AuthorizationPolicyBuilder RequirePermission(this AuthorizationPolicyBuilder policyBuilder,
        PermissionRequirement permission)
    {
        if (permission == null)
            throw new ArgumentNullException(nameof(permission));
        policyBuilder.Requirements.Add(permission);
        return policyBuilder;
    }
}
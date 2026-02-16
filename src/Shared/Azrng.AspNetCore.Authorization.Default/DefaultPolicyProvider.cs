using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Azrng.AspNetCore.Authorization.Default;

/// <summary>
/// 默认授权策略提供器
/// </summary>
/// <remarks>
/// 此提供器实现了 <see cref="IAuthorizationPolicyProvider"/> 接口
/// 用于提供动态授权策略和默认授权策略
/// </remarks>
internal class DefaultPolicyProvider : IAuthorizationPolicyProvider
{
    private readonly AuthorizationOptions _options;

    /// <summary>
    /// 初始化 <see cref="DefaultPolicyProvider"/> 的新实例
    /// </summary>
    /// <param name="options">授权选项</param>
    /// <exception cref="ArgumentNullException">当 options 为 null 时抛出</exception>
    public DefaultPolicyProvider(IOptions<AuthorizationOptions> options)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        _options = options.Value;
    }

    /// <summary>
    /// 获取指定名称的授权策略
    /// </summary>
    /// <param name="policyName">策略名称</param>
    /// <returns>授权策略，如果不存在返回 null</returns>
    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        return Task.FromResult(_options.GetPolicy(policyName));
    }

    /// <summary>
    /// 获取默认授权策略
    /// </summary>
    /// <remarks>
    /// 当使用 [Authorize] 特性而不指定策略名称时，会使用此策略
    /// </remarks>
    /// <returns>默认授权策略</returns>
    public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
    {
        return Task.FromResult(_options.GetPolicy(ServiceCollectionExtensions.DefaultPolicyName)
            ?? new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());
    }

    /// <summary>
    /// 获取回退授权策略
    /// </summary>
    /// <remarks>
    /// 当合并策略和未指定策略时使用此策略
    /// 返回 null 表示不使用回退策略
    /// </remarks>
    /// <returns>回退授权策略（始终返回 null）</returns>
    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
    {
        return Task.FromResult<AuthorizationPolicy?>(null);
    }
}
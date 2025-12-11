using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Azrng.AspNetCore.Authorization.Default;

/// <summary>
/// 资源授权策略
/// 实现动态 AddPolicy
/// </summary>
internal class DefaultPolicyProvider : IAuthorizationPolicyProvider
{
    private readonly AuthorizationOptions _options;

    /// <summary>
    /// 构造资源授权策略
    /// </summary>
    /// <param name="options">配置信息</param>
    /// <exception cref="ArgumentNullException">授权配置为空</exception>
    public DefaultPolicyProvider(IOptions<AuthorizationOptions> options)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        _options = options.Value;
    }

    /// <summary>
    /// 自定义授权策略
    /// </summary>
    /// <param name="policyName"></param>
    /// <returns></returns>
    public Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
    {
        return Task.FromResult(_options.GetPolicy(policyName));
    }

    /// <summary>
    /// 默认策略
    /// 在未指定策略名称的情况下为 [Authorize] 属性提供授权策略
    /// </summary>
    /// <returns></returns>
    public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
    {
        return Task.FromResult(_options.GetPolicy(ServiceCollectionExtensions.DefaultPolicy));
    }

    /// <summary>
    /// 回退策略
    /// 以提供在合并策略和未指定策略时使用的策略
    /// </summary>
    /// <returns></returns>
    public Task<AuthorizationPolicy> GetFallbackPolicyAsync() => Task.FromResult<AuthorizationPolicy>(null);
}
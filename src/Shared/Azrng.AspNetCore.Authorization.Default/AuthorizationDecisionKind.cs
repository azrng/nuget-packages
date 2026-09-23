namespace Azrng.AspNetCore.Authorization.Default;

/// <summary>
/// 权限评估结果类型。
/// </summary>
public enum AuthorizationDecisionKind
{
    /// <summary>
    /// 允许访问。
    /// </summary>
    Allowed,

    /// <summary>
    /// 明确拒绝访问。
    /// </summary>
    Denied,

    /// <summary>
    /// 未找到可用的权限配置。
    /// </summary>
    NotConfigured,

    /// <summary>
    /// 权限依赖发生异常，默认按拒绝处理。
    /// </summary>
    DependencyError
}

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
    /// 拒绝访问（含未配置、依赖故障等一切不允许的场景）。
    /// </summary>
    Denied
}

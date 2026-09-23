namespace Azrng.AspNetCore.Authorization.Default;

/// <summary>
/// 权限评估结果。
/// </summary>
/// <remarks>
/// 只区分允许与拒绝：未配置、依赖故障等场景一律返回拒绝；
/// 依赖故障也可直接抛异常，由授权处理器按 fail-closed 记日志并拒绝。
/// </remarks>
public sealed class AuthorizationDecision
{
    private AuthorizationDecision(AuthorizationDecisionKind kind)
    {
        Kind = kind;
    }

    /// <summary>
    /// 结果类型。
    /// </summary>
    public AuthorizationDecisionKind Kind { get; }

    /// <summary>
    /// 是否允许访问。
    /// </summary>
    public bool IsAllowed => Kind == AuthorizationDecisionKind.Allowed;

    /// <summary>
    /// 创建允许结果。
    /// </summary>
    public static AuthorizationDecision Allow() => new(AuthorizationDecisionKind.Allowed);

    /// <summary>
    /// 创建拒绝结果。
    /// </summary>
    public static AuthorizationDecision Deny() => new(AuthorizationDecisionKind.Denied);
}

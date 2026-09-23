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

/// <summary>
/// 权限评估结果。
/// </summary>
public sealed class AuthorizationDecision
{
    private AuthorizationDecision(AuthorizationDecisionKind kind, string? diagnosticCode)
    {
        Kind = kind;
        DiagnosticCode = diagnosticCode;
    }

    /// <summary>
    /// 结果类型。
    /// </summary>
    public AuthorizationDecisionKind Kind { get; }

    /// <summary>
    /// 可供日志和指标使用的非敏感诊断码。
    /// </summary>
    public string? DiagnosticCode { get; }

    /// <summary>
    /// 是否允许访问。
    /// </summary>
    public bool IsAllowed => Kind == AuthorizationDecisionKind.Allowed;

    /// <summary>
    /// 创建允许结果。
    /// </summary>
    public static AuthorizationDecision Allow() => new(AuthorizationDecisionKind.Allowed, null);

    /// <summary>
    /// 创建拒绝结果。
    /// </summary>
    public static AuthorizationDecision Deny(string? diagnosticCode = null) =>
        new(AuthorizationDecisionKind.Denied, diagnosticCode);

    /// <summary>
    /// 创建未配置结果。
    /// </summary>
    public static AuthorizationDecision NotConfigured(string? diagnosticCode = null) =>
        new(AuthorizationDecisionKind.NotConfigured, diagnosticCode);

    /// <summary>
    /// 创建依赖异常结果。
    /// </summary>
    public static AuthorizationDecision DependencyError(string? diagnosticCode = null) =>
        new(AuthorizationDecisionKind.DependencyError, diagnosticCode);
}

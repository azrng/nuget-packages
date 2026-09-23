namespace Azrng.AspNetCore.Authorization.Default;

/// <summary>
/// 基于请求上下文执行权限评估。
/// </summary>
public interface IPermissionEvaluator
{
    /// <summary>
    /// 异步评估当前请求是否有权限访问。
    /// </summary>
    /// <param name="context">权限上下文。</param>
    /// <param name="cancellationToken">请求取消令牌。</param>
    /// <returns>授权决策。</returns>
    Task<AuthorizationDecision> AuthorizeAsync(
        PermissionContext context,
        CancellationToken cancellationToken = default);
}

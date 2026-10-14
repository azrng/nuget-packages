namespace Azrng.AspNetCore.Authorization.Default;

/// <summary>
/// 权限授权选项。
/// </summary>
public sealed class PermissionAuthorizationOptions
{
    /// <summary>
    /// 是否在权限策略的 Endpoint 没有权限元数据时拒绝请求，而不调用评估器。
    /// </summary>
    /// <remarks>
    /// 默认关闭以兼容按路径执行权限评估的应用。仅认证策略和匿名 Endpoint 不受此选项影响。
    /// </remarks>
    public bool DenyWhenPermissionMetadataMissing { get; set; }
}

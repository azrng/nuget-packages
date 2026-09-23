namespace Azrng.AspNetCore.Authorization.Default;

/// <summary>
/// 多个权限码的匹配方式。
/// </summary>
public enum PermissionMatchMode
{
    /// <summary>
    /// 所有权限码都必须满足。
    /// </summary>
    All,

    /// <summary>
    /// 任意一个权限码满足即可。
    /// </summary>
    Any
}

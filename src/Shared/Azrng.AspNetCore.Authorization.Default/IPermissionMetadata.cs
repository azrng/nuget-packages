namespace Azrng.AspNetCore.Authorization.Default;

/// <summary>
/// Endpoint 上声明的权限元数据。
/// </summary>
public interface IPermissionMetadata
{
    /// <summary>
    /// 权限码集合。
    /// </summary>
    IReadOnlyList<string> Permissions { get; }

    /// <summary>
    /// 多个权限码的匹配方式。
    /// </summary>
    PermissionMatchMode MatchMode { get; }
}

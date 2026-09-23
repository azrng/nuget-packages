using System.Collections.ObjectModel;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Azrng.AspNetCore.Authorization.Default;

/// <summary>
/// 权限评估所需的请求上下文快照。
/// </summary>
/// <remarks>
/// Path + Method 标识“请求什么操作”，User 标识“谁在请求”，
/// RequiredPermissions 是 Endpoint 声明的权限码；其余请求信息（租户头、查询参数等）经 <see cref="HttpContext"/> 获取。
/// </remarks>
public sealed class PermissionContext
{
    /// <summary>
    /// 初始化 <see cref="PermissionContext"/> 的新实例。
    /// </summary>
    public PermissionContext(
        HttpContext httpContext,
        string path,
        string method,
        ClaimsPrincipal user,
        IEnumerable<IPermissionMetadata>? requiredPermissions = null)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        ArgumentNullException.ThrowIfNull(path);
        ArgumentNullException.ThrowIfNull(method);
        ArgumentNullException.ThrowIfNull(user);

        HttpContext = httpContext;
        Path = path;
        Method = method;
        User = user;
        RequiredPermissions = new ReadOnlyCollection<IPermissionMetadata>(
            (requiredPermissions ?? Array.Empty<IPermissionMetadata>()).ToArray());
    }

    /// <summary>
    /// 当前 HTTP 上下文，用于获取路由参数、查询参数、请求头等其余信息。
    /// </summary>
    public HttpContext HttpContext { get; }

    /// <summary>
    /// 请求路径，已转换为小写。
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// HTTP 方法，与 <see cref="Path"/> 共同标识被请求的操作。
    /// </summary>
    public string Method { get; }

    /// <summary>
    /// 当前用户。
    /// </summary>
    public ClaimsPrincipal User { get; }

    /// <summary>
    /// Endpoint 上声明的权限元数据。
    /// </summary>
    public IReadOnlyList<IPermissionMetadata> RequiredPermissions { get; }
}

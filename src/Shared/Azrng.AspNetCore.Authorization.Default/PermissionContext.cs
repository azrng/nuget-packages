using System.Collections.ObjectModel;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Azrng.AspNetCore.Authorization.Default;

/// <summary>
/// 权限评估所需的请求上下文快照。
/// </summary>
/// <remarks>
/// User 标识“谁在请求”，RequiredPermissions 是 Endpoint 声明的权限码；
/// 请求路径、HTTP 方法和其他请求信息经 <see cref="HttpContext"/> 获取。
/// </remarks>
public sealed class PermissionContext
{
    /// <summary>
    /// 初始化 <see cref="PermissionContext"/> 的新实例。
    /// </summary>
    public PermissionContext(
        HttpContext httpContext,
        ClaimsPrincipal user,
        IEnumerable<IPermissionMetadata>? requiredPermissions = null)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        ArgumentNullException.ThrowIfNull(user);

        HttpContext = httpContext;
        User = user;
        RequiredPermissions = new ReadOnlyCollection<IPermissionMetadata>(
            (requiredPermissions ?? Array.Empty<IPermissionMetadata>()).ToArray());
    }

    /// <summary>
    /// 当前 HTTP 上下文，用于获取路由参数、查询参数、请求头等其余信息。
    /// </summary>
    public HttpContext HttpContext { get; }

    /// <summary>
    /// 当前用户。
    /// </summary>
    public ClaimsPrincipal User { get; }

    /// <summary>
    /// Endpoint 上声明的权限元数据。
    /// </summary>
    public IReadOnlyList<IPermissionMetadata> RequiredPermissions { get; }
}

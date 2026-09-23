using System.Collections.ObjectModel;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Azrng.AspNetCore.Authorization.Default;

/// <summary>
/// 权限评估所需的 HTTP 请求上下文。
/// </summary>
public sealed class PermissionContext
{
    /// <summary>
    /// 初始化 <see cref="PermissionContext"/> 的新实例。
    /// </summary>
    public PermissionContext(
        HttpContext httpContext,
        Endpoint? endpoint,
        string path,
        string method,
        RouteValueDictionary routeValues,
        ClaimsPrincipal user,
        IEnumerable<IPermissionMetadata>? requiredPermissions = null)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        ArgumentNullException.ThrowIfNull(path);
        ArgumentNullException.ThrowIfNull(method);
        ArgumentNullException.ThrowIfNull(routeValues);
        ArgumentNullException.ThrowIfNull(user);

        HttpContext = httpContext;
        Endpoint = endpoint;
        Path = path;
        Method = method;
        RouteValues = new RouteValueDictionary(routeValues);
        User = user;
        RequiredPermissions = new ReadOnlyCollection<IPermissionMetadata>(
            (requiredPermissions ?? Array.Empty<IPermissionMetadata>()).ToArray());
    }

    /// <summary>
    /// 当前 HTTP 上下文。
    /// </summary>
    public HttpContext HttpContext { get; }

    /// <summary>
    /// 当前 Endpoint，手动调用授权服务时可能为空。
    /// </summary>
    public Endpoint? Endpoint { get; }

    /// <summary>
    /// 请求路径，保持与旧接口一致，已转换为小写。
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// HTTP 方法。
    /// </summary>
    public string Method { get; }

    /// <summary>
    /// 路由参数快照。
    /// </summary>
    public RouteValueDictionary RouteValues { get; }

    /// <summary>
    /// 当前用户。
    /// </summary>
    public ClaimsPrincipal User { get; }

    /// <summary>
    /// Endpoint 上声明的权限元数据。
    /// </summary>
    public IReadOnlyList<IPermissionMetadata> RequiredPermissions { get; }
}

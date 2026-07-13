using Microsoft.AspNetCore.Authorization;

namespace Azrng.AspNetCore.Authorization.Default;

/// <summary>
/// 基于路径的权限授权需求
/// </summary>
/// <remarks>
/// 此需求用于定义哪些路径允许匿名访问，哪些路径需要权限验证。
/// 实例在策略注册时构建并在整个应用生命周期内共享，因此属性只读且内部数组做了防御性拷贝，
/// 避免外部在运行期修改匿名路径列表造成越权风险。
/// </remarks>
public class PermissionRequirement : IAuthorizationRequirement
{
    private readonly string[] _allowAnonymousPaths;

    /// <summary>
    /// 初始化 <see cref="PermissionRequirement"/> 的新实例
    /// </summary>
    /// <param name="allowAnonymousPaths">允许匿名访问的路径数组</param>
    /// <exception cref="ArgumentNullException">当 <paramref name="allowAnonymousPaths"/> 为 null 时抛出</exception>
    public PermissionRequirement(params string[] allowAnonymousPaths)
    {
        _allowAnonymousPaths = allowAnonymousPaths ?? throw new ArgumentNullException(nameof(allowAnonymousPaths));
    }

    /// <summary>
    /// 获取允许匿名访问的路径集合
    /// </summary>
    /// <remarks>
    /// 这些路径不需要用户登录即可访问，例如 "/api/login"、"/api/register" 等。
    /// 路径匹配按路径段前缀进行（StartsWithSegments），不区分大小写；
    /// 返回只读集合以防止外部修改内部共享状态。
    /// </remarks>
    public IReadOnlyCollection<string> AllowAnonymousPaths => _allowAnonymousPaths;
}
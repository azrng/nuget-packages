using Microsoft.AspNetCore.Authorization;

namespace Azrng.AspNetCore.Authorization.Default;

/// <summary>
/// 基于路径的权限授权需求
/// </summary>
/// <remarks>
/// 此需求用于定义哪些路径允许匿名访问，哪些路径需要权限验证。
/// 实例在策略注册时构建并在整个应用生命周期内共享，因此内部数组会做防御性拷贝，
/// 避免外部数组在运行期修改匿名路径列表造成越权风险。
/// </remarks>
public class PermissionRequirement : IAuthorizationRequirement
{
    private string[] _allowAnonymousPaths;

    /// <summary>
    /// 初始化 <see cref="PermissionRequirement"/> 的新实例
    /// </summary>
    /// <param name="allowAnonymousPaths">允许匿名访问的路径数组</param>
    /// <exception cref="ArgumentNullException">当 <paramref name="allowAnonymousPaths"/> 为 null 时抛出</exception>
    public PermissionRequirement(params string[] allowAnonymousPaths)
    {
        _allowAnonymousPaths = NormalizeAllowAnonymousPaths(allowAnonymousPaths);
    }

    /// <summary>
    /// 获取或设置允许匿名访问的路径数组
    /// </summary>
    /// <remarks>
    /// 这些路径不需要用户登录即可访问，例如 "/api/login"、"/api/register" 等。
    /// 路径匹配按路径段前缀进行（StartsWithSegments），不区分大小写。
    /// 读取和设置时都会进行防御性拷贝，避免外部修改内部共享状态。
    /// </remarks>
    /// <exception cref="ArgumentNullException">当设置值为 null 时抛出</exception>
    public string[] AllowAnonymousPaths
    {
        get => _allowAnonymousPaths.ToArray();
        set => _allowAnonymousPaths = NormalizeAllowAnonymousPaths(value);
    }

    internal IReadOnlyList<string> NormalizedAllowAnonymousPaths => _allowAnonymousPaths;

    private static string[] NormalizeAllowAnonymousPaths(string[] allowAnonymousPaths)
    {
        if (allowAnonymousPaths == null)
            throw new ArgumentNullException(nameof(allowAnonymousPaths));

        return allowAnonymousPaths
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Select(NormalizePath)
            .ToArray();
    }

    private static string NormalizePath(string path)
    {
        var normalized = path.Trim().Replace('\\', '/');

        if (!normalized.StartsWith('/'))
        {
            normalized = "/" + normalized.TrimStart('/');
        }

        if (normalized.Length > 1)
        {
            normalized = normalized.TrimEnd('/');
        }

        return normalized;
    }
}

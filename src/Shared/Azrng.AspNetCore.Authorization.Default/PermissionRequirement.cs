using Microsoft.AspNetCore.Authorization;

namespace Azrng.AspNetCore.Authorization.Default;

/// <summary>
/// 基于路径的权限授权需求
/// </summary>
/// <remarks>
/// 此需求用于定义哪些路径允许匿名访问，哪些路径需要权限验证
/// </remarks>
public class PermissionRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// 初始化 <see cref="PermissionRequirement"/> 的新实例
    /// </summary>
    /// <param name="allowAnonymousPaths">允许匿名访问的路径数组</param>
    public PermissionRequirement(params string[] allowAnonymousPaths)
    {
        AllowAnonymousPaths = allowAnonymousPaths;
    }

    /// <summary>
    /// 获取或设置允许匿名访问的路径数组
    /// </summary>
    /// <remarks>
    /// 这些路径不需要用户登录即可访问
    /// 例如："/api/login", "/api/register" 等
    /// 路径匹配使用包含匹配（Contains），不区分大小写
    /// </remarks>
    public string[] AllowAnonymousPaths { get; set; }
}
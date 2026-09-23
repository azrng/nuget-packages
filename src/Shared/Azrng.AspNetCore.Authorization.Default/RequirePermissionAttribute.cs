using Microsoft.AspNetCore.Authorization;

namespace Azrng.AspNetCore.Authorization.Default;

/// <summary>
/// 为 MVC Controller 或 Action 声明权限码。
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class RequirePermissionAttribute : AuthorizeAttribute, IPermissionMetadata
{
    private readonly IReadOnlyList<string> _permissions;

    /// <summary>
    /// 初始化 <see cref="RequirePermissionAttribute"/> 的新实例。
    /// </summary>
    /// <param name="permissions">权限码。</param>
    public RequirePermissionAttribute(params string[] permissions)
    {
        _permissions = new System.Collections.ObjectModel.ReadOnlyCollection<string>(
            PermissionMetadata.Normalize(permissions));
        Policy = Microsoft.Extensions.DependencyInjection.ServiceCollectionExtensions.DefaultPolicyName;
    }

    /// <inheritdoc />
    public IReadOnlyList<string> Permissions => _permissions;

    /// <summary>
    /// 多个权限码的匹配方式，默认为全部满足。
    /// </summary>
    public PermissionMatchMode MatchMode { get; set; } = PermissionMatchMode.All;
}

using Microsoft.AspNetCore.Authorization;

namespace Azrng.AspNetCore.Authorization.Default;

/// <summary>
/// 默认权限策略使用的内部授权需求。
/// </summary>
internal sealed class PermissionAuthorizationRequirement : IAuthorizationRequirement
{
}

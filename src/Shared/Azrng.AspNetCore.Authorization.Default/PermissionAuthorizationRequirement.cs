using Microsoft.AspNetCore.Authorization;

namespace Azrng.AspNetCore.Authorization.Default;

/// <summary>
/// 默认权限策略使用的内部授权需求：标记该请求需要调用 IPermissionEvaluator。
/// </summary>
internal sealed class PermissionAuthorizationRequirement : IAuthorizationRequirement
{
}

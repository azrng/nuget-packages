using Microsoft.AspNetCore.Authorization;

namespace Azrng.AspNetCore.Authorization.Default
{
    /// <summary>
    /// 权限需求
    /// </summary>
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public PermissionRequirement(params string[] loginVisitAction)
        {
            LoginVisitAction = loginVisitAction;
        }

        /// <summary>
        /// 允许登录即可访问的action
        /// </summary>
        public string[] LoginVisitAction { get; set; }

        // /// <summary>
        // /// 认证授权类型(如果一个项目配置了多种授权方式，可以用来区别)
        // /// </summary>
        // public string ClaimType { get; set; } = "Bearer";
    }
}
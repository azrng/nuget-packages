using System.Security.Claims;

namespace Azrng.AspNetCore.Authentication.Basic
{
    /// <summary>
    /// Basic 认证验证接口
    /// </summary>
    /// <remarks>
    /// 实现此接口以自定义用户认证后的 Claims 生成逻辑
    /// 可以添加角色、权限、自定义声明等信息到 Claims 中
    /// </remarks>
    public interface IBasicAuthorizeVerify
    {
        /// <summary>
        /// 获取当前用户的 Claims
        /// </summary>
        /// <param name="userName">用户名</param>
        /// <returns>用户的 Claims 数组</returns>
        /// <example>
        /// 示例：添加角色和自定义声明
        /// <code>
        /// public Task&lt;Claim[]&gt; GetCurrentUserClaims(string userName)
        /// {
        ///     var claims = new List&lt;Claim&gt;
        ///     {
        ///         new Claim(ClaimTypes.Name, userName),
        ///         new Claim(ClaimTypes.Role, "Admin"),
        ///         new Claim("CustomClaim", "CustomValue")
        ///     };
        ///     return Task.FromResult(claims.ToArray());
        /// }
        /// </code>
        /// </example>
        Task<Claim[]> GetCurrentUserClaims(string userName);
    }
}
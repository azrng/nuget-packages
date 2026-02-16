using System.Security.Claims;

namespace Azrng.AspNetCore.Authentication.Basic
{
    /// <summary>
    /// Basic 认证的默认验证实现
    /// </summary>
    /// <remarks>
    /// 此实现仅返回包含用户名的 Claim，适用于简单场景
    /// 如需自定义 Claims（如添加角色、权限等），请实现 <see cref="IBasicAuthorizeVerify"/> 接口
    /// </remarks>
    public class DefaultBasicAuthorizeVerify : IBasicAuthorizeVerify
    {
        /// <summary>
        /// 获取当前用户的 Claims
        /// </summary>
        /// <param name="userName">用户名</param>
        /// <returns>包含用户名的 Claim 数组</returns>
        public Task<Claim[]> GetCurrentUserClaims(string userName)
        {
            return Task.FromResult(new[] { new Claim(ClaimTypes.Name, userName) });
        }
    }
}
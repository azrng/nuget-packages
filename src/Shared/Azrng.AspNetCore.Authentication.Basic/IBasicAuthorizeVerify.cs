using System.Security.Claims;

namespace Azrng.AspNetCore.Authentication.Basic
{
    /// <summary>
    /// Basic认证验证接口
    /// </summary>
    public interface IBasicAuthorizeVerify
    {
        /// <summary>
        /// 返回当前用户信息的Claim
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        Task<Claim[]> GetCurrentUserClaims(string userName);
    }
}
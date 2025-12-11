using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace Azrng.AspNetCore.Authentication.Basic
{
    /// <summary>
    /// Basic 认证验证通过的自定义实现
    /// </summary>
    public class DefaultBasicAuthorizeVerify : IBasicAuthorizeVerify
    {
        private readonly BasicOptions _basic;

        public DefaultBasicAuthorizeVerify(IOptions<BasicOptions> basicAuthentication)
        {
            _basic = basicAuthentication.Value;
        }

        public Task<Claim[]> GetCurrentUserClaims(string userName)
        {
            return Task.FromResult(new[] { new Claim(ClaimTypes.Name, userName) });
        }
    }
}
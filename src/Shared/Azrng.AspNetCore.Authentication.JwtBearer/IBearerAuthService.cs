using System.Security.Claims;

namespace Azrng.AspNetCore.Authentication.JwtBearer
{
    /// <summary>
    /// jwt认证服务
    /// </summary>
    public interface IBearerAuthService
    {
        /// <summary>
        /// 生成token
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        string CreateToken(string userId);

        /// <summary>
        /// 生成token
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        string CreateToken(string userId, string userName);

        /// <summary>
        /// 生成token
        /// </summary>
        /// <param name="claims"></param>
        /// <returns></returns>
        string CreateToken(IEnumerable<Claim> claims);

        /// <summary>
        /// 验证token
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        bool ValidateToken(string token);

        /// <summary>
        /// 返回解析的NameIdentifier
        /// </summary>
        /// <param name="jwtStr"></param>
        /// <returns></returns>
        string GetJwtNameIdentifier(string jwtStr);

        /// <summary>
        /// 解析token返回载荷信息
        /// </summary>
        /// <param name="jwtStr"></param>
        /// <returns></returns>
        IDictionary<string, string> GetJwtInfo(string jwtStr);
    }
}
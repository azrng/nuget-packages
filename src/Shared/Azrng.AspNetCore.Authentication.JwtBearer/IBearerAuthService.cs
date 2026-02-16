using System.Security.Claims;

namespace Azrng.AspNetCore.Authentication.JwtBearer
{
    /// <summary>
    /// JWT Bearer 认证服务接口
    /// </summary>
    public interface IBearerAuthService
    {
        /// <summary>
        /// 生成 JWT Token（仅包含用户ID）
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>JWT Token 字符串</returns>
        string CreateToken(string userId);

        /// <summary>
        /// 生成 JWT Token（包含用户ID和用户名）
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="userName">用户名</param>
        /// <returns>JWT Token 字符串</returns>
        string CreateToken(string userId, string userName);

        /// <summary>
        /// 生成 JWT Token（自定义声明）
        /// </summary>
        /// <param name="claims">自定义声明集合</param>
        /// <returns>JWT Token 字符串</returns>
        string CreateToken(IEnumerable<Claim> claims);

        /// <summary>
        /// 验证 Token 是否有效（包括签名、过期时间、颁发者、受众）
        /// </summary>
        /// <param name="token">要验证的 JWT Token</param>
        /// <returns>验证成功返回 true，否则返回 false</returns>
        bool ValidateToken(string token);

        /// <summary>
        /// 从 Token 中获取用户标识（NameIdentifier）
        /// </summary>
        /// <param name="jwtStr">JWT Token 字符串</param>
        /// <returns>用户标识，如果不存在则返回空字符串</returns>
        string? GetJwtNameIdentifier(string jwtStr);

        /// <summary>
        /// 解析 Token 返回所有载荷信息
        /// </summary>
        /// <param name="jwtStr">JWT Token 字符串</param>
        /// <returns>载荷键值对字典</returns>
        IDictionary<string, string> GetJwtInfo(string jwtStr);
    }
}
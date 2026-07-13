using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Azrng.AspNetCore.Authentication.JwtBearer
{
    /// <summary>
    /// 构建共享的 <see cref="TokenValidationParameters"/>，确保中间件与
    /// <see cref="IBearerAuthService.ValidateToken"/> 使用完全一致的校验规则
    /// </summary>
    internal static class JwtTokenValidationParametersFactory
    {
        /// <summary>
        /// 基于配置创建一份 Token 校验参数
        /// </summary>
        /// <remarks>调用方每次获得独立的实例，可安全修改而不影响其他路径</remarks>
        public static TokenValidationParameters Create(JwtTokenConfig config, SymmetricSecurityKey securityKey)
        {
            return new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = securityKey,
                ValidateIssuer = true,
                ValidIssuer = config.JwtIssuer,
                ValidateAudience = true,
                ValidAudience = config.JwtAudience,
                RequireExpirationTime = true,
                ValidateLifetime = true,
                // 移除默认 5 分钟时钟偏差容错，使过期时间精确
                ClockSkew = TimeSpan.Zero
            };
        }

        /// <summary>
        /// 从密钥字符串构建 <see cref="SymmetricSecurityKey"/>
        /// </summary>
        public static SymmetricSecurityKey CreateSecurityKey(string secretKey)
            => new(Encoding.UTF8.GetBytes(secretKey));
    }
}

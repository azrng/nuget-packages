namespace Azrng.AspNetCore.Authentication.JwtBearer
{
    /// <summary>
    /// JWT Token 配置选项
    /// </summary>
    public class JwtTokenConfig
    {
        /// <summary>
        /// JWT 签名密钥
        /// </summary>
        /// <remarks>
        /// 无默认值，必须显式提供。生产环境应从安全配置中心或 Secret Manager 读取，
        /// 禁止硬编码到源码或 appsettings 中。密钥长度最低 32 位字符，建议使用
        /// 更长的随机字符串（至少包含 8 种不同字符）。
        /// </remarks>
        public string JwtSecretKey { get; set; } = string.Empty;

        /// <summary>
        /// JWT 颁发者标识
        /// </summary>
        public string JwtIssuer { get; set; } = "issuer";

        /// <summary>
        /// JWT 受众标识
        /// </summary>
        public string JwtAudience { get; set; } = "audience";

        /// <summary>
        /// Token 有效期(默认24小时)
        /// </summary>
        public TimeSpan ValidTime { get; set; } = TimeSpan.FromHours(24);
    }
}

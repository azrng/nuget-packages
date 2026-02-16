namespace Azrng.AspNetCore.Authentication.JwtBearer
{
    /// <summary>
    /// JWT Token 配置选项
    /// </summary>
    public class JwtTokenConfig
    {
        /// <summary>
        /// JWT 签名密钥(包含默认秘钥)
        /// </summary>
        /// <remarks>密钥长度最低 16 位，建议使用更长的随机字符串</remarks>
        public string JwtSecretKey { get; set; } = "SecretKeyOfDoomThatMustBeAMinimumNumberOfBytes";

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
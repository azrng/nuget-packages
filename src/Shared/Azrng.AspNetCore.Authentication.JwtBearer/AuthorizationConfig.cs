namespace Azrng.AspNetCore.Authentication.JwtBearer
{
    /// <summary>
    /// JWT Token配置
    /// </summary>
    public class JwtTokenConfig
    {
        /// <summary>
        /// 密钥
        /// </summary>
        /// <remarks>密钥长度太短会报出异常，最低16位</remarks>
        public string JwtSecretKey { get; set; } = "SecretKeyOfDoomThatMustBeAMinimumNumberOfBytes";

        /// <summary>
        /// 颁发者
        /// </summary>
        public string JwtIssuer { get; set; } = "issuer";

        /// <summary>
        /// 受理者
        /// </summary>
        public string JwtAudience { get; set; } = "audience";

        /// <summary>
        /// 有效期
        /// </summary>
        public TimeSpan ValidTime { get; set; } = TimeSpan.FromHours(12);
    }
}
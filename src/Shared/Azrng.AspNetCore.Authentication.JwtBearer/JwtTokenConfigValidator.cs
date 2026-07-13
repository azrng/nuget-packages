using Microsoft.Extensions.Options;

namespace Azrng.AspNetCore.Authentication.JwtBearer
{
    /// <summary>
    /// JWT 配置校验器，确保无论通过哪条注册路径都会强制校验密钥安全
    /// </summary>
    /// <remarks>
    /// 注册到 DI 容器后，配置在首次解析 <c>IOptions&lt;JwtTokenConfig&gt;</c> 时被校验，
    /// 配合 <c>ValidateOnStart</c> 可在应用启动时即暴露配置问题。
    /// </remarks>
    public class JwtTokenConfigValidator : IValidateOptions<JwtTokenConfig>
    {
        /// <inheritdoc />
        public ValidateOptionsResult Validate(string? name, JwtTokenConfig options)
        {
            if (options == null)
                return ValidateOptionsResult.Fail("JWT 配置不能为空");

            if (string.IsNullOrWhiteSpace(options.JwtSecretKey))
                return ValidateOptionsResult.Fail("JWT 密钥不能为空");

            if (options.JwtSecretKey.Length < 32)
                return ValidateOptionsResult.Fail("JWT 密钥长度必须至少为 32 位字符，以确保安全性");

            // 检查密钥复杂度（避免全为相同字符或简单模式）
            if (options.JwtSecretKey.Distinct().Count() < 8)
                return ValidateOptionsResult.Fail("JWT 密钥复杂度不足，请使用包含多种字符的密钥");

            return ValidateOptionsResult.Success;
        }
    }
}

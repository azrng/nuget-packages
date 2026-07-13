using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Azrng.AspNetCore.Authentication.JwtBearer
{
    /// <summary>
    /// JWT Bearer 认证服务实现
    /// </summary>
    public class JwtBearerAuthService : IBearerAuthService
    {
        private static readonly JwtSecurityTokenHandler _tokenHandler = new();

        private readonly IOptionsMonitor<JwtTokenConfig> _optionsMonitor;

        public JwtBearerAuthService(IOptionsMonitor<JwtTokenConfig> optionsMonitor)
        {
            _optionsMonitor = optionsMonitor ?? throw new ArgumentNullException(nameof(optionsMonitor));
        }

        /// <summary>
        /// 当前配置（每次访问读取最新值，支持热更新）
        /// </summary>
        private JwtTokenConfig Config => _optionsMonitor.CurrentValue;

        /// <summary>
        /// 基于当前密钥构建签名凭证，每次按需创建（密钥可能变化）
        /// </summary>
        private SigningCredentials CreateSigningCredentials()
        {
            var securityKey = JwtTokenValidationParametersFactory.CreateSecurityKey(Config.JwtSecretKey);
            return new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        }

        public string CreateToken(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("用户ID不能为空", nameof(id));

            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, id), };
            return CreateToken(claims);
        }

        public string CreateToken(string id, string userName)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("用户ID不能为空", nameof(id));
            if (string.IsNullOrEmpty(userName))
                throw new ArgumentException("用户名不能为空", nameof(userName));

            var claims = new List<Claim>
                         {
                             new Claim(ClaimTypes.Name, userName),
                             new Claim(ClaimTypes.NameIdentifier, id)
                         };
            return CreateToken(claims);
        }

        public string CreateToken(IEnumerable<Claim> claims)
        {
            if (claims == null)
                throw new ArgumentNullException(nameof(claims));

            var config = Config;
            var token = new JwtSecurityToken(
                issuer: config.JwtIssuer,
                audience: config.JwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.Add(config.ValidTime),
                signingCredentials: CreateSigningCredentials());

            // 直接调用；WriteToken 内部只做序列化，原始异常应向上冒泡便于诊断
            return _tokenHandler.WriteToken(token);
        }

        public bool ValidateToken(string token)
        {
            if (string.IsNullOrEmpty(token))
                throw new ArgumentException("Token 不能为空", nameof(token));

            var config = Config;
            var securityKey = JwtTokenValidationParametersFactory.CreateSecurityKey(config.JwtSecretKey);
            var tokenValidationParameters = JwtTokenValidationParametersFactory.Create(config, securityKey);

            try
            {
                _tokenHandler.ValidateToken(token, tokenValidationParameters, out _);
                return true;
            }
            catch (SecurityTokenException)
            {
                // 仅吞掉 Token 校验类异常；编程错误（如 null）由前面的参数校验抛出
                return false;
            }
        }

        public string? GetJwtNameIdentifier(string jwtStr)
        {
            if (string.IsNullOrEmpty(jwtStr))
                throw new ArgumentException("Token 不能为空", nameof(jwtStr));

            var jwtToken = _tokenHandler.ReadJwtToken(jwtStr);
            return jwtToken.Payload.TryGetValue(ClaimTypes.NameIdentifier, out var value)
                ? value?.ToString()
                : null;
        }

        public IDictionary<string, string> GetJwtInfo(string jwtStr)
        {
            if (string.IsNullOrEmpty(jwtStr))
                throw new ArgumentException("Token 不能为空", nameof(jwtStr));

            var jwtToken = _tokenHandler.ReadJwtToken(jwtStr);
            return jwtToken.Payload.ToDictionary(
                x => x.Key,
                x => x.Value?.ToString() ?? string.Empty);
        }
    }
}

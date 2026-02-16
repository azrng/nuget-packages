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

        private readonly JwtTokenConfig _config;
        private readonly Lazy<SymmetricSecurityKey> _securityKey;
        private readonly Lazy<SigningCredentials> _signingCredentials;

        public JwtBearerAuthService(IOptions<JwtTokenConfig> options)
        {
            _config = options?.Value ?? new JwtTokenConfig();

            // 使用 Lazy 延迟初始化并缓存对象，提高性能
            _securityKey = new Lazy<SymmetricSecurityKey>(() => new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.JwtSecretKey)));
            _signingCredentials =
                new Lazy<SigningCredentials>(() => new SigningCredentials(_securityKey.Value, SecurityAlgorithms.HmacSha256));
        }

        public string CreateToken(string id)
        {
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, id), };
            return CreateToken(claims);
        }

        public string CreateToken(string id, string userName)
        {
            var claims = new List<Claim> { new Claim(ClaimTypes.Name, userName), new Claim(ClaimTypes.NameIdentifier, id), };
            return CreateToken(claims);
        }

        public string CreateToken(IEnumerable<Claim> claims)
        {
            var token = new JwtSecurityToken(issuer: _config.JwtIssuer,
                audience: _config.JwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.Add(_config.ValidTime),
                signingCredentials: _signingCredentials.Value);

            try
            {
                return _tokenHandler.WriteToken(token);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"生成token出错: {ex.Message}", ex);
            }
        }

        public bool ValidateToken(string token)
        {
            try
            {
                var tokenValidationParameters = new TokenValidationParameters
                                                {
                                                    ValidateIssuerSigningKey = true,
                                                    IssuerSigningKey = _securityKey.Value,
                                                    ValidateIssuer = true,
                                                    ValidIssuer = _config.JwtIssuer,
                                                    ValidateAudience = true,
                                                    ValidAudience = _config.JwtAudience,
                                                    ValidateLifetime = true,
                                                    ClockSkew = TimeSpan.Zero // 减少默认的5分钟时钟偏差容错
                                                };

                _tokenHandler.ValidateToken(token, tokenValidationParameters, out _);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public string? GetJwtNameIdentifier(string jwtStr)
        {
            var jwtToken = _tokenHandler.ReadJwtToken(jwtStr);
            return jwtToken.Payload.TryGetValue(ClaimTypes.NameIdentifier, out var value)
                ? value?.ToString()
                : null;
        }

        public IDictionary<string, string> GetJwtInfo(string jwtStr)
        {
            var jwtToken = _tokenHandler.ReadJwtToken(jwtStr);
            return jwtToken?.Payload?.ToDictionary(x => x.Key, x => x.Value.ToString() ?? string.Empty) ?? new Dictionary<string, string>();
        }
    }
}
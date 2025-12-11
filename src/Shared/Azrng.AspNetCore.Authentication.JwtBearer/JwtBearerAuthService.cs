using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Azrng.AspNetCore.Authentication.JwtBearer
{
    public class JwtBearerAuthService : IBearerAuthService
    {
        private readonly JwtTokenConfig _config;

        public JwtBearerAuthService(IOptions<JwtTokenConfig> options)
        {
            _config = options?.Value ?? new JwtTokenConfig();
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
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.JwtSecretKey));
            var token = new JwtSecurityToken(issuer: _config.JwtIssuer,
                audience: _config.JwtAudience,
                claims: claims,
                expires: DateTime.Now.Add(_config.ValidTime),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
            try
            {
                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"生成token出错  {ex.Message}");
            }
        }

        public bool ValidateToken(string token)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.JwtSecretKey));
            var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
            return jwt.RawSignature ==
                   JwtTokenUtilities.CreateEncodedSignature(jwt.RawHeader + "." + jwt.RawPayload, signingCredentials);
        }

        public string GetJwtNameIdentifier(string jwtStr)
        {
            var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(jwtStr);
            return jwtToken.Payload.FirstOrDefault(t => t.Key == ClaimTypes.NameIdentifier).Value?.ToString() ??
                   string.Empty;
        }

        public IDictionary<string, string> GetJwtInfo(string jwtStr)
        {
            var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(jwtStr);
            return jwtToken.Payload.ToDictionary(x => x.Key, x => x.Value.ToString());
        }
    }
}
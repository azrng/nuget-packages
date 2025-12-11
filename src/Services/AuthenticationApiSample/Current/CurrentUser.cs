using Azrng.Core;
using System.Security.Claims;

namespace AuthenticationApiSample.Current
{
    public class CurrentUser : ICurrentUser
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUser(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string Sub => _httpContextAccessor.HttpContext.User?.FindFirst("sub")?.Value;

        public string UserId => _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        public string UserName => _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Name) ?? string.Empty;

        public string NickName => _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.GivenName) ?? string.Empty;

        public List<string> Auds => _httpContextAccessor.HttpContext.User.FindAll("aud").Select(x => x.Value).ToList();

        public string Token => _httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString();
    }
}
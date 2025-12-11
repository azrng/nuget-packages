using Azrng.AspNetCore.Authentication.Basic;
using Azrng.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AuthenticationApiSample.Controllers
{
    /// <summary>
    /// token控制器
    /// </summary>
    [ApiController]
    [Route("[controller]/[action]")]
    public class TokenController : ControllerBase
    {
        private readonly IBearerAuthService _bearerAuthService;
        private readonly ILogger<TokenController> _logger;

        public TokenController(IBearerAuthService bearerAuthService, ILogger<TokenController> logger)
        {
            _bearerAuthService = bearerAuthService;
            _logger = logger;
        }

        /// <summary>
        /// 获取jwt oken
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public string GetToken()
        {
            var claim = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "111111"), new Claim(ClaimTypes.Role, "123456")
            };
            return _bearerAuthService.CreateToken(claim);
        }

        /// <summary>
        /// jwt认证
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize]
        public string TestRequest()
        {
            _logger.LogInformation($"我被请求了  {DateTime.Now}");

            return "success" + DateTime.Now;
        }

        /// <summary>
        /// Basic 认证
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize(AuthenticationSchemes = BasicAuthentication.AuthenticationSchema)]
        public string BasicTestRequest()
        {
            _logger.LogInformation($"我被请求了  {DateTime.Now}");
            var name = HttpContext.User.Claims.FirstOrDefault(t => t.Type == ClaimTypes.Name);
            return "success" + DateTime.Now + " " + name?.Value;
        }
    }
}
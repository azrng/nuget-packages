using Azrng.Core.Results;
using Microsoft.AspNetCore.Authentication;
using Newtonsoft.Json;

namespace AuthenticationApiSample.Auths
{
    /// <summary>
    /// 自定义认证处理器
    /// </summary>
    public class CustomerAuthenticationHandler : IAuthenticationHandler
    {
        /// <summary>
        /// 自定义授权schema
        /// </summary>
        public const string CustomerSchemeName = "customerAuth";

        private AuthenticationScheme _scheme;
        private HttpContext _context;

        public Task<AuthenticateResult> AuthenticateAsync()
        {
            // 根据输入的请求头选择认证schema
            if (_context.Request.Headers.TryGetValue("Authorization", out var values))
            {
                var valStr = values.ToString();
                if (valStr.StartsWith("Bearer "))
                    return _context.AuthenticateAsync("Bearer");
                if (valStr.StartsWith("Basic "))
                    return _context.AuthenticateAsync("Basic");
                else
                    return Task.FromResult(AuthenticateResult.Fail("未登陆"));
            }

// #if DEBUG //开发模式默认登录用户是admin,用于快速调试
//             var claimsIdentity = new ClaimsIdentity(new Claim[]
//            {
//                 new Claim("name", "admin"),
//                 new Claim("nickname", "超级管理员"),
//                 new Claim("role", "admin"),
//                 new Claim("sub", "1"),
//            }, CustomerSchemeName);
//             var ticket = new AuthenticationTicket(new ClaimsPrincipal(claimsIdentity), _scheme.Name);
//             return Task.FromResult(AuthenticateResult.Success(ticket));
// #endif

            return Task.FromResult(AuthenticateResult.Fail("未登陆"));
        }

        public async Task ChallengeAsync(AuthenticationProperties? properties)
        {
            _context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await _context.Response.WriteAsync(
                JsonConvert.SerializeObject(ResultModel<string>.Error("您无权访问该接口，请确保已经登录", "401")));
        }

        public async Task ForbidAsync(AuthenticationProperties? properties)
        {
            _context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            _context.Response.ContentType = "application/json";
            await _context.Response.WriteAsync(
                JsonConvert.SerializeObject(ResultModel<string>.Error("您的访问权限不够，请联系管理员", "401")));
        }

        public Task InitializeAsync(AuthenticationScheme scheme, HttpContext context)
        {
            _scheme = scheme;
            _context = context;
            return Task.CompletedTask;
        }
    }
}
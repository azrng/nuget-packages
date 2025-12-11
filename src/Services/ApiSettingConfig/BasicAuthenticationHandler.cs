using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace ApiSettingConfig
{
    public class BasicAuthenticationSchemeOptions : AuthenticationSchemeOptions
    {
        public string UserName { get; set; }

        public string Password { get; set; }

        public Func<HttpContext, string, string, Task<bool>> UserCredentialValidator { get; set; }
            = (context, user, pass) =>
            {
                var options = context.RequestServices.GetRequiredService<IOptions<BasicAuthenticationSchemeOptions>>()
                    .Value;
                return Task.FromResult(user == options.UserName && pass == options.Password);
            };
    }

    public class BasicAuthenticationHandler : AuthenticationHandler<BasicAuthenticationSchemeOptions>
    {
        public BasicAuthenticationHandler(IOptionsMonitor<BasicAuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
            {
                Response.StatusCode = 401;
                Response.Headers["WWW-Authenticate"] = "Basic";
                return AuthenticateResult.Fail("Missing or invalid Authorization header.");
            }

            var authorization = Request.Headers["Authorization"].ToString().Split(' ');
            if (authorization.Length != 2 || authorization[0].Trim() != "Basic")
            {
                return AuthenticateResult.Fail("Invalid authentication header");
            }

            try
            {
                var credentialString = Encoding.UTF8.GetString(Convert.FromBase64String(authorization[1]));
                var credentials = credentialString.Split(':');
                var username = credentials[0];
                var password = credentials[1];

                // 验证用户凭据
                var valid = await Options.UserCredentialValidator.Invoke(Request.HttpContext, username, password);
                if (valid)
                {
                    var claims = new[] { new Claim(ClaimTypes.Name, username) };
                    var identity = new ClaimsIdentity(claims, Scheme.Name);
                    var principal = new ClaimsPrincipal(identity);
                    var ticket = new AuthenticationTicket(principal, Scheme.Name);

                    return AuthenticateResult.Success(ticket);
                }
                else
                {
                    Response.StatusCode = 401;
                    Response.Headers["WWW-Authenticate"] = "Basic";
                    return AuthenticateResult.Fail("Invalid username or password.");
                }
            }
            catch
            {
                // 处理解码或验证错误
                return AuthenticateResult.Fail("Invalid credentials");
            }
        }
    }
}
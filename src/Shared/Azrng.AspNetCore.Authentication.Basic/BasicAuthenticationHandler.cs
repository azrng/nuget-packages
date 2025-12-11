using Azrng.Core;
using Azrng.Core.Results;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace Azrng.AspNetCore.Authentication.Basic;

/// <summary>
/// 继承已实现的基类
/// </summary>
public class BasicAuthenticationHandler : AuthenticationHandler<BasicOptions>
{
    private readonly ILogger<BasicAuthenticationHandler> _logger;
    private readonly IBasicAuthorizeVerify _basicAuthorizeVerify;
    private readonly IJsonSerializer _jsonSerializer;

    public BasicAuthenticationHandler(IOptionsMonitor<BasicOptions> options,
                                      ILoggerFactory logger,
                                      UrlEncoder encoder,
                                      ISystemClock clock, IBasicAuthorizeVerify basicAuthorizeVerify,
                                      IJsonSerializer jsonSerializer) : base(options, logger, encoder, clock)
    {
        _basicAuthorizeVerify = basicAuthorizeVerify;
        _jsonSerializer = jsonSerializer;
        _logger = logger.CreateLogger<BasicAuthenticationHandler>();
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.ContainsKey("Authorization"))
        {
            return AuthenticateResult.Fail("未标注Authorization请求头。");
        }

        var authHeader = Request.Headers["Authorization"].ToString();
        if (!authHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
        {
            return AuthenticateResult.Fail("Authorization请求头格式不正确。");
        }

        var base64EncodedValue = authHeader["Basic ".Length..];
        if (string.IsNullOrWhiteSpace(base64EncodedValue))
        {
            return AuthenticateResult.Fail("无效的认证头");
        }

        string userName, password;
        try
        {
            var userNamePassword = Encoding.UTF8.GetString(Convert.FromBase64String(base64EncodedValue));
            var userNamePasswordArray = userNamePassword.Split(':');
            if (userNamePasswordArray.Length != 2)
            {
                return AuthenticateResult.Fail("无效的认证头");
            }

            userName = userNamePasswordArray[0];
            password = userNamePasswordArray[1];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"转换格式报错，错误信息：{ex.Message}");
            return AuthenticateResult.Fail("认证头格式无效");
        }

        var valid = await Options.UserCredentialValidator.Invoke(Request.HttpContext, userName, password);
        if (!valid)
            return AuthenticateResult.Fail("无效用户名或密码。");

        var claims = await _basicAuthorizeVerify.GetCurrentUserClaims(userName);

        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, Scheme.Name));
        var ticket = new AuthenticationTicket(claimsPrincipal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }

    protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.ContentType = "application/json";
        Response.StatusCode = StatusCodes.Status401Unauthorized;
        await Response.WriteAsync(_jsonSerializer.ToJson(ResultModel<string>.Error("您无权访问该接口，请确保已经登录", "401")));
    }

    protected override async Task HandleForbiddenAsync(AuthenticationProperties properties)
    {
        Response.ContentType = "application/json";
        Response.StatusCode = StatusCodes.Status403Forbidden;
        await Response.WriteAsync(_jsonSerializer.ToJson(ResultModel<string>.Error("您的访问权限不够，请联系管理员", "401")));
    }
}
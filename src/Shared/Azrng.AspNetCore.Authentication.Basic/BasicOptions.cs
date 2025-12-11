using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Azrng.AspNetCore.Authentication.Basic;

/// <summary>
/// Basic认证类  自定义认证类
/// </summary>
public class BasicOptions : AuthenticationSchemeOptions
{
    /// <summary>
    /// 用户名
    /// </summary>
    public string UserName { get; set; } = null!;

    /// <summary>
    /// 密码
    /// </summary>
    public string Password { get; set; } = null!;

    public Func<HttpContext, string, string, Task<bool>> UserCredentialValidator { get; set; }
        = (context, user, pass) =>
        {
            var options = context.RequestServices.GetRequiredService<IOptions<BasicOptions>>()
                                 .Value;
            return Task.FromResult(user == options.UserName && pass == options.Password);
        };
}
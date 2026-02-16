using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Azrng.AspNetCore.Authentication.Basic;

/// <summary>
/// Basic 认证配置选项
/// </summary>
public class BasicOptions : AuthenticationSchemeOptions
{
    /// <summary>
    /// 默认用户名
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// 默认密码
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// 用户凭据验证器（用于验证用户名和密码是否正确）
    /// </summary>
    /// <remarks>
    /// 默认实现会验证传入的用户名和密码是否与配置的 UserName 和 Password 匹配
    /// 你可以替换此验证器以实现自定义的验证逻辑（如数据库验证）
    /// </remarks>
    public Func<HttpContext, string, string, Task<bool>> UserCredentialValidator { get; set; } =
        (context, userName, password) =>
        {
            // 默认验证逻辑：从 Options 中获取配置的用户名和密码进行验证
            var options = context.RequestServices.GetRequiredService<IOptionsMonitor<BasicOptions>>()
                .Get(BasicAuthentication.AuthenticationSchema);
            return Task.FromResult(userName == options.UserName && password == options.Password);
        };
}
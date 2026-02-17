using Azrng.SettingConfig.Service;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Azrng.SettingConfig.BasicAuthorization;

/// <summary>
/// SettingConfig 配置中心的 Basic 认证授权过滤器
/// </summary>
/// <remarks>
/// 如果与其他安全中间件（如 OWIN 安全）一起使用，请确保在配置其他安全中间件之前配置 SettingConfig
/// </remarks>
public class BasicAuthAuthorizationFilter : IDashboardAuthorizationFilter
{
    private const int UnauthorizedStatusCode = 401;
    private const int MovedPermanentlyStatusCode = 301;
    private const string WwwAuthenticateHeader = "WWW-Authenticate";
    private const string BasicRealm = "Basic realm=\"SettingConfig Dashboard\"";

    private readonly BasicAuthAuthorizationFilterOptions _options;

    /// <summary>
    /// 初始化 <see cref="BasicAuthAuthorizationFilter"/> 的新实例，使用默认选项
    /// </summary>
    public BasicAuthAuthorizationFilter()
        : this(new BasicAuthAuthorizationFilterOptions())
    {
    }

    /// <summary>
    /// 初始化 <see cref="BasicAuthAuthorizationFilter"/> 的新实例
    /// </summary>
    /// <param name="options">认证选项</param>
    public BasicAuthAuthorizationFilter(BasicAuthAuthorizationFilterOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// 发送 401 未授权挑战响应
    /// </summary>
    /// <param name="context">HTTP 上下文</param>
    /// <returns>始终返回 false</returns>
    private static bool Challenge(HttpContext context)
    {
        context.Response.StatusCode = UnauthorizedStatusCode;
        context.Response.Headers.Append(WwwAuthenticateHeader, BasicRealm);
        return false;
    }

    /// <summary>
    /// 授权请求
    /// </summary>
    /// <param name="dashboardContext">Dashboard 上下文</param>
    /// <returns>授权成功返回 true，否则返回 false</returns>
    public bool Authorize(DashboardContext dashboardContext)
    {
        var context = dashboardContext.GetHttpContext();

        // SSL 重定向处理
        if (_options.SslRedirect && context.Request.Scheme != "https")
        {
            var redirectUri = new UriBuilder("https", context.Request.Host.ToString(), 443, context.Request.Path)
                .ToString();

            context.Response.StatusCode = MovedPermanentlyStatusCode;
            context.Response.Redirect(redirectUri);
            return false;
        }

        // SSL 要求检查
        if (_options.RequireSsl && !context.Request.IsHttps)
        {
            return false;
        }

        // 获取 Authorization 头
        var header = context.Request.Headers["Authorization"].ToString();

        if (string.IsNullOrWhiteSpace(header))
        {
            return Challenge(context);
        }

        // 解析 Authorization 头
        if (!AuthenticationHeaderValue.TryParse(header, out var authValues))
        {
            return Challenge(context);
        }

        if (!"Basic".Equals(authValues.Scheme, StringComparison.OrdinalIgnoreCase))
        {
            return Challenge(context);
        }

        // 解码 Base64 凭据
        if (string.IsNullOrWhiteSpace(authValues.Parameter))
        {
            return Challenge(context);
        }

        string parameter;
        try
        {
            parameter = Encoding.UTF8.GetString(Convert.FromBase64String(authValues.Parameter));
        }
        catch (FormatException)
        {
            return Challenge(context);
        }

        // 分割用户名和密码（正确处理密码中包含冒号的情况）
        var colonIndex = parameter.IndexOf(':');
        if (colonIndex <= 0) // 用户名不能为空
        {
            return Challenge(context);
        }

        var login = parameter.Substring(0, colonIndex);
        var password = parameter.Substring(colonIndex + 1);

        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
        {
            return Challenge(context);
        }

        // 验证用户凭据
        var isValidUser = _options.Users.Any(user => user.Validate(login, password, _options.LoginCaseSensitive));
        return isValidUser || Challenge(context);
    }
}
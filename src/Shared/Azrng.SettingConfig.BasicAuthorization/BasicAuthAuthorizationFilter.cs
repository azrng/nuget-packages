using Azrng.SettingConfig.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
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
    private const int DefaultHttpsPort = 443;
    private const string WwwAuthenticateHeader = "WWW-Authenticate";
    private const string BasicRealm = "Basic realm=\"SettingConfig Dashboard\"";
    private const string HttpsScheme = "https";
    private const string BasicScheme = "Basic";

    private readonly BasicAuthAuthorizationFilterOptions _options;
    private readonly ILogger<BasicAuthAuthorizationFilter>? _logger;

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
        : this(options, null)
    {
    }

    /// <summary>
    /// 初始化 <see cref="BasicAuthAuthorizationFilter"/> 的新实例（带日志支持）
    /// </summary>
    /// <param name="options">认证选项</param>
    /// <param name="logger">日志记录器</param>
    internal BasicAuthAuthorizationFilter(
        BasicAuthAuthorizationFilterOptions options,
        ILogger<BasicAuthAuthorizationFilter>? logger)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger;
    }

    /// <summary>
    /// 发送 401 未授权挑战响应
    /// </summary>
    /// <param name="context">HTTP 上下文</param>
    /// <param name="reason">拒绝原因（用于日志记录）</param>
    /// <returns>始终返回 false</returns>
    private bool Challenge(HttpContext context, string reason = "未提供有效的凭据")
    {
        context.Response.StatusCode = UnauthorizedStatusCode;
        context.Response.Headers.Append(WwwAuthenticateHeader, BasicRealm);

        // 记录失败的认证尝试
        if (_logger != null && _logger.IsEnabled(LogLevel.Warning))
        {
            var remoteIp = context.Connection.RemoteIpAddress?.ToString() ?? "未知";
            _logger.LogWarning(
                "认证失败: {Reason}, IP: {RemoteIP}, User-Agent: {UserAgent}",
                reason,
                remoteIp,
                context.Request.Headers["UserAgent"].ToString());
        }

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
        var remoteIp = context.Connection.RemoteIpAddress?.ToString() ?? "未知";

        // SSL 重定向处理
        if (_options.SslRedirect && context.Request.Scheme != HttpsScheme)
        {
            var redirectUri = new UriBuilder(
                HttpsScheme,
                context.Request.Host.ToString(),
                DefaultHttpsPort,
                context.Request.Path)
                .ToString();

            context.Response.Redirect(redirectUri, permanent: true);

            _logger?.LogInformation(
                "SSL 重定向: 从 {Scheme} 重定向到 HTTPS, IP: {RemoteIP}",
                context.Request.Scheme,
                remoteIp);

            return false;
        }

        // SSL 要求检查
        if (_options.RequireSsl && !context.Request.IsHttps)
        {
            return Challenge(context, "要求 SSL 连接");
        }

        // 获取并验证 Authorization 头
        var header = context.Request.Headers["Authorization"].ToString();

        if (string.IsNullOrWhiteSpace(header) ||
            !AuthenticationHeaderValue.TryParse(header, out var authValues) ||
            !BasicScheme.Equals(authValues.Scheme, StringComparison.OrdinalIgnoreCase))
        {
            return Challenge(context, "无效的 Authorization 头或认证方案");
        }

        // 解码 Base64 凭据
        if (string.IsNullOrWhiteSpace(authValues.Parameter))
        {
            return Challenge(context, "凭据参数为空");
        }

        string parameter;
        try
        {
            parameter = Encoding.UTF8.GetString(Convert.FromBase64String(authValues.Parameter));
        }
        catch (FormatException)
        {
            return Challenge(context, "凭据格式无效");
        }

        // 分割用户名和密码（正确处理密码中包含冒号的情况）
        var colonIndex = parameter.IndexOf(':');
        if (colonIndex <= 0) // 用户名不能为空
        {
            return Challenge(context, "凭据格式错误（缺少用户名或密码）");
        }

        var login = parameter.Substring(0, colonIndex);
        var password = parameter.Substring(colonIndex + 1);

        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
        {
            return Challenge(context, "用户名或密码为空");
        }

        // 验证用户凭据
        var isValidUser = _options.Users.Any(user => user.Validate(login, password, _options.LoginCaseSensitive));

        if (isValidUser)
        {
            _logger?.LogInformation("认证成功: 用户名 {Login}, IP: {RemoteIP}", login, remoteIp);
            return true;
        }
        else
        {
            return Challenge(context, $"用户名或密码错误（尝试的用户名: {login}）");
        }
    }
}

namespace Azrng.SettingConfig.BasicAuthorization;

/// <summary>
/// Basic 认证授权过滤器选项
/// </summary>
public class BasicAuthAuthorizationFilterOptions
{
    /// <summary>
    /// 初始化 <see cref="BasicAuthAuthorizationFilterOptions"/> 的新实例
    /// </summary>
    public BasicAuthAuthorizationFilterOptions()
    {
        SslRedirect = true;
        RequireSsl = true;
        LoginCaseSensitive = true;
        Users = Array.Empty<BasicAuthAuthorizationUser>();
    }

    /// <summary>
    /// 获取或设置是否将所有非 SSL 请求重定向到 SSL URL
    /// </summary>
    public bool SslRedirect { get; set; }

    /// <summary>
    /// 获取或设置是否要求 SSL 连接才能访问配置中心
    /// </summary>
    /// <remarks>
    /// 使用 Basic 认证时强烈建议启用 SSL，以避免凭据在网络上明文传输
    /// </remarks>
    public bool RequireSsl { get; set; }

    /// <summary>
    /// 获取或设置登录名验证是否区分大小写
    /// </summary>
    public bool LoginCaseSensitive { get; set; }

    /// <summary>
    /// 获取或设置允许访问配置中心的用户列表
    /// </summary>
    public IEnumerable<BasicAuthAuthorizationUser> Users { get; set; }
}

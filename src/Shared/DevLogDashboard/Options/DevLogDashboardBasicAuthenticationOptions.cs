namespace Azrng.DevLogDashboard.Options;

/// <summary>
/// DevLogDashboard Basic 认证配置
/// </summary>
public class DevLogDashboardBasicAuthenticationOptions
{
    /// <summary>
    /// Basic 认证用户名
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Basic 认证密码
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// 浏览器弹窗显示的 Realm
    /// </summary>
    public string Realm { get; set; } = "DevLogDashboard";
}

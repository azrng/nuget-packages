namespace Azrng.DevLogDashboard.Options;

/// <summary>
/// DevLogDashboard Basic 认证配置
/// </summary>
public class DevLogDashboardBasicAuthenticationOptions
{
    /// <summary>
    /// 认证方案名称
    /// </summary>
    public string Scheme { get; set; } = "Basic";

    /// <summary>
    /// 浏览器弹窗显示的 Realm
    /// </summary>
    public string Realm { get; set; } = "DevLogDashboard";
}

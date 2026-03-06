namespace DevLogDashboard;

/// <summary>
/// DevLogDashboard 常量定义
/// </summary>
public static class DevLogDashboardConsts
{
    /// <summary>
    /// 默认仪表板路径
    /// </summary>
    public const string DefaultEndpointPath = "/dev-logs";

    /// <summary>
    /// 默认 API 路径前缀
    /// </summary>
    public const string DefaultApiPrefix = "/dev-logs-api";

    /// <summary>
    /// 默认最大日志数量
    /// </summary>
    public const int DefaultMaxLogCount = 10000;

    /// <summary>
    /// 默认日志保留时间（小时）
    /// </summary>
    public const int DefaultRetentionHours = 24;
}

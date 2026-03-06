namespace DevLogDashboard.Models;

/// <summary>
/// 仪表板首页数据模型
/// </summary>
public class DashboardModel
{
    /// <summary>
    /// 总日志数
    /// </summary>
    public int TotalLogs { get; set; }

    /// <summary>
    /// 错误日志数
    /// </summary>
    public int ErrorLogs { get; set; }

    /// <summary>
    /// 警告日志数
    /// </summary>
    public int WarningLogs { get; set; }

    /// <summary>
    /// 信息日志数
    /// </summary>
    public int InformationLogs { get; set; }

    /// <summary>
    /// 各级别日志统计
    /// </summary>
    public Dictionary<string, int> LevelStatistics { get; set; } = new();

    /// <summary>
    /// 最近错误日志列表
    /// </summary>
    public List<LogEntry> RecentErrors { get; set; } = new();

    /// <summary>
    /// 日志趋势数据（按小时）
    /// </summary>
    public List<HourlyLogCount> HourlyCounts { get; set; } = new();
}

/// <summary>
/// 每小时日志数量
/// </summary>
public class HourlyLogCount
{
    public string Hour { get; set; } = string.Empty;
    public int Count { get; set; }
    public int ErrorCount { get; set; }
    public int WarningCount { get; set; }
}

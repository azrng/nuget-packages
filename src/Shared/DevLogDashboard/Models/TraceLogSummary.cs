namespace DevLogDashboard;

/// <summary>
/// 追踪日志汇总信息
/// </summary>
public class TraceLogSummary
{
    /// <summary>
    /// RequestId
    /// </summary>
    public string RequestId { get; set; } = string.Empty;

    /// <summary>
    /// 日志数量
    /// </summary>
    public int LogCount { get; set; }

    /// <summary>
    /// 最早时间
    /// </summary>
    public DateTime FirstTimestamp { get; set; }

    /// <summary>
    /// 最晚时间
    /// </summary>
    public DateTime LastTimestamp { get; set; }

    /// <summary>
    /// 耗时（毫秒）
    /// </summary>
    public double Duration => (LastTimestamp - FirstTimestamp).TotalMilliseconds;

    /// <summary>
    /// 请求路径
    /// </summary>
    public string? RequestPath { get; set; }

    /// <summary>
    /// 请求方法
    /// </summary>
    public string? RequestMethod { get; set; }

    /// <summary>
    /// 响应状态码
    /// </summary>
    public int? ResponseStatusCode { get; set; }

    /// <summary>
    /// 是否包含错误
    /// </summary>
    public bool HasError { get; set; }
}

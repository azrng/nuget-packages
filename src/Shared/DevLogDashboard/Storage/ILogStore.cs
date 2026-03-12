using Azrng.DevLogDashboard.Models;

namespace Azrng.DevLogDashboard.Storage;

/// <summary>
/// 日志存储接口
/// </summary>
public interface ILogStore
{
    /// <summary>
    /// 初始化存储（例如创建数据库表、索引等）
    /// </summary>
    Task InitializeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 批量异步添加日志
    /// </summary>
    ValueTask AddBatchAsync(IEnumerable<LogEntry> entries, CancellationToken cancellationToken = default);

    /// <summary>
    /// 查询日志（支持分页、过滤、排序）
    /// </summary>
    Task<PageResult<LogEntry>> QueryAsync(LogQuery query, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据 RequestId 获取日志列表（用于请求追踪）
    /// </summary>
    Task<List<LogEntry>> GetByRequestIdAsync(string requestId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取追踪汇总列表（用于请求分析）
    /// </summary>
    Task<List<TraceLogSummary>> GetTraceSummariesAsync(DateTime? startTime, DateTime? endTime, CancellationToken cancellationToken = default);
}

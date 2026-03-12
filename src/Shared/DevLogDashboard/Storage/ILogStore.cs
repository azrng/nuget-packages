using Azrng.DevLogDashboard.Models;
using Microsoft.Extensions.Logging;

namespace Azrng.DevLogDashboard.Storage;

/// <summary>
/// 日志存储接口
/// </summary>
public interface ILogStore
{
    /// <summary>
    /// 异步添加日志
    /// </summary>
    ValueTask AddAsync(LogEntry entry, CancellationToken cancellationToken = default);

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

    /// <summary>
    /// 清空所有日志
    /// </summary>
    void Clear();

    /// <summary>
    /// 日志总数
    /// </summary>
    int Count { get; }
}

using Azrng.DevLogDashboard.Models;
using Microsoft.Extensions.Logging;

namespace Azrng.DevLogDashboard.Storage;

/// <summary>
/// 日志存储接口
/// </summary>
public interface ILogStore
{
    /// <summary>
    /// 添加日志
    /// </summary>
    void Add(LogEntry entry);

    /// <summary>
    /// 异步添加日志
    /// </summary>
    ValueTask AddAsync(LogEntry entry, CancellationToken cancellationToken = default);

    /// <summary>
    /// 查询日志
    /// </summary>
    Task<PageResult<LogEntry>> QueryAsync(LogQuery query, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据 ID 获取日志
    /// </summary>
    Task<LogEntry?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据 RequestId 获取日志列表
    /// </summary>
    Task<List<LogEntry>> GetByRequestIdAsync(string requestId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取追踪汇总列表
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

    /// <summary>
    /// 获取指定时间范围内的日志总数
    /// </summary>
    int GetCountByTimeRange(DateTime? startTime, DateTime? endTime);

    /// <summary>
    /// 获取各级别日志数量统计
    /// </summary>
    Dictionary<LogLevel, int> GetLogLevelStatistics(DateTime? startTime, DateTime? endTime);

    /// <summary>
    /// 获取最近 N 条错误日志
    /// </summary>
    List<LogEntry> GetRecentErrors(int count, DateTime? startTime, DateTime? endTime);
}

using Azrng.DevLogDashboard.Models;

namespace Azrng.DevLogDashboard.Storage;

/// <summary>
/// 日志存储抽象基类，提供默认实现以简化子类的实现负担
/// </summary>
public abstract class LogStoreBase : ILogStore
{
    /// <summary>
    /// 初始化存储（例如创建数据库表、索引等）
    /// </summary>
    public abstract Task InitializeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 批量异步添加日志（必须实现）
    /// </summary>
    public abstract ValueTask AddBatchAsync(IEnumerable<LogEntry> entries, CancellationToken cancellationToken = default);

    /// <summary>
    /// 异步添加单条日志（默认实现，通过批量方法实现）
    /// </summary>
    public virtual ValueTask AddAsync(LogEntry? entry, CancellationToken cancellationToken = default)
    {
        if (entry == null)
        {
            return ValueTask.CompletedTask;
        }

        // 默认实现：将单条日志包装成批量调用
        return AddBatchAsync(new[] { entry }, cancellationToken);
    }

    /// <summary>
    /// 查询日志（支持分页、过滤、排序）
    /// </summary>
    public abstract Task<PageResult<LogEntry>> QueryAsync(LogQuery query, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据 RequestId 获取日志列表（用于请求追踪）
    /// </summary>
    public abstract Task<List<LogEntry>> GetByRequestIdAsync(string requestId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取追踪汇总列表（用于请求分析）
    /// </summary>
    public abstract Task<List<TraceLogSummary>> GetTraceSummariesAsync(DateTime? startTime, DateTime? endTime, CancellationToken cancellationToken = default);
}

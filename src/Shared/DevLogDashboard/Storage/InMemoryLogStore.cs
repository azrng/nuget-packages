using Azrng.DevLogDashboard.Models;
using Microsoft.Extensions.Logging;

namespace Azrng.DevLogDashboard.Storage;

/// <summary>
/// In-memory log storage with bounded queue and lightweight indexes.
/// </summary>
public class InMemoryLogStore : ILogStore
{
    private readonly Queue<LogEntry> _logs = new();
    private readonly Dictionary<string, LogEntry> _logsById = new(StringComparer.Ordinal);
    private readonly Dictionary<string, List<LogEntry>> _logsByRequestId = new(StringComparer.Ordinal);
    private readonly Dictionary<string, TraceInfo> _traceIndex = new(StringComparer.Ordinal);
    private readonly ReaderWriterLockSlim _lock = new(LockRecursionPolicy.NoRecursion);
    private readonly int _maxLogCount;
    private const int MaxPageSize = 500; // 限制单次查询最大返回数量

    public InMemoryLogStore(int maxLogCount = 10000)
    {
        _maxLogCount = maxLogCount > 0 ? maxLogCount : 10000;
    }

    public Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        // 内存存储无需初始化
        return Task.CompletedTask;
    }

    public ValueTask AddAsync(LogEntry? entry, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return ValueTask.FromCanceled(cancellationToken);
        }

        if (entry is null)
        {
            return ValueTask.CompletedTask;
        }

        _lock.EnterWriteLock();
        try
        {
            _logs.Enqueue(entry);

            if (!string.IsNullOrEmpty(entry.Id))
            {
                _logsById[entry.Id] = entry;
            }

            // 增量更新 Trace 索引
            if (!string.IsNullOrEmpty(entry.RequestId))
            {
                if (!_logsByRequestId.TryGetValue(entry.RequestId, out var requestLogs))
                {
                    requestLogs = new List<LogEntry>();
                    _logsByRequestId[entry.RequestId] = requestLogs;
                }

                requestLogs.Add(entry);

                // 增量更新 Trace 汇总信息
                if (!_traceIndex.TryGetValue(entry.RequestId, out var traceInfo))
                {
                    traceInfo = new TraceInfo
                    {
                        RequestId = entry.RequestId
                    };
                    _traceIndex[entry.RequestId] = traceInfo;
                }

                traceInfo.LogCount++;
                if (entry.Timestamp < traceInfo.FirstTimestamp)
                {
                    traceInfo.FirstTimestamp = entry.Timestamp;
                }
                if (entry.Timestamp > traceInfo.LastTimestamp)
                {
                    traceInfo.LastTimestamp = entry.Timestamp;
                }
                if (string.IsNullOrEmpty(traceInfo.RequestPath) && !string.IsNullOrEmpty(entry.RequestPath))
                {
                    traceInfo.RequestPath = entry.RequestPath;
                }
                if (string.IsNullOrEmpty(traceInfo.RequestMethod) && !string.IsNullOrEmpty(entry.RequestMethod))
                {
                    traceInfo.RequestMethod = entry.RequestMethod;
                }
                // 更新状态码（使用最新的，覆盖之前的）
                if (entry.ResponseStatusCode.HasValue)
                {
                    traceInfo.ResponseStatusCode = entry.ResponseStatusCode.Value;
                }
                if (entry.Level >= LogLevel.Error)
                {
                    traceInfo.HasError = true;
                }
            }

            TrimExcessLogs();
        }
        finally
        {
            _lock.ExitWriteLock();
        }

        return ValueTask.CompletedTask;
    }

    public Task<List<LogEntry>> GetByRequestIdAsync(string requestId, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled<List<LogEntry>>(cancellationToken);
        }

        if (string.IsNullOrEmpty(requestId))
        {
            return Task.FromResult(new List<LogEntry>());
        }

        _lock.EnterReadLock();
        try
        {
            if (!_logsByRequestId.TryGetValue(requestId, out var logs))
            {
                return Task.FromResult(new List<LogEntry>());
            }

            // 已在 Add 时按时间顺序添加，直接返回副本
            return Task.FromResult(logs.ToList());
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public Task<PageResult<LogEntry>> QueryAsync(LogQuery query, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled<PageResult<LogEntry>>(cancellationToken);
        }

        var pageIndex = query.PageIndex <= 0 ? 1 : query.PageIndex;
        var pageSize = Math.Min(query.PageSize <= 0 ? 50 : query.PageSize, MaxPageSize);
        var skip = (pageIndex - 1) * pageSize;

        _lock.EnterReadLock();
        try
        {
            // 优化：如果只需要按时间倒序且无其他过滤条件，直接从队列尾部扫描
            bool canFastPath = query.OrderByTimeAscending == false &&
                              string.IsNullOrEmpty(query.Id) &&
                              string.IsNullOrEmpty(query.Keyword) &&
                              string.IsNullOrEmpty(query.RequestId) &&
                              string.IsNullOrEmpty(query.Source) &&
                              string.IsNullOrEmpty(query.Application) &&
                              !query.MinLevel.HasValue &&
                              !query.StartTime.HasValue &&
                              !query.EndTime.HasValue;

            List<LogEntry> filtered;

            if (canFastPath)
            {
                // 快速路径：从队列尾部倒序扫描，早停
                filtered = new List<LogEntry>();
                var array = _logs.ToArray();
                int targetCount = skip + pageSize;

                // 从最新开始扫描（队列尾部）
                for (int i = array.Length - 1; i >= 0 && filtered.Count < targetCount; i--)
                {
                    filtered.Add(array[i]);
                }

                // 反转回正确顺序（最新的在前）
                filtered.Reverse();
            }
            else
            {
                // 正常路径：需要过滤
                filtered = ApplyFilters(_logs, query).ToList();

                // 排序
                filtered.Sort((a, b) => query.OrderByTimeAscending
                    ? a.Timestamp.CompareTo(b.Timestamp)
                    : b.Timestamp.CompareTo(a.Timestamp));
            }

            var total = filtered.Count;
            var items = filtered
                .Skip(skip)
                .Take(pageSize)
                .ToList();

            return Task.FromResult(PageResult<LogEntry>.Create(items, total, pageIndex, pageSize));
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public Task<List<TraceLogSummary>> GetTraceSummariesAsync(DateTime? startTime, DateTime? endTime,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled<List<TraceLogSummary>>(cancellationToken);
        }

        _lock.EnterReadLock();
        try
        {
            var query = _traceIndex.Values.AsEnumerable();

            // 时间范围过滤
            if (startTime.HasValue)
            {
                query = query.Where(x => x.LastTimestamp >= startTime.Value);
            }

            if (endTime.HasValue)
            {
                query = query.Where(x => x.FirstTimestamp <= endTime.Value);
            }

            var summaries = query
                .Select(x => new TraceLogSummary
                {
                    RequestId = x.RequestId,
                    LogCount = x.LogCount,
                    FirstTimestamp = x.FirstTimestamp,
                    LastTimestamp = x.LastTimestamp,
                    RequestPath = x.RequestPath,
                    RequestMethod = x.RequestMethod,
                    ResponseStatusCode = x.ResponseStatusCode,
                    HasError = x.HasError
                })
                .OrderByDescending(x => x.LastTimestamp)
                .Take(1000)
                .ToList();

            return Task.FromResult(summaries);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    private IEnumerable<LogEntry> ApplyFilters(IEnumerable<LogEntry> logs, LogQuery query)
    {
        var filtered = logs;

        if (!string.IsNullOrEmpty(query.Id))
        {
            filtered = filtered.Where(x => x.Id == query.Id);
        }

        if (query.StartTime.HasValue)
        {
            filtered = filtered.Where(x => x.Timestamp >= query.StartTime.Value);
        }

        if (query.EndTime.HasValue)
        {
            filtered = filtered.Where(x => x.Timestamp <= query.EndTime.Value);
        }

        if (query.MinLevel.HasValue)
        {
            filtered = filtered.Where(x => x.Level >= query.MinLevel.Value);
        }

        if (!string.IsNullOrEmpty(query.RequestId))
        {
            filtered = filtered.Where(x => x.RequestId == query.RequestId);
        }

        if (!string.IsNullOrEmpty(query.Source))
        {
            filtered = filtered.Where(x => x.Source == query.Source);
        }

        if (!string.IsNullOrEmpty(query.Application))
        {
            filtered = filtered.Where(x => x.Application == query.Application);
        }

        if (!string.IsNullOrEmpty(query.Keyword))
        {
            filtered = ApplyKeywordFilter(filtered, query.Keyword);
        }

        return filtered;
    }

    private IEnumerable<LogEntry> ApplyKeywordFilter(IEnumerable<LogEntry> logs, string keyword)
    {
        var filtered = logs;

        keyword = keyword.Trim();

        var andParts = keyword.Split(new[] { " and ", " AND " }, StringSplitOptions.None);

        var conditions = new List<string>();
        foreach (var part in andParts)
        {
            var orParts = part.Split(new[] { " or ", " OR " }, StringSplitOptions.None);
            conditions.AddRange(orParts);
        }

        foreach (var condition in conditions)
        {
            var cond = condition.Trim();
            if (string.IsNullOrEmpty(cond))
            {
                continue;
            }

            if (cond.Contains('=') && !cond.Contains(" like ", StringComparison.OrdinalIgnoreCase))
            {
                var parts = cond.Split('=', 2);
                if (parts.Length == 2)
                {
                    var field = parts[0].Trim().ToLowerInvariant();
                    var value = parts[1].Trim().Trim('\"', '\'');

                    filtered = filtered.Where(x => MatchField(x, field, value, exactMatch: true));
                }
            }
            else if (cond.Contains(" like ", StringComparison.OrdinalIgnoreCase))
            {
                const string likeToken = " like ";
                var likeIndex = cond.IndexOf(likeToken, StringComparison.OrdinalIgnoreCase);
                if (likeIndex > 0)
                {
                    var field = cond[..likeIndex].Trim().ToLowerInvariant();
                    var value = cond[(likeIndex + likeToken.Length)..].Trim().Trim('\"', '\'');

                    filtered = filtered.Where(x => MatchField(x, field, value, exactMatch: false));
                }
            }
            else
            {
                filtered = filtered.Where(x =>
                    !string.IsNullOrEmpty(x.Message) &&
                    x.Message.Contains(cond));
            }
        }

        return filtered;
    }

    private bool MatchField(LogEntry log, string field, string value, bool exactMatch)
    {
        var fieldValue = GetFieldValue(log, field);

        if (string.IsNullOrEmpty(fieldValue))
        {
            return false;
        }

        return exactMatch
            ? fieldValue.Equals(value, StringComparison.OrdinalIgnoreCase)
            : fieldValue.Contains(value, StringComparison.OrdinalIgnoreCase);
    }

    private static string? GetFieldValue(LogEntry log, string field)
    {
        return field switch
        {
            "message" => log.Message,
            "source" => log.Source,
            "requestid" => log.RequestId,
            "requestpath" => log.RequestPath,
            "requestmethod" => log.RequestMethod,
            "level" => log.Level.ToString(),
            "exception" => log.Exception,
            "stacktrace" => log.StackTrace,
            "application" => log.Application,
            "logger" => log.Logger,
            "actionid" => log.ActionId,
            "actionname" => log.ActionName,
            "environment" => log.Environment,
            "machinename" => log.MachineName,
            _ => log.Properties.TryGetValue(field, out var val) ? val?.ToString() : null
        };
    }

    private void TrimExcessLogs()
    {
        while (_logs.Count > _maxLogCount)
        {
            var removed = _logs.Dequeue();

            if (!string.IsNullOrEmpty(removed.Id)
                && _logsById.TryGetValue(removed.Id, out var indexed)
                && ReferenceEquals(indexed, removed))
            {
                _logsById.Remove(removed.Id);
            }

            if (!string.IsNullOrEmpty(removed.RequestId)
                && _logsByRequestId.TryGetValue(removed.RequestId, out var requestLogs))
            {
                requestLogs.Remove(removed);
                if (requestLogs.Count == 0)
                {
                    _logsByRequestId.Remove(removed.RequestId);
                    _traceIndex.Remove(removed.RequestId);
                }
            }
        }
    }

    /// <summary>
    /// Trace 汇总信息的增量索引
    /// </summary>
    private class TraceInfo
    {
        public string RequestId { get; set; } = string.Empty;
        public int LogCount { get; set; }
        public DateTime FirstTimestamp { get; set; } = DateTime.MaxValue;
        public DateTime LastTimestamp { get; set; } = DateTime.MinValue;
        public string? RequestPath { get; set; }
        public string? RequestMethod { get; set; }
        public int? ResponseStatusCode { get; set; }
        public bool HasError { get; set; }
    }
}

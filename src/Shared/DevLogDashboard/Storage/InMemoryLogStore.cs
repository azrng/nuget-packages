using Azrng.DevLogDashboard.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Azrng.DevLogDashboard.Storage;

/// <summary>
/// 内存日志存储实现
/// </summary>
public class InMemoryLogStore : ILogStore
{
    private readonly ConcurrentBag<LogEntry> _logs = new();
    private readonly int _maxLogCount;
    private readonly object _lockObj = new();

    public InMemoryLogStore(int maxLogCount = 10000)
    {
        _maxLogCount = maxLogCount;
    }

    public int Count => _logs.Count;

    public void Add(LogEntry? entry)
    {
        if (entry == null) return;

        _logs.Add(entry);

        // 超出最大数量时，清理旧日志
        if (_logs.Count > _maxLogCount)
        {
            Cleanup();
        }
    }

    public ValueTask AddAsync(LogEntry entry, CancellationToken cancellationToken = default)
    {
        Add(entry);
        return ValueTask.CompletedTask;
    }

    public Task<LogEntry?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var log = _logs.FirstOrDefault(x => x.Id == id);
        return Task.FromResult(log);
    }

    public Task<List<LogEntry>> GetByRequestIdAsync(string requestId, CancellationToken cancellationToken = default)
    {
        var logs = _logs
            .Where(x => x.RequestId == requestId)
            .OrderBy(x => x.Timestamp)
            .ToList();
        return Task.FromResult(logs);
    }

    public async Task<PageResult<LogEntry>> QueryAsync(LogQuery query, CancellationToken cancellationToken = default)
    {
        var filtered = ApplyFilters(query);

        // 排序
        filtered = query.OrderByTimeAscending
            ? filtered.OrderBy(x => x.Timestamp).ToList()
            : filtered.OrderByDescending(x => x.Timestamp).ToList();

        var total = filtered.Count;

        // 分页
        var items = filtered
            .Skip(query.Skip)
            .Take(query.PageSize)
            .ToList();

        return PageResult<LogEntry>.Create(items, total, query.PageIndex, query.PageSize);
    }

    public async Task<List<TraceLogSummary>> GetTraceSummariesAsync(DateTime? startTime, DateTime? endTime, CancellationToken cancellationToken = default)
    {
        var queryable = _logs.AsQueryable();

        if (startTime.HasValue)
        {
            queryable = queryable.Where(x => x.Timestamp >= startTime.Value);
        }

        if (endTime.HasValue)
        {
            queryable = queryable.Where(x => x.Timestamp <= endTime.Value);
        }

        var summaries = queryable
            .Where(x => !string.IsNullOrEmpty(x.RequestId))
            .GroupBy(x => x.RequestId!)
            .Select(g => new TraceLogSummary
            {
                RequestId = g.Key!,
                LogCount = g.Count(),
                FirstTimestamp = g.Min(x => x.Timestamp),
                LastTimestamp = g.Max(x => x.Timestamp),
                RequestPath = g.Select(x => x.RequestPath).FirstOrDefault(x => !string.IsNullOrEmpty(x)),
                RequestMethod = g.Select(x => x.RequestMethod).FirstOrDefault(x => !string.IsNullOrEmpty(x)),
                ResponseStatusCode = g.Select(x => x.ResponseStatusCode).FirstOrDefault(x => x.HasValue),
                HasError = g.Any(x => x.Level >= LogLevel.Error)
            })
            .OrderByDescending(x => x.LastTimestamp)
            .Take(1000)
            .ToList();

        return await Task.FromResult(summaries);
    }

    public void Clear()
    {
        lock (_lockObj)
        {
            var temp = _logs.ToArray();
            _logs.Clear();
        }
    }

    public int GetCountByTimeRange(DateTime? startTime, DateTime? endTime)
    {
        var queryable = _logs.AsQueryable();

        if (startTime.HasValue)
        {
            queryable = queryable.Where(x => x.Timestamp >= startTime.Value);
        }

        if (endTime.HasValue)
        {
            queryable = queryable.Where(x => x.Timestamp <= endTime.Value);
        }

        return queryable.Count();
    }

    public Dictionary<LogLevel, int> GetLogLevelStatistics(DateTime? startTime, DateTime? endTime)
    {
        var queryable = _logs.AsQueryable();

        if (startTime.HasValue)
        {
            queryable = queryable.Where(x => x.Timestamp >= startTime.Value);
        }

        if (endTime.HasValue)
        {
            queryable = queryable.Where(x => x.Timestamp <= endTime.Value);
        }

        return queryable
            .GroupBy(x => x.Level)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    public List<LogEntry> GetRecentErrors(int count, DateTime? startTime, DateTime? endTime)
    {
        var queryable = _logs.AsQueryable();

        if (startTime.HasValue)
        {
            queryable = queryable.Where(x => x.Timestamp >= startTime.Value);
        }

        if (endTime.HasValue)
        {
            queryable = queryable.Where(x => x.Timestamp <= endTime.Value);
        }

        return queryable
            .Where(x => x.Level >= LogLevel.Error)
            .OrderByDescending(x => x.Timestamp)
            .Take(count)
            .ToList();
    }

    private List<LogEntry> ApplyFilters(LogQuery query)
    {
        var queryable = _logs.AsQueryable();

        // 时间范围过滤
        if (query.StartTime.HasValue)
        {
            queryable = queryable.Where(x => x.Timestamp >= query.StartTime.Value);
        }

        if (query.EndTime.HasValue)
        {
            queryable = queryable.Where(x => x.Timestamp <= query.EndTime.Value);
        }

        // 日志级别过滤（使用 MinLevel，过滤出该级别及更高级别的日志）
        if (query.MinLevel.HasValue)
        {
            queryable = queryable.Where(x => x.Level >= query.MinLevel.Value);
        }

        // RequestId 过滤
        if (!string.IsNullOrEmpty(query.RequestId))
        {
            queryable = queryable.Where(x => x.RequestId == query.RequestId);
        }

        // 来源过滤
        if (!string.IsNullOrEmpty(query.Source))
        {
            queryable = queryable.Where(x => x.Source == query.Source);
        }

        // 应用名称过滤
        if (!string.IsNullOrEmpty(query.Application))
        {
            queryable = queryable.Where(x => x.Application == query.Application);
        }

        // 关键词搜索（支持表达式语法）
        if (!string.IsNullOrEmpty(query.Keyword))
        {
            queryable = ApplyKeywordFilter(queryable, query.Keyword);
        }

        return queryable.ToList();
    }

    private IQueryable<LogEntry> ApplyKeywordFilter(IQueryable<LogEntry> queryable, string keyword)
    {
        // 支持表达式语法：message="xxx" and level="ERROR"
        // 简单实现：支持 and/or 组合

        keyword = keyword.Trim();

        // 检查是否包含 and/or
        var andParts = keyword.Split(new[] { " and ", " AND " }, StringSplitOptions.None);

        var conditions = new List<string>();
        foreach (var part in andParts)
        {
            var orParts = part.Split(new[] { " or ", " OR " }, StringSplitOptions.None);
            // 简单处理：只支持 and 连接多个条件，or 暂不支持复杂组合
            conditions.AddRange(orParts);
        }

        foreach (var condition in conditions)
        {
            var cond = condition.Trim();
            if (string.IsNullOrEmpty(cond)) continue;

            // 精准匹配：field="value"
            if (cond.Contains("=") && !cond.Contains(" like "))
            {
                var parts = cond.Split('=', 2);
                if (parts.Length == 2)
                {
                    var field = parts[0].Trim().ToLower();
                    var value = parts[1].Trim().Trim('"', '\'');

                    queryable = queryable.Where(x => MatchField(x, field, value, true));
                }
            }
            // 模糊匹配：field like "value"
            else if (cond.Contains(" like "))
            {
                var parts = cond.Split(new[] { " like " }, StringSplitOptions.None);
                if (parts.Length == 2)
                {
                    var field = parts[0].Trim().ToLower();
                    var value = parts[1].Trim().Trim('"', '\'');

                    queryable = queryable.Where(x => MatchField(x, field, value, false));
                }
            }
            // 简单关键词搜索（默认搜索 message）
            else
            {
                queryable = queryable.Where(x => x.Message != null && x.Message.Contains(cond));
            }
        }

        return queryable;
    }

    private bool MatchField(LogEntry log, string field, string value, bool exactMatch)
    {
        var fieldValue = GetFieldValue(log, field);

        if (string.IsNullOrEmpty(fieldValue))
        {
            return false;
        }

        if (exactMatch)
        {
            return fieldValue.Equals(value, StringComparison.OrdinalIgnoreCase);
        }
        else
        {
            return fieldValue.Contains(value, StringComparison.OrdinalIgnoreCase);
        }
    }

    private string? GetFieldValue(LogEntry log, string field)
    {
        return field.ToLower() switch
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

    private void Cleanup()
    {
        // 简单的清理策略：删除最旧的 10% 日志
        var toRemove = _maxLogCount / 10;
        if (toRemove == 0) toRemove = 1;

        lock (_lockObj)
        {
            var oldestLogs = _logs
                .OrderBy(x => x.Timestamp)
                .Take(toRemove)
                .ToList();

            foreach (var log in oldestLogs)
            {
                _logs.TryTake(out _);
            }
        }
    }
}

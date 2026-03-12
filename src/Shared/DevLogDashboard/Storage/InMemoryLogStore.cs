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
    private readonly ReaderWriterLockSlim _lock = new(LockRecursionPolicy.NoRecursion);
    private readonly int _maxLogCount;

    public InMemoryLogStore(int maxLogCount = 10000)
    {
        _maxLogCount = maxLogCount > 0 ? maxLogCount : 10000;
    }

    public int Count
    {
        get
        {
            _lock.EnterReadLock();
            try
            {
                return _logs.Count;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
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

            if (!string.IsNullOrEmpty(entry.RequestId))
            {
                if (!_logsByRequestId.TryGetValue(entry.RequestId, out var requestLogs))
                {
                    requestLogs = new List<LogEntry>();
                    _logsByRequestId[entry.RequestId] = requestLogs;
                }

                requestLogs.Add(entry);
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

            var ordered = logs
                .OrderBy(x => x.Timestamp)
                .ToList();

            return Task.FromResult(ordered);
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
        var pageSize = query.PageSize <= 0 ? 50 : query.PageSize;
        var skip = (pageIndex - 1) * pageSize;

        var filtered = ApplyFilters(Snapshot(), query).ToList();

        filtered.Sort((a, b) => query.OrderByTimeAscending
            ? a.Timestamp.CompareTo(b.Timestamp)
            : b.Timestamp.CompareTo(a.Timestamp));

        var total = filtered.Count;
        var items = filtered
            .Skip(skip)
            .Take(pageSize)
            .ToList();

        return Task.FromResult(PageResult<LogEntry>.Create(items, total, pageIndex, pageSize));
    }

    public Task<List<TraceLogSummary>> GetTraceSummariesAsync(DateTime? startTime, DateTime? endTime,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled<List<TraceLogSummary>>(cancellationToken);
        }

        var logs = Snapshot();

        var query = logs.Where(x => !string.IsNullOrEmpty(x.RequestId));

        if (startTime.HasValue)
        {
            query = query.Where(x => x.Timestamp >= startTime.Value);
        }

        if (endTime.HasValue)
        {
            query = query.Where(x => x.Timestamp <= endTime.Value);
        }

        var summaries = query
            .GroupBy(x => x.RequestId!)
            .Select(g => new TraceLogSummary
            {
                RequestId = g.Key,
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

        return Task.FromResult(summaries);
    }

    public void Clear()
    {
        _lock.EnterWriteLock();
        try
        {
            _logs.Clear();
            _logsById.Clear();
            _logsByRequestId.Clear();
        }
        finally
        {
            _lock.ExitWriteLock();
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
                    var value = parts[1].Trim().Trim('"', '\'');

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
                    var value = cond[(likeIndex + likeToken.Length)..].Trim().Trim('"', '\'');

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
                }
            }
        }
    }

    private LogEntry[] Snapshot()
    {
        _lock.EnterReadLock();
        try
        {
            return _logs.ToArray();
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }
}



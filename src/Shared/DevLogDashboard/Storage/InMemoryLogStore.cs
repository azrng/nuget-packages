using Azrng.DevLogDashboard.Models;
using Microsoft.Extensions.Logging;

namespace Azrng.DevLogDashboard.Storage;

/// <summary>
/// In-memory log storage with bounded queue and lightweight indexes.
/// </summary>
public class InMemoryLogStore : LogStoreBase
{
    private const string OrOperator = "or";
    private const string AndOperator = "and";

    private readonly Queue<LogEntry> _logs = new();
    private readonly Dictionary<string, LogEntry> _logsById = new(StringComparer.Ordinal);
    private readonly Dictionary<string, List<LogEntry>> _logsByRequestId = new(StringComparer.Ordinal);
    private readonly Dictionary<string, TraceInfo> _traceIndex = new(StringComparer.Ordinal);
    private readonly ReaderWriterLockSlim _lock = new(LockRecursionPolicy.NoRecursion);
    private readonly int _maxLogCount;

    private const int MaxPageSize = 500;

    public InMemoryLogStore(int maxLogCount = 10000)
    {
        _maxLogCount = maxLogCount > 0 ? maxLogCount : 10000;
    }

    public override Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public override ValueTask AddBatchAsync(IEnumerable<LogEntry> entries, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return ValueTask.FromCanceled(cancellationToken);
        }

        if (entries == null)
        {
            return ValueTask.CompletedTask;
        }

        _lock.EnterWriteLock();
        try
        {
            foreach (var entry in entries)
            {
                if (entry == null)
                {
                    continue;
                }

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
        }
        finally
        {
            _lock.ExitWriteLock();
        }

        return ValueTask.CompletedTask;
    }

    public override Task<List<LogEntry>> GetByRequestIdAsync(string requestId, CancellationToken cancellationToken = default)
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

            return Task.FromResult(logs.ToList());
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public override Task<PageResult<LogEntry>> QueryAsync(LogQuery query, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled<PageResult<LogEntry>>(cancellationToken);
        }

        query ??= new LogQuery();

        var pageIndex = query.PageIndex <= 0 ? 1 : query.PageIndex;
        var pageSize = Math.Min(query.PageSize <= 0 ? 50 : query.PageSize, MaxPageSize);
        var skip = (pageIndex - 1) * pageSize;

        _lock.EnterReadLock();
        try
        {
            var canFastPath = !query.OrderByTimeAscending &&
                              string.IsNullOrEmpty(query.Id) &&
                              string.IsNullOrEmpty(query.Keyword) &&
                              string.IsNullOrEmpty(query.RequestId) &&
                              string.IsNullOrEmpty(query.Source) &&
                              string.IsNullOrEmpty(query.Application) &&
                              !query.MinLevel.HasValue &&
                              !query.StartTime.HasValue &&
                              !query.EndTime.HasValue;

            if (canFastPath)
            {
                var array = _logs.ToArray();
                var items = new List<LogEntry>(pageSize);
                var skipped = 0;

                for (var i = array.Length - 1; i >= 0 && items.Count < pageSize; i--)
                {
                    if (skipped < skip)
                    {
                        skipped++;
                        continue;
                    }

                    items.Add(array[i]);
                }

                return Task.FromResult(PageResult<LogEntry>.Create(items, array.Length, pageIndex, pageSize));
            }

            var filtered = ApplyFilters(query).ToList();

            filtered.Sort((a, b) => query.OrderByTimeAscending
                ? a.Timestamp.CompareTo(b.Timestamp)
                : b.Timestamp.CompareTo(a.Timestamp));

            var total = filtered.Count;
            var pagedItems = filtered
                .Skip(skip)
                .Take(pageSize)
                .ToList();

            return Task.FromResult(PageResult<LogEntry>.Create(pagedItems, total, pageIndex, pageSize));
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public override Task<List<TraceLogSummary>> GetTraceSummariesAsync(
        DateTime? startTime,
        DateTime? endTime,
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

    private IEnumerable<LogEntry> ApplyFilters(LogQuery query)
    {
        IEnumerable<LogEntry> filtered = _logs;

        if (!string.IsNullOrEmpty(query.Id))
        {
            if (_logsById.TryGetValue(query.Id, out var log))
            {
                filtered = new[] { log };
            }
            else
            {
                return Array.Empty<LogEntry>();
            }
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
            if (string.IsNullOrEmpty(query.Id))
            {
                if (_logsByRequestId.TryGetValue(query.RequestId, out var requestLogs))
                {
                    filtered = requestLogs;
                }
                else
                {
                    return Array.Empty<LogEntry>();
                }
            }
            else
            {
                filtered = filtered.Where(x => x.RequestId == query.RequestId);
            }
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
        keyword = keyword.Trim();
        if (string.IsNullOrEmpty(keyword))
        {
            return logs;
        }

        var groups = SplitByLogicalOperator(keyword, OrOperator)
            .Select(group => SplitByLogicalOperator(group, AndOperator)
                .Select(condition => condition.Trim())
                .Where(condition => !string.IsNullOrEmpty(condition))
                .ToArray())
            .Where(group => group.Length > 0)
            .ToArray();

        if (groups.Length == 0)
        {
            return logs;
        }

        return logs.Where(log => groups.Any(group => group.All(condition => MatchCondition(log, condition))));
    }

    private bool MatchCondition(LogEntry log, string condition)
    {
        if (condition.Contains('=') && !condition.Contains(" like ", StringComparison.OrdinalIgnoreCase))
        {
            var parts = condition.Split('=', 2);
            if (parts.Length == 2)
            {
                var field = parts[0].Trim().ToLowerInvariant();
                var value = parts[1].Trim().Trim('"', '\'');
                return MatchField(log, field, value, exactMatch: true);
            }

            return false;
        }

        if (condition.Contains(" like ", StringComparison.OrdinalIgnoreCase))
        {
            const string likeToken = " like ";
            var likeIndex = condition.IndexOf(likeToken, StringComparison.OrdinalIgnoreCase);
            if (likeIndex > 0)
            {
                var field = condition[..likeIndex].Trim().ToLowerInvariant();
                var value = condition[(likeIndex + likeToken.Length)..].Trim().Trim('"', '\'');
                return MatchField(log, field, value, exactMatch: false);
            }

            return false;
        }

        return !string.IsNullOrEmpty(log.Message) &&
               log.Message.Contains(condition, StringComparison.OrdinalIgnoreCase);
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

    private static IEnumerable<string> SplitByLogicalOperator(string input, string operatorToken)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            yield break;
        }

        var segmentStart = 0;
        var inQuotes = false;
        char quoteChar = default;

        for (var i = 0; i < input.Length; i++)
        {
            var current = input[i];

            if (inQuotes)
            {
                if (current == quoteChar && !IsEscaped(input, i))
                {
                    inQuotes = false;
                }

                continue;
            }

            if (current is '"' or '\'')
            {
                inQuotes = true;
                quoteChar = current;
                continue;
            }

            if (!IsLogicalOperatorAt(input, i, operatorToken))
            {
                continue;
            }

            var segment = input[segmentStart..i].Trim();
            if (!string.IsNullOrEmpty(segment))
            {
                yield return segment;
            }

            i += operatorToken.Length - 1;
            segmentStart = i + 1;
        }

        var trailingSegment = input[segmentStart..].Trim();
        if (!string.IsNullOrEmpty(trailingSegment))
        {
            yield return trailingSegment;
        }
    }

    private static bool IsLogicalOperatorAt(string input, int index, string operatorToken)
    {
        if (index < 0 || index + operatorToken.Length > input.Length)
        {
            return false;
        }

        if (!input.AsSpan(index, operatorToken.Length).Equals(operatorToken, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var beforeIndex = index - 1;
        var afterIndex = index + operatorToken.Length;
        var hasBoundaryBefore = beforeIndex >= 0 && char.IsWhiteSpace(input[beforeIndex]);
        var hasBoundaryAfter = afterIndex < input.Length && char.IsWhiteSpace(input[afterIndex]);

        return hasBoundaryBefore && hasBoundaryAfter;
    }

    private static bool IsEscaped(string input, int index)
    {
        var slashCount = 0;
        for (var i = index - 1; i >= 0 && input[i] == '\\'; i--)
        {
            slashCount++;
        }

        return slashCount % 2 == 1;
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
                else if (_traceIndex.TryGetValue(removed.RequestId, out var traceInfo))
                {
                    RecomputeTraceInfo(traceInfo, requestLogs);
                }
            }
        }
    }

    private static void RecomputeTraceInfo(TraceInfo traceInfo, List<LogEntry> requestLogs)
    {
        traceInfo.LogCount = requestLogs.Count;
        traceInfo.FirstTimestamp = DateTime.MaxValue;
        traceInfo.LastTimestamp = DateTime.MinValue;
        traceInfo.RequestPath = null;
        traceInfo.RequestMethod = null;
        traceInfo.ResponseStatusCode = null;
        traceInfo.HasError = false;

        foreach (var log in requestLogs)
        {
            if (log == null)
            {
                continue;
            }

            if (log.Timestamp < traceInfo.FirstTimestamp)
            {
                traceInfo.FirstTimestamp = log.Timestamp;
            }

            if (log.Timestamp > traceInfo.LastTimestamp)
            {
                traceInfo.LastTimestamp = log.Timestamp;
                if (log.ResponseStatusCode.HasValue)
                {
                    traceInfo.ResponseStatusCode = log.ResponseStatusCode.Value;
                }
            }

            if (string.IsNullOrEmpty(traceInfo.RequestPath) && !string.IsNullOrEmpty(log.RequestPath))
            {
                traceInfo.RequestPath = log.RequestPath;
            }

            if (string.IsNullOrEmpty(traceInfo.RequestMethod) && !string.IsNullOrEmpty(log.RequestMethod))
            {
                traceInfo.RequestMethod = log.RequestMethod;
            }

            if (log.Level >= LogLevel.Error)
            {
                traceInfo.HasError = true;
            }
        }
    }

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

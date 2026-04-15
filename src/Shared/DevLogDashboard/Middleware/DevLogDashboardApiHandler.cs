using Azrng.DevLogDashboard.Background;
using Azrng.DevLogDashboard.Models;
using Azrng.DevLogDashboard.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text.Json;

namespace Azrng.DevLogDashboard.Middleware;

/// <summary>
/// DevLogDashboard API 处理器
/// </summary>
internal class DevLogDashboardApiHandler
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    private readonly ILogStore _logStore;
    private readonly IBackgroundLogQueue _logQueue;

    public DevLogDashboardApiHandler(ILogStore logStore, IBackgroundLogQueue logQueue)
    {
        _logStore = logStore;
        _logQueue = logQueue;
    }

    public async Task HandleApiRequestAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;
        var method = context.Request.Method.ToUpperInvariant();

        if (path.Equals("/api/logs", StringComparison.OrdinalIgnoreCase) && method == "GET")
        {
            await HandleLogsQueryAsync(context);
            return;
        }

        if (method == "GET" && path.StartsWith("/api/logs/", StringComparison.OrdinalIgnoreCase))
        {
            await HandleLogDetailAsync(context);
            return;
        }

        if (path.Equals("/api/traces", StringComparison.OrdinalIgnoreCase) && method == "GET")
        {
            await HandleTracesAsync(context);
            return;
        }

        if (path.Equals("/api/serverTime", StringComparison.OrdinalIgnoreCase) && method == "GET")
        {
            await HandleServerTimeAsync(context);
            return;
        }

        if (method == "GET" && path.StartsWith("/api/traces/", StringComparison.OrdinalIgnoreCase))
        {
            await HandleTraceDetailAsync(context);
            return;
        }

        context.Response.StatusCode = 404;
        await context.Response.WriteAsJsonAsync(new { error = "未找到 API 端点" }, JsonOptions);
    }

    private async Task HandleLogsQueryAsync(HttpContext context)
    {
        const int maxPageSize = 500;
        const int maxKeywordLength = 500;

        var pageSize = ParseInt(context.Request.Query["pageSize"], 50);
        if (pageSize > maxPageSize)
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new { error = $"每页大小不能超过 {maxPageSize}" }, JsonOptions);
            return;
        }

        var keyword = context.Request.Query["keyword"].ToString();
        if (!string.IsNullOrEmpty(keyword) && keyword.Length > maxKeywordLength)
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new { error = $"关键字长度不能超过 {maxKeywordLength} 个字符" }, JsonOptions);
            return;
        }

        var query = new LogQuery
        {
            Keyword = keyword,
            MinLevel = ParseLogLevel(context.Request.Query["level"]),
            RequestId = context.Request.Query["requestId"],
            Source = context.Request.Query["source"],
            Application = context.Request.Query["application"],
            StartTime = ParseDateTime(context.Request.Query["startTime"]),
            EndTime = ParseDateTime(context.Request.Query["endTime"]),
            OrderByTimeAscending = ParseBool(context.Request.Query["orderByTimeAscending"], false),
            PageIndex = ParseInt(context.Request.Query["pageIndex"], 1),
            PageSize = pageSize
        };

        var result = await _logStore.QueryAsync(query, context.RequestAborted);
        await context.Response.WriteAsJsonAsync(result, JsonOptions);
    }

    private async Task HandleLogDetailAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;
        var idStart = path.LastIndexOf("/api/logs/", StringComparison.OrdinalIgnoreCase);
        var id = idStart >= 0 ? path[(idStart + 10)..] : string.Empty;

        if (string.IsNullOrEmpty(id))
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new { error = "日志 ID 是必需的" }, JsonOptions);
            return;
        }

        var query = new LogQuery
        {
            Id = id,
            PageIndex = 1,
            PageSize = 1
        };

        var result = await _logStore.QueryAsync(query, context.RequestAborted);
        var log = result.Items.FirstOrDefault();

        if (log == null)
        {
            context.Response.StatusCode = 404;
            await context.Response.WriteAsJsonAsync(new { error = "未找到日志" }, JsonOptions);
            return;
        }

        await context.Response.WriteAsJsonAsync(log, JsonOptions);
    }

    private async Task HandleTracesAsync(HttpContext context)
    {
        var startTime = ParseDateTime(context.Request.Query["startTime"]);
        var endTime = ParseDateTime(context.Request.Query["endTime"]);

        var traces = await _logStore.GetTraceSummariesAsync(startTime, endTime, context.RequestAborted);
        await context.Response.WriteAsJsonAsync(traces, JsonOptions);
    }

    private async Task HandleTraceDetailAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;
        var idStart = path.LastIndexOf("/api/traces/", StringComparison.OrdinalIgnoreCase);
        var requestId = idStart >= 0 ? path[(idStart + 12)..] : string.Empty;

        if (string.IsNullOrEmpty(requestId))
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new { error = "请求 ID 是必需的" }, JsonOptions);
            return;
        }

        var logs = await _logStore.GetByRequestIdAsync(requestId, context.RequestAborted);
        await context.Response.WriteAsJsonAsync(logs, JsonOptions);
    }

    private async Task HandleServerTimeAsync(HttpContext context)
    {
        var serverTime = DateTime.Now;
        await context.Response.WriteAsJsonAsync(new
        {
            serverTime = serverTime.ToString("o"),
            queuedCount = _logQueue.GetQueuedCount(),
            droppedCount = _logQueue.GetDroppedCount()
        }, JsonOptions);
    }

    private static LogLevel? ParseLogLevel(string? level)
    {
        if (string.IsNullOrEmpty(level))
        {
            return null;
        }

        return level.ToUpper() switch
        {
            "TRACE" => LogLevel.Trace,
            "DEBUG" => LogLevel.Debug,
            "INFO" or "INFORMATION" => LogLevel.Information,
            "WARN" or "WARNING" => LogLevel.Warning,
            "ERROR" => LogLevel.Error,
            "FATAL" or "CRITICAL" => LogLevel.Critical,
            _ => null
        };
    }

    private static DateTime? ParseDateTime(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }

        if (DateTimeOffset.TryParse(
                value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeUniversal,
                out var offsetResult))
        {
            return offsetResult.LocalDateTime;
        }

        if (DateTime.TryParse(
                value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeLocal,
                out var result))
        {
            return result;
        }

        return null;
    }

    private static bool ParseBool(string? value, bool defaultValue)
    {
        if (string.IsNullOrEmpty(value))
        {
            return defaultValue;
        }

        return bool.TryParse(value, out var result) ? result : defaultValue;
    }

    private static int ParseInt(string? value, int defaultValue)
    {
        if (string.IsNullOrEmpty(value))
        {
            return defaultValue;
        }

        return int.TryParse(value, out var result) ? result : defaultValue;
    }
}

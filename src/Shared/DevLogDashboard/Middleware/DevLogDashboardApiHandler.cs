using Azrng.DevLogDashboard.Models;
using Azrng.DevLogDashboard.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
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

    public DevLogDashboardApiHandler(ILogStore logStore)
    {
        _logStore = logStore;
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

        if (path.Equals("/api/clear", StringComparison.OrdinalIgnoreCase) && method == "POST")
        {
            await HandleClearLogsAsync(context);
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
        await context.Response.WriteAsJsonAsync(new { error = "API endpoint not found" }, JsonOptions);
    }

    private async Task HandleLogsQueryAsync(HttpContext context)
    {
        var query = new LogQuery
        {
            Keyword = context.Request.Query["keyword"],
            MinLevel = ParseLogLevel(context.Request.Query["level"]),
            RequestId = context.Request.Query["requestId"],
            Source = context.Request.Query["source"],
            Application = context.Request.Query["application"],
            StartTime = ParseDateTime(context.Request.Query["startTime"]),
            EndTime = ParseDateTime(context.Request.Query["endTime"]),
            OrderByTimeAscending = ParseBool(context.Request.Query["orderByTimeAscending"], false),
            PageIndex = ParseInt(context.Request.Query["pageIndex"], 1),
            PageSize = ParseInt(context.Request.Query["pageSize"], 50)
        };

        var result = await _logStore.QueryAsync(query);
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
            await context.Response.WriteAsJsonAsync(new { error = "Log ID is required" }, JsonOptions);
            return;
        }

        // 使用 QueryAsync 查询单条日志
        var query = new LogQuery
        {
            Id = id,
            PageIndex = 1,
            PageSize = 1
        };

        var result = await _logStore.QueryAsync(query);
        var log = result.Items.FirstOrDefault();

        if (log == null)
        {
            context.Response.StatusCode = 404;
            await context.Response.WriteAsJsonAsync(new { error = "Log not found" }, JsonOptions);
            return;
        }

        await context.Response.WriteAsJsonAsync(log, JsonOptions);
    }

    private async Task HandleTracesAsync(HttpContext context)
    {
        var startTime = ParseDateTime(context.Request.Query["startTime"]);
        var endTime = ParseDateTime(context.Request.Query["endTime"]);

        var traces = await _logStore.GetTraceSummariesAsync(startTime, endTime);
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
            await context.Response.WriteAsJsonAsync(new { error = "Request ID is required" }, JsonOptions);
            return;
        }

        var logs = await _logStore.GetByRequestIdAsync(requestId);
        await context.Response.WriteAsJsonAsync(logs, JsonOptions);
    }

    private async Task HandleServerTimeAsync(HttpContext context)
    {
        var serverTime = DateTime.Now;
        await context.Response.WriteAsJsonAsync(new { serverTime = serverTime.ToString("o") }, JsonOptions);
    }

    private async Task HandleClearLogsAsync(HttpContext context)
    {
        _logStore.Clear();
        await context.Response.WriteAsJsonAsync(new { success = true }, JsonOptions);
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

        return DateTime.TryParse(value, out var result) ? result : null;
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

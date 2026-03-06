using Azrng.DevLogDashboard.Models;
using Azrng.DevLogDashboard.Storage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Azrng.DevLogDashboard.Endpoints;

/// <summary>
/// 仪表板 API 端点
/// </summary>
public static class DashboardEndpoints
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public static void MapDashboardEndpoints(this IEndpointRouteBuilder app)
    {
        var prefix = "/dev-logs-api";

        // 获取仪表板数据
        app.MapGet($"{prefix}/dashboard", async (HttpContext httpContext, ILogStore logStore) =>
        {
            var now = DateTime.Now;
            var startOfDay = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);

            var dashboard = new DashboardModel
            {
                TotalLogs = logStore.Count,
                LevelStatistics = logStore.GetLogLevelStatistics(null, null).ToDictionary(
                    k => k.Key.ToString(), v => v.Value),
                ErrorLogs = logStore.GetLogLevelStatistics(null, null).GetValueOrDefault(LogLevel.Error, 0) +
                           logStore.GetLogLevelStatistics(null, null).GetValueOrDefault(LogLevel.Critical, 0),
                WarningLogs = logStore.GetLogLevelStatistics(null, null).GetValueOrDefault(LogLevel.Warning, 0),
                InformationLogs = logStore.GetLogLevelStatistics(null, null).GetValueOrDefault(LogLevel.Information, 0),
                RecentErrors = logStore.GetRecentErrors(10, null, null),
                HourlyCounts = GetHourlyCounts(logStore, startOfDay)
            };

            await httpContext.Response.WriteAsJsonAsync(dashboard, JsonOptions);
        });

        // 查询日志列表
        app.MapGet($"{prefix}/logs", async (
            HttpContext httpContext,
            ILogStore logStore,
            [FromQuery] string? keyword,
            [FromQuery] string? level,
            [FromQuery] string? requestId,
            [FromQuery] string? source,
            [FromQuery] string? application,
            [FromQuery] DateTime? startTime,
            [FromQuery] DateTime? endTime,
            [FromQuery] bool orderByTimeAscending = false,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 50) =>
        {
            var query = new LogQuery
            {
                Keyword = keyword,
                MinLevel = ParseLogLevel(level),
                RequestId = requestId,
                Source = source,
                Application = application,
                StartTime = startTime,
                EndTime = endTime,
                OrderByTimeAscending = orderByTimeAscending,
                PageIndex = pageIndex,
                PageSize = pageSize
            };

            var result = await logStore.QueryAsync(query);
            await httpContext.Response.WriteAsJsonAsync(result, JsonOptions);
        });

        // 获取日志详情
        app.MapGet($"{prefix}/logs/{{id}}", async (
            HttpContext httpContext,
            ILogStore logStore,
            string id) =>
        {
            var log = await logStore.GetByIdAsync(id);
            if (log == null)
            {
                httpContext.Response.StatusCode = 404;
                await httpContext.Response.WriteAsJsonAsync(new { error = "Log not found" });
                return;
            }

            await httpContext.Response.WriteAsJsonAsync(log, JsonOptions);
        });

        // 获取追踪列表
        app.MapGet($"{prefix}/traces", async (
            HttpContext httpContext,
            ILogStore logStore,
            [FromQuery] DateTime? startTime,
            [FromQuery] DateTime? endTime) =>
        {
            var traces = await logStore.GetTraceSummariesAsync(startTime, endTime);
            await httpContext.Response.WriteAsJsonAsync(traces, JsonOptions);
        });

        // 获取追踪详情
        app.MapGet($"{prefix}/traces/{{requestId}}", async (
            HttpContext httpContext,
            ILogStore logStore,
            string requestId) =>
        {
            var logs = await logStore.GetByRequestIdAsync(requestId);
            await httpContext.Response.WriteAsJsonAsync(logs, JsonOptions);
        });

        // 清空日志
        app.MapPost($"{prefix}/clear", async (HttpContext httpContext, ILogStore logStore) =>
        {
            logStore.Clear();
            await httpContext.Response.WriteAsJsonAsync(new { success = true });
        });
    }

    private static List<HourlyLogCount> GetHourlyCounts(ILogStore logStore, DateTime startTime)
    {
        var now = DateTime.Now;
        var hourlyCounts = new List<HourlyLogCount>();

        for (int hour = 0; hour < 24; hour++)
        {
            var hourStart = startTime.AddHours(hour);
            var hourEnd = hourStart.AddHours(1);

            var stats = logStore.GetLogLevelStatistics(hourStart, hourEnd);

            hourlyCounts.Add(new HourlyLogCount
            {
                Hour = hourStart.ToString("HH:00"),
                Count = stats.Values.Sum(),
                ErrorCount = stats.GetValueOrDefault(LogLevel.Error, 0) + stats.GetValueOrDefault(LogLevel.Critical, 0),
                WarningCount = stats.GetValueOrDefault(LogLevel.Warning, 0)
            });
        }

        return hourlyCounts;
    }

    private static LogLevel? ParseLogLevel(string? level)
    {
        if (string.IsNullOrEmpty(level)) return null;

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
}

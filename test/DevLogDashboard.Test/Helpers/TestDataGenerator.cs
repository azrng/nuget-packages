using Azrng.DevLogDashboard.Models;
using Microsoft.Extensions.Logging;

namespace DevLogDashboard.Test.Helpers;

/// <summary>
/// 测试数据生成器
/// </summary>
public class TestDataGenerator
{
    private readonly Random _random = new();
    private static readonly string[][] RequestPaths = new[]
    {
        new[] { "/api/users", "GET" },
        new[] { "/api/users", "POST" },
        new[] { "/api/products", "GET" },
        new[] { "/api/products/123", "GET" },
        new[] { "/api/orders", "POST" },
        new[] { "/api/auth/login", "POST" },
        new[] { "/api/dashboard", "GET" },
        new[] { "/health", "GET" }
    };

    private static readonly string[] Sources = new[]
    {
        "DevLogDashboard.Test",
        "API.Controllers.UsersController",
        "API.Services.ProductService",
        "API.Services.AuthService",
        "Microsoft.AspNetCore.Mvc.Infrastructure"
    };

    private static readonly string[] Messages = new[]
    {
        "User logged in successfully",
        "Failed to connect to database",
        "Request processed successfully",
        "Timeout occurred while calling external API",
        "Validation failed for input data",
        "Cache miss for key",
        "Background job started",
        "Payment processed",
        "Authentication token expired",
        "Record not found in database"
    };

    /// <summary>
    /// 创建单个日志条目
    /// </summary>
    public LogEntry CreateLogEntry(
        LogLevel? level = null,
        string? message = null,
        string? requestId = null,
        DateTime? timestamp = null)
    {
        var pathInfo = RequestPaths[_random.Next(RequestPaths.Length)];
        var now = DateTime.Now;

        return new LogEntry
        {
            Id = Guid.NewGuid().ToString("N"),
            RequestId = requestId ?? Guid.NewGuid().ToString("N"),
            ConnectionId = Guid.NewGuid().ToString("N"),
            Timestamp = timestamp ?? now.AddSeconds(_random.Next(-1000, 0)),
            Level = level ?? GetRandomLogLevel(),
            Message = message ?? Messages[_random.Next(Messages.Length)],
            Exception = ShouldAddException() ? "System.Exception: Something went wrong" : null,
            StackTrace = ShouldAddException() ? "   at API.Controllers.UsersController.Get() in /src/Controllers/UsersController.cs:42" : null,
            Source = Sources[_random.Next(Sources.Length)],
            EventId = _random.Next(1000),
            RequestPath = pathInfo[0],
            RequestMethod = pathInfo[1],
            ResponseStatusCode = _random.Next(1, 10) == 1 ? null : (_random.Next(1, 5) == 1 ? 500 : 200),
            ElapsedMilliseconds = _random.Next(1, 500),
            ThreadId = _random.Next(1, 100),
            ProcessId = 12345,
            MachineName = "TestMachine",
            Application = "TestApp",
            AppVersion = "1.0.0",
            Environment = "Development",
            Logger = Sources[_random.Next(Sources.Length)],
            ActionId = _random.Next(1, 5) == 1 ? Guid.NewGuid().ToString("N") : null,
            ActionName = _random.Next(1, 3) == 1 ? "DevLogDashboard.Test.Controllers.Home.Index" : null,
            Properties = new Dictionary<string, object?>
            {
                { "CustomProperty1", $"Value{_random.Next(100)}" },
                { "CustomProperty2", _random.Next(1, 3) == 1 ? "SpecialValue" : null }
            }
        };
    }

    /// <summary>
    /// 创建多个日志条目
    /// </summary>
    public List<LogEntry> CreateLogEntries(int count, LogLevel? fixedLevel = null)
    {
        return Enumerable.Range(0, count)
            .Select(i => CreateLogEntry(level: fixedLevel))
            .ToList();
    }

    /// <summary>
    /// 创建指定请求 ID 的日志条目
    /// </summary>
    public LogEntry CreateLogEntryForRequest(string requestId, LogLevel? level = null)
    {
        return CreateLogEntry(level: level, requestId: requestId);
    }

    /// <summary>
    /// 创建查询对象
    /// </summary>
    public LogQuery CreateLogQuery(int pageIndex = 1, int pageSize = 50)
    {
        return new LogQuery
        {
            PageIndex = pageIndex,
            PageSize = pageSize,
            OrderByTimeAscending = false
        };
    }

    /// <summary>
    /// 创建错误日志
    /// </summary>
    public LogEntry CreateErrorLog(string? message = null)
    {
        return CreateLogEntry(
            level: LogLevel.Error,
            message: message ?? "An error occurred"
        );
    }

    /// <summary>
    /// 创建信息日志
    /// </summary>
    public LogEntry CreateInfoLog(string? message = null)
    {
        return CreateLogEntry(
            level: LogLevel.Information,
            message: message ?? "Information message"
        );
    }

    /// <summary>
    /// 创建带有特定消息的日志
    /// </summary>
    public LogEntry CreateLogWithMessage(string message)
    {
        return CreateLogEntry(message: message);
    }

    /// <summary>
    /// 创建时间范围内的日志
    /// </summary>
    public List<LogEntry> CreateLogEntriesInTimeRange(DateTime startTime, DateTime endTime, int count)
    {
        var span = endTime - startTime;
        return Enumerable.Range(0, count)
            .Select(i =>
            {
                var offset = span.TotalSeconds * i / count;
                return CreateLogEntry(timestamp: startTime.AddSeconds(offset));
            })
            .ToList();
    }

    private LogLevel GetRandomLogLevel()
    {
        var levels = new[]
        {
            LogLevel.Trace,
            LogLevel.Debug,
            LogLevel.Information,
            LogLevel.Warning,
            LogLevel.Error,
            LogLevel.Critical
        };

        // 加权随机，更可能生成 Info 和 Warning
        var weights = new[] { 5, 10, 40, 30, 10, 5 };
        var totalWeight = weights.Sum();
        var random = _random.Next(totalWeight);

        var cumulative = 0;
        for (int i = 0; i < weights.Length; i++)
        {
            cumulative += weights[i];
            if (random < cumulative)
            {
                return levels[i];
            }
        }

        return LogLevel.Information;
    }

    private bool ShouldAddException()
    {
        // 只有 Error 和 Critical 日志，或者 10% 的其他级别日志才有异常信息
        return _random.Next(1, 10) == 1;
    }
}

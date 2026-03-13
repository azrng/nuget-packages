using Azrng.DevLogDashboard.Models;
using Microsoft.Extensions.Logging;

namespace DevLogDashboard.PerformanceTest;

/// <summary>
/// 性能测试数据生成器
/// </summary>
public static class TestDataGenerator
{
    private static readonly Random Random = new();

    public static LogEntry CreateLogEntry(LogLevel? level = null, string? message = null, string? requestId = null)
    {
        return new LogEntry
        {
            Id = Guid.NewGuid().ToString("N"),
            RequestId = requestId ?? Guid.NewGuid().ToString("N"),
            Timestamp = DateTime.Now.AddSeconds(Random.Next(-1000, 0)),
            Level = level ?? GetRandomLogLevel(),
            Message = message ?? $"Log message {Random.Next()}",
            Source = "PerformanceTest.Source",
            RequestPath = "/api/test",
            RequestMethod = "GET",
            ResponseStatusCode = 200,
            ElapsedMilliseconds = Random.Next(10, 1000),
            MachineName = "TestMachine",
            Application = "TestApp",
            Environment = "Development"
        };
    }

    public static List<LogEntry> CreateLogEntries(int count)
    {
        return Enumerable.Range(0, count)
            .Select(_ => CreateLogEntry())
            .ToList();
    }

    private static LogLevel GetRandomLogLevel()
    {
        var levels = new[] { LogLevel.Trace, LogLevel.Debug, LogLevel.Information,
                           LogLevel.Warning, LogLevel.Error, LogLevel.Critical };
        return levels[Random.Next(levels.Length)];
    }
}

using Azrng.DevLogDashboard.Background;
using Azrng.DevLogDashboard.Models;
using Azrng.DevLogDashboard.Storage;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Logging;

namespace DevLogDashboard.PerformanceTest.Benchmarks;

/// <summary>
/// 日志写入吞吐量基准测试
/// 目标：≥ 1,000 条/秒
/// </summary>
[MemoryDiagnoser]
public class LogWriteThroughputBenchmark
{
    private InMemoryLogStore _store = null!;
    private BackgroundLogQueue _queue = null!;
    private List<LogEntry> _logs100 = null!;
    private List<LogEntry> _logs1000 = null!;

    [GlobalSetup]
    public void Setup()
    {
        _store = new InMemoryLogStore(maxLogCount: 10000);
        _queue = new BackgroundLogQueue(capacity: 10000);
        _logs100 = TestDataGenerator.CreateLogEntries(100);
        _logs1000 = TestDataGenerator.CreateLogEntries(1000);
    }

    [Benchmark]
    public async Task DirectWrite_Batch100()
    {
        var store = new InMemoryLogStore(maxLogCount: 10000);
        await store.AddBatchAsync(_logs100);
    }

    [Benchmark]
    public async Task DirectWrite_Batch1000()
    {
        var store = new InMemoryLogStore(maxLogCount: 10000);
        await store.AddBatchAsync(_logs1000);
    }

    [Benchmark]
    public async Task QueueWrite_Batch100()
    {
        var queue = new BackgroundLogQueue(capacity: 10000);
        foreach (var log in _logs100)
        {
            await queue.QueueLogEntryAsync(log);
        }
    }

    [Benchmark]
    public async Task QueueWrite_Batch1000()
    {
        var queue = new BackgroundLogQueue(capacity: 10000);
        foreach (var log in _logs1000)
        {
            await queue.QueueLogEntryAsync(log);
        }
    }

    [Benchmark]
    public async Task ConcurrentWrite_10Threads()
    {
        var store = new InMemoryLogStore(maxLogCount: 10000);
        var tasks = Enumerable.Range(0, 10).Select(async i =>
        {
            var logs = TestDataGenerator.CreateLogEntries(100);
            await store.AddBatchAsync(logs);
        });
        await Task.WhenAll(tasks);
    }

    [Benchmark]
    public async Task Query_Retrieve100()
    {
        // 先填充数据
        await _store.AddBatchAsync(_logs1000);

        // 查询 100 条
        var query = new LogQuery { PageSize = 100 };
        await _store.QueryAsync(query);
    }

    [Benchmark]
    public async Task Query_WithKeywordFilter()
    {
        // 先填充特定数据
        var logs = Enumerable.Range(0, 1000)
            .Select(i => TestDataGenerator.CreateLogEntry(
                level: i % 2 == 0 ? LogLevel.Error : LogLevel.Information,
                message: i % 3 == 0 ? "Error: timeout" : "Normal message"
            ))
            .ToList();

        var store = new InMemoryLogStore(maxLogCount: 10000);
        await store.AddBatchAsync(logs);

        // 查询包含 Error 的日志
        var query = new LogQuery { Keyword = "Error", PageSize = 100 };
        await store.QueryAsync(query);
    }

    [Benchmark]
    public async Task GetByRequestId_ExactMatch()
    {
        // 先填充数据
        var requestId = Guid.NewGuid().ToString("N");
        var logs = Enumerable.Range(0, 100)
            .Select(i => TestDataGenerator.CreateLogEntry(requestId: requestId))
            .ToList();

        var store = new InMemoryLogStore(maxLogCount: 10000);
        await store.AddBatchAsync(logs);

        // 按 RequestId 查询
        await store.GetByRequestIdAsync(requestId);
    }

    [Benchmark]
    public async Task GetTraceSummaries_Aggregation()
    {
        // 先填充数据 - 100 个不同的请求
        var logs = Enumerable.Range(0, 1000)
            .Select(i => TestDataGenerator.CreateLogEntry(
                requestId: (i % 100).ToString()
            ))
            .ToList();

        var store = new InMemoryLogStore(maxLogCount: 10000);
        await store.AddBatchAsync(logs);

        // 获取追踪汇总
        await store.GetTraceSummariesAsync(null, null);
    }
}

using Azrng.DevLogDashboard.Background;
using Azrng.DevLogDashboard.Models;
using Azrng.DevLogDashboard.Storage;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace DevLogDashboard.PerformanceTest.Stress;

/// <summary>
/// 内存占用和内存泄漏测试
/// </summary>
public class MemoryLeakTest
{
    private readonly ITestOutputHelper _output;

    public MemoryLeakTest(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task AddBatchAsync_WhenExceedsMaxCount_ShouldReleaseMemory()
    {
        // Arrange
        var maxCount = 1000;
        var store = new InMemoryLogStore(maxLogCount: maxCount);

        // 强制 GC 并记录初始内存
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        var initialMemory = GC.GetTotalMemory(true);

        _output.WriteLine($"Initial memory: {initialMemory / 1024.0 / 1024.0:F2} MB");

        // Act - 写入超过最大数量的日志（10 倍）
        for (int i = 0; i < 10; i++)
        {
            var logs = TestDataGenerator.CreateLogEntries(maxCount);
            await store.AddBatchAsync(logs);

            // 每 3 次检查一次内存
            if (i % 3 == 2)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                var currentMemory = GC.GetTotalMemory(true);
                _output.WriteLine($"After iteration {i + 1}: {currentMemory / 1024.0 / 1024.0:F2} MB");
            }
        }

        // 强制 GC 并获取最终内存
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        var finalMemory = GC.GetTotalMemory(true);
        var memoryIncreaseMB = (finalMemory - initialMemory) / 1024.0 / 1024.0;

        _output.WriteLine($"Final memory: {finalMemory / 1024.0 / 1024.0:F2} MB");
        _output.WriteLine($"Memory increase: {memoryIncreaseMB:F2} MB");

        // Assert - 内存增长应该被限制
        // 由于日志被裁剪，内存不应该持续增长
        memoryIncreaseMB.Should().BeLessThan(50);
    }

    [Fact]
    public async Task BackgroundLogQueue_NoMemoryLeak()
    {
        // Arrange
        var capacity = 10000;
        var queue = new BackgroundLogQueue(capacity: capacity);

        // 强制 GC 并记录初始内存
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        var initialMemory = GC.GetTotalMemory(true);

        _output.WriteLine($"Initial memory: {initialMemory / 1024.0 / 1024.0:F2} MB");

        // Act - 反复写入和读取
        for (int i = 0; i < 100; i++)
        {
            // 写入 100 条
            for (int j = 0; j < 100; j++)
            {
                await queue.QueueLogEntryAsync(TestDataGenerator.CreateLogEntry());
            }

            // 读取 100 条
            var batch = await queue.DequeueBatchAsync(100, CancellationToken.None);

            // 每 20 次检查一次内存
            if (i % 20 == 19)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                var currentMemory = GC.GetTotalMemory(true);
                _output.WriteLine($"After iteration {i + 1}: {currentMemory / 1024.0 / 1024.0:F2} MB");
            }
        }

        // 清空队列
        while (queue.GetQueuedCount() > 0)
        {
            await queue.DequeueBatchAsync(100, CancellationToken.None);
        }

        // 强制 GC 并获取最终内存
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        var finalMemory = GC.GetTotalMemory(true);
        var memoryIncreaseMB = (finalMemory - initialMemory) / 1024.0 / 1024.0;

        _output.WriteLine($"Final memory: {finalMemory / 1024.0 / 1024.0:F2} MB");
        _output.WriteLine($"Memory increase: {memoryIncreaseMB:F2} MB");

        // Assert - 队列不应该有明显的内存泄漏
        memoryIncreaseMB.Should().BeLessThan(10);
    }

    [Fact]
    public async Task StoreDoesNotGrowBeyondMaxCount()
    {
        // Arrange
        var maxCount = 500;
        var store = new InMemoryLogStore(maxLogCount: maxCount);

        // Act - 写入 10 倍于最大数量的日志
        for (int i = 0; i < 10; i++)
        {
            var logs = TestDataGenerator.CreateLogEntries(maxCount);
            await store.AddBatchAsync(logs);

            var result = await store.QueryAsync(new LogQuery { PageSize = int.MaxValue });
            _output.WriteLine($"Iteration {i + 1}: {result.Total} logs");

            // 每次迭代后都不应该超过最大数量
            result.Total.Should().BeLessOrEqualTo(maxCount);
        }

        // 最终验证
        var finalResult = await store.QueryAsync(new LogQuery { PageSize = int.MaxValue });
        finalResult.Total.Should().Be(maxCount);

        _output.WriteLine($"Final total logs: {finalResult.Total}");
    }

    [Fact]
    public async Task IndexCleanup_WhenLogsAreTrimmed()
    {
        // Arrange
        var maxCount = 100;
        var store = new InMemoryLogStore(maxLogCount: maxCount);
        var requestId = Guid.NewGuid().ToString("N");

        // Act - 添加同一请求的多条日志（超过最大数量）
        var logs = Enumerable.Range(0, 500)
            .Select(i => TestDataGenerator.CreateLogEntry(requestId: requestId))
            .ToList();

        await store.AddBatchAsync(logs);

        // 检查 RequestId 索引是否被正确清理
        var requestLogs = await store.GetByRequestIdAsync(requestId);

        _output.WriteLine($"Total logs for request: {requestLogs.Count}");

        // Assert - 索引中的日志数量应该与实际存储的日志数量一致
        var totalResult = await store.QueryAsync(new LogQuery { PageSize = int.MaxValue });
        requestLogs.Count.Should().BeLessOrEqualTo(maxCount);
        requestLogs.Count.Should().Be(totalResult.Total);
    }

    [Fact]
    public async Task TraceIndexCleanup_WhenLogsAreTrimmed()
    {
        // Arrange
        var maxCount = 50;
        var store = new InMemoryLogStore(maxLogCount: maxCount);

        // 创建多个请求的日志
        var requests = Enumerable.Range(0, 20)
            .Select(_ => Guid.NewGuid().ToString("N"))
            .ToList();

        var logs = requests
            .SelectMany(requestId => Enumerable.Range(0, 10)
                .Select(_ => TestDataGenerator.CreateLogEntry(requestId: requestId)))
            .ToList();

        await store.AddBatchAsync(logs);

        // 获取所有追踪汇总
        var summaries = await store.GetTraceSummariesAsync(null, null);

        _output.WriteLine($"Total trace summaries: {summaries.Count}");

        // Assert - 追踪汇总数量应该与实际请求数匹配
        // 由于日志被裁剪，某些请求可能完全没有日志
        var totalLogs = await store.QueryAsync(new LogQuery { PageSize = int.MaxValue });

        summaries.Count.Should().BeLessOrEqualTo(requests.Count);
        summaries.Sum(s => s.LogCount).Should().Be(totalLogs.Total);

        _output.WriteLine($"Total logs in store: {totalLogs.Total}");
        _output.WriteLine($"Total logs in trace index: {summaries.Sum(s => s.LogCount)}");
    }

    [Fact]
    public async Task MemoryEfficiency_ManySmallWrites()
    {
        // Arrange
        var store = new InMemoryLogStore(maxLogCount: 10000);

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        var initialMemory = GC.GetTotalMemory(true);

        // Act - 大量小批量写入
        for (int i = 0; i < 1000; i++)
        {
            var logs = TestDataGenerator.CreateLogEntries(10);
            await store.AddBatchAsync(logs);
        }

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        var finalMemory = GC.GetTotalMemory(true);
        var memoryIncreaseMB = (finalMemory - initialMemory) / 1024.0 / 1024.0;

        _output.WriteLine($"Memory after 1000 small writes: {memoryIncreaseMB:F2} MB");

        // Assert - 内存增长应该合理
        memoryIncreaseMB.Should().BeLessThan(100);
    }
}

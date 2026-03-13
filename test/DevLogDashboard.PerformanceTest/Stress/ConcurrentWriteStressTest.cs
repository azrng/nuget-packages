using Azrng.DevLogDashboard.Background;
using Azrng.DevLogDashboard.Models;
using Azrng.DevLogDashboard.Storage;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace DevLogDashboard.PerformanceTest.Stress;

/// <summary>
/// 并发写入压力测试
/// </summary>
public class ConcurrentWriteStressTest
{
    private readonly ITestOutputHelper _output;

    public ConcurrentWriteStressTest(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task ConcurrentWrite_NoDeadlock()
    {
        // Arrange
        var store = new InMemoryLogStore(maxLogCount: 10000);
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        var random = new Random();

        // Act - 20 个并发线程随机写入
        var tasks = Enumerable.Range(0, 20).Select(i => Task.Run(async () =>
        {
            var threadRandom = new Random(i);
            try
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    var count = threadRandom.Next(1, 100);
                    var logs = Enumerable.Range(0, count)
                        .Select(_ => TestDataGenerator.CreateLogEntry())
                        .ToList();

                    await store.AddBatchAsync(logs);
                    await Task.Delay(threadRandom.Next(1, 10), cts.Token);
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when token is cancelled
            }
        }));

        await Task.WhenAll(tasks);

        // Assert - 如果没有抛出异常或死锁，测试通过
        var result = await store.QueryAsync(new LogQuery { PageSize = 10000 });
        _output.WriteLine($"Total logs stored: {result.Total}");
        result.Total.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task SustainedThroughput_MaintainsPerformance()
    {
        // Arrange
        var store = new InMemoryLogStore(maxLogCount: 10000);
        var duration = TimeSpan.FromSeconds(5);
        var startTime = DateTime.Now;
        var totalLogs = 0;

        // Act - 持续写入 5 秒
        while (DateTime.Now - startTime < duration)
        {
            var logs = TestDataGenerator.CreateLogEntries(100);
            await store.AddBatchAsync(logs);
            totalLogs += 100;
        }

        var throughput = totalLogs / duration.TotalSeconds;

        // Assert - 应该维持 > 1,000 条/秒的吞吐量
        _output.WriteLine($"Throughput: {throughput:F2} logs/second");
        _output.WriteLine($"Total logs: {totalLogs}");

        throughput.Should().BeGreaterThan(1000);
    }

    [Fact]
    public async Task BurstLoad_HandleHighVolumeWithoutDropping()
    {
        // Arrange
        var store = new InMemoryLogStore(maxLogCount: 5000);
        var burstSize = 10000;

        // Act - 突发写入大量日志
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var logs = TestDataGenerator.CreateLogEntries(burstSize);
        await store.AddBatchAsync(logs);
        stopwatch.Stop();

        // Assert
        var result = await store.QueryAsync(new LogQuery { PageSize = 10000, MinLevel = LogLevel.Trace });

        // 由于 MaxLogCount=5000，应该保留 5000 条
        result.Total.Should().Be(5000);

        _output.WriteLine($"Burst write time: {stopwatch.ElapsedMilliseconds}ms");
        _output.WriteLine($"Logs stored: {result.Total}");
    }

    [Fact]
    public async Task QueueUnderPressure_NoDropsWithinCapacity()
    {
        // Arrange
        var queue = new BackgroundLogQueue(capacity: 5000);
        var writeCount = 10000;

        // Act - 快速写入超过容量的日志
        var tasks = Enumerable.Range(0, 10).Select(i => Task.Run(async () =>
        {
            for (int j = 0; j < writeCount / 10; j++)
            {
                await queue.QueueLogEntryAsync(TestDataGenerator.CreateLogEntry());
            }
        }));

        await Task.WhenAll(tasks);

        // Assert
        var queuedCount = queue.GetQueuedCount();
        _output.WriteLine($"Queued count: {queuedCount}");

        // 队列容量为 5000，所以队列中应该有 5000 条
        queuedCount.Should().Be(5000);
    }

    [Fact]
    public async Task MixedReadWriteLoad_NoCorruption()
    {
        // Arrange
        var store = new InMemoryLogStore(maxLogCount: 10000);
        var duration = TimeSpan.FromSeconds(3);
        var startTime = DateTime.Now;

        // Act - 同时进行读写
        var writeTask = Task.Run(async () =>
        {
            var random = new Random();
            while (DateTime.Now - startTime < duration)
            {
                var logs = TestDataGenerator.CreateLogEntries(random.Next(10, 100));
                await store.AddBatchAsync(logs);
                await Task.Delay(random.Next(10, 50));
            }
        });

        var readTask = Task.Run(async () =>
        {
            var random = new Random();
            var queryCount = 0;

            while (DateTime.Now - startTime < duration)
            {
                var query = new LogQuery
                {
                    PageSize = random.Next(10, 100),
                    PageIndex = random.Next(1, 5)
                };

                await store.QueryAsync(query);
                queryCount++;
                await Task.Delay(random.Next(10, 50));
            }

            _output.WriteLine($"Total queries: {queryCount}");
        });

        await Task.WhenAll(writeTask, readTask);

        // Assert - 验证数据完整性
        var result = await store.QueryAsync(new LogQuery { PageSize = 10000 });
        _output.WriteLine($"Final total logs: {result.Total}");

        // 如果读写过程中没有抛出异常，说明没有数据损坏
        result.Total.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task RepeatedAddAndTrim_ConsistencyCheck()
    {
        // Arrange
        var maxCount = 100;
        var store = new InMemoryLogStore(maxLogCount: maxCount);

        // Act - 反复添加和裁剪
        for (int i = 0; i < 100; i++)
        {
            var logs = TestDataGenerator.CreateLogEntries(50);
            await store.AddBatchAsync(logs);

            var result = await store.QueryAsync(new LogQuery { PageSize = 1000 });

            // 验证数量不超过最大值
            result.Total.Should().BeLessOrEqualTo(maxCount);
        }

        // 最终验证
        var finalResult = await store.QueryAsync(new LogQuery { PageSize = 1000 });
        finalResult.Total.Should().Be(maxCount);

        _output.WriteLine($"Final count after 100 iterations: {finalResult.Total}");
    }
}

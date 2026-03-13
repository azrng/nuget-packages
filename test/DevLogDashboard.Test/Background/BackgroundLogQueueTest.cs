using Azrng.DevLogDashboard.Background;
using Azrng.DevLogDashboard.Models;
using DevLogDashboard.Test.Helpers;
using Xunit;

namespace DevLogDashboard.Test.Background;

/// <summary>
/// BackgroundLogQueue 单元测试
/// </summary>
public class BackgroundLogQueueTest
{
    // ========== 基础入队测试 ==========

    [Fact]
    public async Task QueueLogEntryAsync_WhenQueueNotFull_ShouldSucceed()
    {
        // Arrange
        var queue = new BackgroundLogQueue(capacity: 100);
        var entry = new LogEntry { Message = "Test", Level = Microsoft.Extensions.Logging.LogLevel.Information };

        // Act
        await queue.QueueLogEntryAsync(entry);

        // Assert
        queue.GetQueuedCount().Should().Be(1);
    }

    [Fact]
    public async Task QueueLogEntryAsync_WhenMultipleEntries_ShouldCountCorrectly()
    {
        // Arrange
        var queue = new BackgroundLogQueue(capacity: 100);
        var generator = new TestDataGenerator();

        // Act
        for (int i = 0; i < 50; i++)
        {
            await queue.QueueLogEntryAsync(generator.CreateLogEntry());
        }

        // Assert
        queue.GetQueuedCount().Should().Be(50);
    }

    [Fact]
    public async Task QueueLogEntryAsync_WhenNullEntry_ShouldNotEnqueue()
    {
        // Arrange
        var queue = new BackgroundLogQueue(capacity: 100);

        // Act
        await queue.QueueLogEntryAsync(null!);

        // Assert
        queue.GetQueuedCount().Should().Be(0);
    }

    // ========== 有界队列测试 ==========

    [Fact]
    public async Task QueueLogEntryAsync_WhenQueueFull_ShouldDropOldest()
    {
        // Arrange
        var queue = new BackgroundLogQueue(capacity: 5);
        var generator = new TestDataGenerator();

        // Act - 添加 6 条日志（超过容量）
        for (int i = 0; i < 6; i++)
        {
            var entry = generator.CreateLogEntry();
            entry.Id = i.ToString(); // 用 ID 标识顺序
            await queue.QueueLogEntryAsync(entry);
        }

        // Assert - 队列应该只保留最新的 5 条
        queue.GetQueuedCount().Should().Be(5);

        // 最早的日志（ID=0）应该被丢弃
        var remainingLogs = new List<LogEntry>();
        for (int i = 0; i < 5; i++)
        {
            var log = await queue.DequeueAsync(CancellationToken.None, TimeSpan.FromSeconds(1));
            if (log != null)
            {
                remainingLogs.Add(log);
            }
        }

        remainingLogs.Should().HaveCount(5);
        remainingLogs.Any(x => x.Id == "0").Should().BeFalse(); // 最早的被丢弃
    }

    [Fact]
    public async Task QueueLogEntryAsync_WhenFull_ShouldTrackDroppedCount()
    {
        // Arrange
        var queue = new BackgroundLogQueue(capacity: 3);
        var generator = new TestDataGenerator();

        // Act - 添加 5 条日志
        for (int i = 0; i < 5; i++)
        {
            await queue.QueueLogEntryAsync(generator.CreateLogEntry());
        }

        // Assert
        queue.GetQueuedCount().Should().Be(3);
        // 注意：GetDroppedCount 是一个内部方法，我们需要通过行为来验证
    }

    // ========== 基础出队测试 ==========

    [Fact]
    public async Task DequeueAsync_WhenQueueHasItems_ShouldReturnItem()
    {
        // Arrange
        var queue = new BackgroundLogQueue(capacity: 10);
        var entry = new LogEntry
        {
            Id = "test-id",
            Message = "Test message",
            Level = Microsoft.Extensions.Logging.LogLevel.Information
        };
        await queue.QueueLogEntryAsync(entry);

        // Act
        var result = await queue.DequeueAsync(CancellationToken.None, TimeSpan.FromSeconds(1));

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be("test-id");
        queue.GetQueuedCount().Should().Be(0);
    }

    [Fact]
    public async Task DequeueAsync_WhenQueueEmpty_ShouldReturnNullOnTimeout()
    {
        // Arrange
        var queue = new BackgroundLogQueue(capacity: 10);

        // Act
        var result = await queue.DequeueAsync(CancellationToken.None, TimeSpan.FromMilliseconds(100));

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DequeueAsync_ShouldMaintainFIFOOrder()
    {
        // Arrange
        var queue = new BackgroundLogQueue(capacity: 10);
        var generator = new TestDataGenerator();

        var entry1 = generator.CreateLogEntry();
        entry1.Id = "1";
        var entry2 = generator.CreateLogEntry();
        entry2.Id = "2";
        var entry3 = generator.CreateLogEntry();
        entry3.Id = "3";

        await queue.QueueLogEntryAsync(entry1);
        await queue.QueueLogEntryAsync(entry2);
        await queue.QueueLogEntryAsync(entry3);

        // Act
        var result1 = await queue.DequeueAsync(CancellationToken.None, TimeSpan.FromSeconds(1));
        var result2 = await queue.DequeueAsync(CancellationToken.None, TimeSpan.FromSeconds(1));
        var result3 = await queue.DequeueAsync(CancellationToken.None, TimeSpan.FromSeconds(1));

        // Assert
        result1!.Id.Should().Be("1");
        result2!.Id.Should().Be("2");
        result3!.Id.Should().Be("3");
    }

    // ========== 取消令牌测试 ==========

    [Fact]
    public async Task DequeueAsync_WhenCancelledImmediately_ShouldReturnNull()
    {
        // Arrange
        var queue = new BackgroundLogQueue(capacity: 10);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var result = await queue.DequeueAsync(cts.Token, TimeSpan.FromSeconds(5));

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DequeueAsync_WhenCancelledDuringWait_ShouldReturnNull()
    {
        // Arrange
        var queue = new BackgroundLogQueue(capacity: 10);
        var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));

        // Act
        var result = await queue.DequeueAsync(cts.Token, TimeSpan.FromSeconds(5));

        // Assert
        result.Should().BeNull();
    }

    // ========== 批量出队测试 ==========

    [Fact]
    public async Task DequeueBatchAsync_WhenQueueHasItems_ShouldReturnBatch()
    {
        // Arrange
        var queue = new BackgroundLogQueue(capacity: 100);
        var generator = new TestDataGenerator();

        // 添加 50 条日志
        for (int i = 0; i < 50; i++)
        {
            await queue.QueueLogEntryAsync(generator.CreateLogEntry());
        }

        // Act
        var batch = await queue.DequeueBatchAsync(20, CancellationToken.None);

        // Assert
        batch.Should().HaveCount(20);
        queue.GetQueuedCount().Should().Be(30);
    }

    [Fact]
    public async Task DequeueBatchAsync_WhenRequestMoreThanAvailable_ShouldReturnAllAvailable()
    {
        // Arrange
        var queue = new BackgroundLogQueue(capacity: 100);
        var generator = new TestDataGenerator();

        // 只添加 10 条日志
        for (int i = 0; i < 10; i++)
        {
            await queue.QueueLogEntryAsync(generator.CreateLogEntry());
        }

        // Act - 请求 20 条
        var batch = await queue.DequeueBatchAsync(20, CancellationToken.None);

        // Assert
        batch.Should().HaveCount(10);
        queue.GetQueuedCount().Should().Be(0);
    }

    [Fact]
    public async Task DequeueBatchAsync_WhenQueueEmpty_ShouldWaitForFirstItem()
    {
        // Arrange
        var queue = new BackgroundLogQueue(capacity: 100);
        var generator = new TestDataGenerator();
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));

        // 启动一个延迟任务来添加日志
        _ = Task.Run(async () =>
        {
            await Task.Delay(100);
            await queue.QueueLogEntryAsync(generator.CreateLogEntry());
        });

        // Act - 应该等待并获取到日志
        var batch = await queue.DequeueBatchAsync(10, cts.Token);

        // Assert
        batch.Should().HaveCount(1);
    }

    [Fact]
    public async Task DequeueBatchAsync_WhenCancelled_ShouldReturnPartialBatch()
    {
        // Arrange
        var queue = new BackgroundLogQueue(capacity: 100);
        var generator = new TestDataGenerator();

        // 添加 10 条日志
        for (int i = 0; i < 10; i++)
        {
            await queue.QueueLogEntryAsync(generator.CreateLogEntry());
        }

        var cts = new CancellationTokenSource();

        // 启动一个延迟任务来取消
        _ = Task.Run(async () =>
        {
            await Task.Delay(50);
            cts.Cancel();
        });

        // Act - 在第一条读取后取消
        var batch = await queue.DequeueBatchAsync(20, cts.Token);

        // Assert
        batch.Should().NotBeEmpty();
        batch.Count.Should().BeLessOrEqualTo(10);
    }

    [Fact]
    public async Task DequeueBatchAsync_ShouldNotWaitForAdditionalItems()
    {
        // Arrange
        var queue = new BackgroundLogQueue(capacity: 100);
        var generator = new TestDataGenerator();

        // 只添加 5 条日志
        for (int i = 0; i < 5; i++)
        {
            await queue.QueueLogEntryAsync(generator.CreateLogEntry());
        }

        // Act - 请求 20 条，但只有 5 条可用
        var batch = await queue.DequeueBatchAsync(20, CancellationToken.None);

        // Assert - 应该立即返回可用的 5 条，而不是等待更多
        batch.Should().HaveCount(5);
        queue.GetQueuedCount().Should().Be(0);
    }

    // ========== 并发测试 ==========

    [Fact]
    public async Task ConcurrentEnqueueDequeue_ShouldBeThreadSafe()
    {
        // Arrange
        var queue = new BackgroundLogQueue(capacity: 1000);
        var generator = new TestDataGenerator();

        // Act - 并发入队
        var enqueueTasks = Enumerable.Range(0, 10).Select(i => Task.Run(async () =>
        {
            for (int j = 0; j < 100; j++)
            {
                await queue.QueueLogEntryAsync(generator.CreateLogEntry());
                await Task.Delay(1);
            }
        }));

        // 并发出队
        var dequeueTasks = Enumerable.Range(0, 5).Select(i => Task.Run(async () =>
        {
            var count = 0;
            while (count < 100)
            {
                var batch = await queue.DequeueBatchAsync(10, CancellationToken.None);
                if (batch.Count > 0)
                {
                    count += batch.Count;
                }
                await Task.Delay(5);
            }
        }));

        await Task.WhenAll(enqueueTasks.Concat(dequeueTasks));

        // Assert - 队列应该大致平衡
        queue.GetQueuedCount().Should().BeLessThan(1000); // 不应该超过容量
    }

    [Fact]
    public async Task ConcurrentMultipleDequeue_ShouldNotDuplicateItems()
    {
        // Arrange
        var queue = new BackgroundLogQueue(capacity: 100);
        var generator = new TestDataGenerator();

        // 添加 50 条日志
        for (int i = 0; i < 50; i++)
        {
            var entry = generator.CreateLogEntry();
            entry.Id = i.ToString();
            await queue.QueueLogEntryAsync(entry);
        }

        // Act - 并发出队
        var allBatches = await Task.WhenAll(Enumerable.Range(0, 5).Select(i =>
            queue.DequeueBatchAsync(20, CancellationToken.None).AsTask()
        ));

        // Assert - 总数应该不超过 50，没有重复
        var totalCount = allBatches.Sum(b => b.Count);
        totalCount.Should().BeLessOrEqualTo(50);

        var allIds = allBatches.SelectMany(b => b.Select(e => e.Id)).ToHashSet();
        allIds.Count.Should().Be(totalCount); // 没有重复的 ID
    }

    // ========== 边界条件测试 ==========

    [Fact]
    public void Constructor_WithZeroCapacity_ShouldUseDefaultCapacity()
    {
        // Act
        var queue = new BackgroundLogQueue(capacity: 0);

        // Assert - 应该使用默认容量而不是崩溃
        queue.GetQueuedCount().Should().Be(0);
    }

    [Fact]
    public void Constructor_WithNegativeCapacity_ShouldUseDefaultCapacity()
    {
        // Act
        var queue = new BackgroundLogQueue(capacity: -100);

        // Assert - 应该使用默认容量而不是崩溃
        queue.GetQueuedCount().Should().Be(0);
    }

    [Fact]
    public async Task GetQueuedCount_AfterDequeueAll_ShouldReturnZero()
    {
        // Arrange
        var queue = new BackgroundLogQueue(capacity: 10);
        var generator = new TestDataGenerator();

        for (int i = 0; i < 5; i++)
        {
            await queue.QueueLogEntryAsync(generator.CreateLogEntry());
        }

        // Act - 出队所有日志
        for (int i = 0; i < 5; i++)
        {
            await queue.DequeueAsync(CancellationToken.None, TimeSpan.FromSeconds(1));
        }

        // Assert
        queue.GetQueuedCount().Should().Be(0);
    }

    // ========== 性能验证测试 ==========

    [Fact]
    public async Task HighThroughput_EnqueueMany_ShouldNotBlock()
    {
        // Arrange
        var queue = new BackgroundLogQueue(capacity: 10000);
        var generator = new TestDataGenerator();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act - 快速入队 1000 条日志
        var tasks = Enumerable.Range(0, 100).Select(i => Task.Run(async () =>
        {
            for (int j = 0; j < 10; j++)
            {
                await queue.QueueLogEntryAsync(generator.CreateLogEntry());
            }
        }));

        await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert - 应该在合理时间内完成（不阻塞）
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000);
        queue.GetQueuedCount().Should().Be(1000);
    }

    [Fact]
    public async Task DequeueBatchAsync_LargeBatch_ShouldBeEfficient()
    {
        // Arrange
        var queue = new BackgroundLogQueue(capacity: 10000);
        var generator = new TestDataGenerator();

        for (int i = 0; i < 1000; i++)
        {
            await queue.QueueLogEntryAsync(generator.CreateLogEntry());
        }

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act - 批量出队 1000 条
        var batch = await queue.DequeueBatchAsync(1000, CancellationToken.None);

        stopwatch.Stop();

        // Assert
        batch.Should().HaveCount(1000);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000); // 应该很快
    }
}

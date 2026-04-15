using Azrng.DevLogDashboard.Background;
using Azrng.DevLogDashboard.Models;
using Azrng.DevLogDashboard.Storage;
using DevLogDashboard.Test.Helpers;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DevLogDashboard.Test.Background;

/// <summary>
/// BackgroundLogWriter 单元测试
/// </summary>
public class BackgroundLogWriterTest
{
    private readonly TestDataGenerator _generator;

    public BackgroundLogWriterTest()
    {
        _generator = new TestDataGenerator();
    }

    private static async Task RunWriterForAsync(BackgroundLogWriter writer, TimeSpan runDuration, CancellationToken stopToken = default)
    {
        await writer.StartAsync(CancellationToken.None);

        try
        {
            await Task.Delay(runDuration, stopToken);
        }
        catch (OperationCanceledException) when (stopToken.IsCancellationRequested)
        {
            // 测试主动结束时允许中断等待
        }
        finally
        {
            await writer.StopAsync(CancellationToken.None);
        }
    }

    // ========== 构造函数测试 ==========

    [Fact]
    public void Constructor_WhenQueueIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        var storeMock = new Mock<ILogStore>();
        var loggerMock = new Mock<ILogger<BackgroundLogWriter>>();
        var options = new BackgroundLogWriterOptions();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
        {
            new BackgroundLogWriter(null!, storeMock.Object, loggerMock.Object, options);
        });
    }

    [Fact]
    public void Constructor_WhenStoreIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        var queueMock = new Mock<IBackgroundLogQueue>();
        var loggerMock = new Mock<ILogger<BackgroundLogWriter>>();
        var options = new BackgroundLogWriterOptions();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
        {
            new BackgroundLogWriter(queueMock.Object, null!, loggerMock.Object, options);
        });
    }

    [Fact]
    public void Constructor_WhenLoggerIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        var queueMock = new Mock<IBackgroundLogQueue>();
        var storeMock = new Mock<ILogStore>();
        var options = new BackgroundLogWriterOptions();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
        {
            new BackgroundLogWriter(queueMock.Object, storeMock.Object, null!, options);
        });
    }

    [Fact]
    public void Constructor_WhenOptionsIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        var queueMock = new Mock<IBackgroundLogQueue>();
        var storeMock = new Mock<ILogStore>();
        var loggerMock = new Mock<ILogger<BackgroundLogWriter>>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
        {
            new BackgroundLogWriter(queueMock.Object, storeMock.Object, loggerMock.Object, null!);
        });
    }

    // ========== 基础执行测试 ==========

    [Fact]
    public async Task ExecuteAsync_ShouldProcessBatches()
    {
        // Arrange
        var queueMock = new Mock<IBackgroundLogQueue>();
        var storeMock = new Mock<ILogStore>();
        var loggerMock = new Mock<ILogger<BackgroundLogWriter>>();

        var logs = _generator.CreateLogEntries(5);

        queueMock.Setup(x => x.DequeueBatchAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(logs);
        queueMock.Setup(x => x.GetQueuedCount()).Returns(0);

        var options = new BackgroundLogWriterOptions
        {
            BatchSize = 10,
            PollInterval = TimeSpan.FromMilliseconds(100)
        };

        var writer = new BackgroundLogWriter(
            queueMock.Object,
            storeMock.Object,
            loggerMock.Object,
            options
        );

        // Act
        await RunWriterForAsync(writer, TimeSpan.FromMilliseconds(150));

        // Assert
        storeMock.Verify(x => x.AddBatchAsync(
            It.Is<IEnumerable<LogEntry>>(items => items.Count() == 5),
            It.IsAny<CancellationToken>()
        ), Times.AtLeastOnce());
    }

    [Fact]
    public async Task ExecuteAsync_WhenQueueHasMore_ShouldContinueProcessing()
    {
        // Arrange
        var queueMock = new Mock<IBackgroundLogQueue>();
        var storeMock = new Mock<ILogStore>();
        var loggerMock = new Mock<ILogger<BackgroundLogWriter>>();

        var logs1 = _generator.CreateLogEntries(10);
        var logs2 = _generator.CreateLogEntries(10);

        var callCount = 0;
        queueMock.Setup(x => x.DequeueBatchAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                callCount++;
                if (callCount == 1)
                {
                    queueMock.Setup(x => x.GetQueuedCount()).Returns(10);
                    return logs1;
                }
                else
                {
                    queueMock.Setup(x => x.GetQueuedCount()).Returns(0);
                    return logs2;
                }
            });

        var options = new BackgroundLogWriterOptions
        {
            BatchSize = 10,
            PollInterval = TimeSpan.FromMilliseconds(50)
        };

        var writer = new BackgroundLogWriter(
            queueMock.Object,
            storeMock.Object,
            loggerMock.Object,
            options
        );

        // Act
        await RunWriterForAsync(writer, TimeSpan.FromMilliseconds(300));

        // Assert - 应该至少处理两次
        storeMock.Verify(x => x.AddBatchAsync(
            It.IsAny<IEnumerable<LogEntry>>(),
            It.IsAny<CancellationToken>()
        ), Times.AtLeast(2));
    }

    // ========== 异常处理测试 ==========

    [Fact]
    public async Task ExecuteAsync_WhenStoreThrows_ShouldLogAndContinue()
    {
        // Arrange
        var queueMock = new Mock<IBackgroundLogQueue>();
        var storeMock = new Mock<ILogStore>();
        var loggerMock = new Mock<ILogger<BackgroundLogWriter>>();

        var logs = _generator.CreateLogEntries(5);

        queueMock.Setup(x => x.DequeueBatchAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(logs);
        queueMock.Setup(x => x.GetQueuedCount()).Returns(0);

        storeMock.Setup(x => x.AddBatchAsync(It.IsAny<IEnumerable<LogEntry>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Store error"));

        var options = new BackgroundLogWriterOptions
        {
            BatchSize = 10,
            PollInterval = TimeSpan.FromMilliseconds(100)
        };

        var writer = new BackgroundLogWriter(
            queueMock.Object,
            storeMock.Object,
            loggerMock.Object,
            options
        );

        // Act & Assert - 不应该抛出异常
        var act = async () =>
        {
            await RunWriterForAsync(writer, TimeSpan.FromMilliseconds(200));
        };
        await act.Should().NotThrowAsync();

        // 验证错误被记录
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenQueueThrows_ShouldLogAndContinue()
    {
        // Arrange
        var queueMock = new Mock<IBackgroundLogQueue>();
        var storeMock = new Mock<ILogStore>();
        var loggerMock = new Mock<ILogger<BackgroundLogWriter>>();

        queueMock.Setup(x => x.DequeueBatchAsync(10, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Queue error"));

        var options = new BackgroundLogWriterOptions
        {
            BatchSize = 10,
            PollInterval = TimeSpan.FromMilliseconds(100)
        };

        var writer = new BackgroundLogWriter(
            queueMock.Object,
            storeMock.Object,
            loggerMock.Object,
            options
        );

        // Act & Assert - 不应该抛出异常
        var act = async () =>
        {
            await RunWriterForAsync(writer, TimeSpan.FromMilliseconds(200));
        };
        await act.Should().NotThrowAsync();

        // 验证错误被记录
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    // ========== 关闭和刷新测试 ==========

    [Fact]
    public async Task ExecuteAsync_WhenCancelled_ShouldFlushRemainingLogs()
    {
        // Arrange
        var queueMock = new Mock<IBackgroundLogQueue>();
        var storeMock = new Mock<ILogStore>();
        var loggerMock = new Mock<ILogger<BackgroundLogWriter>>();

        var logs = _generator.CreateLogEntries(10);

        queueMock.Setup(x => x.DequeueBatchAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(logs);
        queueMock.Setup(x => x.GetQueuedCount()).Returns(10);

        var options = new BackgroundLogWriterOptions
        {
            BatchSize = 10,
            PollInterval = TimeSpan.FromMilliseconds(100)
        };

        var writer = new BackgroundLogWriter(
            queueMock.Object,
            storeMock.Object,
            loggerMock.Object,
            options
        );

        // Act - 先启动服务
        await RunWriterForAsync(writer, TimeSpan.FromMilliseconds(200));

        // Assert - 应该调用 AddBatchAsync 至少一次
        storeMock.Verify(x => x.AddBatchAsync(
            It.IsAny<IEnumerable<LogEntry>>(),
            It.IsAny<CancellationToken>()
        ), Times.AtLeastOnce());
    }

    [Fact]
    public async Task ExecuteAsync_OnShutdown_ShouldLogShutdownMessage()
    {
        // Arrange
        var queueMock = new Mock<IBackgroundLogQueue>();
        var storeMock = new Mock<ILogStore>();
        var loggerMock = new Mock<ILogger<BackgroundLogWriter>>();

        queueMock.Setup(x => x.DequeueBatchAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<LogEntry>());
        queueMock.Setup(x => x.GetQueuedCount()).Returns(0);

        var options = new BackgroundLogWriterOptions
        {
            BatchSize = 10,
            PollInterval = TimeSpan.FromMilliseconds(100)
        };

        var writer = new BackgroundLogWriter(
            queueMock.Object,
            storeMock.Object,
            loggerMock.Object,
            options
        );

        // Act
        await RunWriterForAsync(writer, TimeSpan.FromMilliseconds(150));

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("started")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("stopped")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    // ========== 批量大小测试 ==========

    [Fact]
    public async Task ExecuteAsync_ShouldRespectBatchSize()
    {
        // Arrange
        var queueMock = new Mock<IBackgroundLogQueue>();
        var storeMock = new Mock<ILogStore>();
        var loggerMock = new Mock<ILogger<BackgroundLogWriter>>();

        var batchSize = 5;
        var logs = _generator.CreateLogEntries(10);

        queueMock.Setup(x => x.DequeueBatchAsync(batchSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync(logs.Take(batchSize).ToList());
        queueMock.Setup(x => x.GetQueuedCount()).Returns(10);

        var options = new BackgroundLogWriterOptions
        {
            BatchSize = batchSize,
            PollInterval = TimeSpan.FromMilliseconds(100)
        };

        var writer = new BackgroundLogWriter(
            queueMock.Object,
            storeMock.Object,
            loggerMock.Object,
            options
        );

        // Act
        await RunWriterForAsync(writer, TimeSpan.FromMilliseconds(200));

        // Assert - 应该使用指定的批量大小
        queueMock.Verify(x => x.DequeueBatchAsync(batchSize, It.IsAny<CancellationToken>()), Times.AtLeastOnce());
    }

    // ========== 轮询间隔测试 ==========

    [Fact]
    public async Task ExecuteAsync_ShouldRespectPollInterval()
    {
        // Arrange
        var queueMock = new Mock<IBackgroundLogQueue>();
        var storeMock = new Mock<ILogStore>();
        var loggerMock = new Mock<ILogger<BackgroundLogWriter>>();

        queueMock.Setup(x => x.DequeueBatchAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<LogEntry>());
        queueMock.Setup(x => x.GetQueuedCount()).Returns(0);

        var pollInterval = TimeSpan.FromMilliseconds(200);

        var options = new BackgroundLogWriterOptions
        {
            BatchSize = 10,
            PollInterval = pollInterval
        };

        var writer = new BackgroundLogWriter(
            queueMock.Object,
            storeMock.Object,
            loggerMock.Object,
            options
        );

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        await RunWriterForAsync(writer, TimeSpan.FromMilliseconds(300));

        stopwatch.Stop();

        // Assert - 应该至少调用 DequeueBatchAsync 两次
        queueMock.Verify(x => x.DequeueBatchAsync(10, It.IsAny<CancellationToken>()), Times.AtLeast(2));
    }

    // ========== 空队列处理测试 ==========

    [Fact]
    public async Task ExecuteAsync_WhenQueueEmpty_ShouldWaitAndRetry()
    {
        // Arrange
        var queueMock = new Mock<IBackgroundLogQueue>();
        var storeMock = new Mock<ILogStore>();
        var loggerMock = new Mock<ILogger<BackgroundLogWriter>>();

        queueMock.Setup(x => x.DequeueBatchAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<LogEntry>());
        queueMock.Setup(x => x.GetQueuedCount()).Returns(0);

        var options = new BackgroundLogWriterOptions
        {
            BatchSize = 10,
            PollInterval = TimeSpan.FromMilliseconds(50)
        };

        var writer = new BackgroundLogWriter(
            queueMock.Object,
            storeMock.Object,
            loggerMock.Object,
            options
        );

        // Act
        await RunWriterForAsync(writer, TimeSpan.FromMilliseconds(200));

        // Assert - 应该多次尝试读取（即使队列为空）
        queueMock.Verify(x => x.DequeueBatchAsync(10, It.IsAny<CancellationToken>()), Times.AtLeast(2));
    }

    // ========== 调试日志测试 ==========

    [Fact]
    public async Task ExecuteAsync_ShouldLogDebugInformation()
    {
        // Arrange
        var queueMock = new Mock<IBackgroundLogQueue>();
        var storeMock = new Mock<ILogStore>();
        var loggerMock = new Mock<ILogger<BackgroundLogWriter>>();

        var logs = _generator.CreateLogEntries(5);

        queueMock.Setup(x => x.DequeueBatchAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(logs);
        queueMock.Setup(x => x.GetQueuedCount()).Returns(0);

        var options = new BackgroundLogWriterOptions
        {
            BatchSize = 10,
            PollInterval = TimeSpan.FromMilliseconds(100)
        };

        var writer = new BackgroundLogWriter(
            queueMock.Object,
            storeMock.Object,
            loggerMock.Object,
            options
        );

        // Act
        await RunWriterForAsync(writer, TimeSpan.FromMilliseconds(200));

        // Assert - 应该记录调试信息
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Persisted")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    // ========== Options 默认值测试 ==========

    [Fact]
    public void BackgroundLogWriterOptions_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var options = new BackgroundLogWriterOptions();

        // Assert
        options.BatchSize.Should().Be(100);
        options.PollInterval.Should().Be(TimeSpan.FromSeconds(1));
        options.ShutdownFlushTimeout.Should().Be(TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task ExecuteAsync_ShouldInitializeStoreBeforeWriting()
    {
        // Arrange
        var queue = new BackgroundLogQueue(capacity: 10);
        var storeMock = new Mock<ILogStore>();
        var loggerMock = new Mock<ILogger<BackgroundLogWriter>>();

        var logs = _generator.CreateLogEntries(2);
        foreach (var log in logs)
        {
            await queue.QueueLogEntryAsync(log);
        }

        storeMock.Setup(x => x.InitializeAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        storeMock.Setup(x => x.AddBatchAsync(It.IsAny<IEnumerable<LogEntry>>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        var writer = new BackgroundLogWriter(
            queue,
            storeMock.Object,
            loggerMock.Object,
            new BackgroundLogWriterOptions
            {
                BatchSize = 10,
                PollInterval = TimeSpan.FromMilliseconds(20)
            });

        // Act
        await writer.StartAsync(CancellationToken.None);
        await Task.Delay(100);
        await writer.StopAsync(CancellationToken.None);

        // Assert
        storeMock.Verify(x => x.InitializeAsync(It.IsAny<CancellationToken>()), Times.Once);
        storeMock.Verify(x => x.AddBatchAsync(
            It.Is<IEnumerable<LogEntry>>(items => items.Count() == 2),
            It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }
}

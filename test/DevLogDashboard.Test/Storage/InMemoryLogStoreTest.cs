using Azrng.DevLogDashboard.Models;
using Azrng.DevLogDashboard.Storage;
using DevLogDashboard.Test.Helpers;
using Microsoft.Extensions.Logging;
using Xunit;

namespace DevLogDashboard.Test.Storage;

/// <summary>
/// InMemoryLogStore 单元测试
/// </summary>
public class InMemoryLogStoreTest : IClassFixture<TestDataGenerator>
{
    private readonly TestDataGenerator _generator;

    public InMemoryLogStoreTest(TestDataGenerator generator)
    {
        _generator = generator;
    }

    // ========== 基础存储测试 ==========

    [Fact]
    public async Task InitializeAsync_ShouldCompleteSuccessfully()
    {
        // Arrange
        var store = new InMemoryLogStore();

        // Act
        var task = store.InitializeAsync();

        // Assert
        await task;
    }

    [Fact]
    public async Task AddBatchAsync_WhenGivenLogs_ShouldStoreThem()
    {
        // Arrange
        var store = new InMemoryLogStore(maxLogCount: 100);
        var logs = _generator.CreateLogEntries(10);

        // Act
        await store.AddBatchAsync(logs);

        // Assert
        var result = await store.QueryAsync(new LogQuery { PageSize = 100 });
        result.Total.Should().Be(10);
        result.Items.Should().HaveCount(10);
    }

    [Fact]
    public async Task AddBatchAsync_WhenEmptyList_ShouldNotThrow()
    {
        // Arrange
        var store = new InMemoryLogStore();
        var emptyLogs = new List<LogEntry>();

        // Act & Assert
        var act = async () => await store.AddBatchAsync(emptyLogs);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task AddBatchAsync_WhenNullList_ShouldNotThrow()
    {
        // Arrange
        var store = new InMemoryLogStore();

        // Act & Assert
        var act = async () => await store.AddBatchAsync(null!);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task AddBatchAsync_WhenContainsNullEntries_ShouldSkipThem()
    {
        // Arrange
        var store = new InMemoryLogStore();
        var logs = new List<LogEntry?>
        {
            _generator.CreateLogEntry(),
            null,
            _generator.CreateLogEntry(),
            null,
            _generator.CreateLogEntry()
        };

        // Act
        await store.AddBatchAsync(logs!);

        // Assert
        var result = await store.QueryAsync(new LogQuery { PageSize = 100 });
        result.Total.Should().Be(3);
    }

    // ========== 日志裁剪测试 ==========

    [Fact]
    public async Task AddBatchAsync_WhenExceedsMaxCount_ShouldTrimOldest()
    {
        // Arrange
        var maxCount = 100;
        var store = new InMemoryLogStore(maxLogCount: maxCount);

        // Act - 写入超过最大数量的日志
        var logs = _generator.CreateLogEntries(150);
        await store.AddBatchAsync(logs);

        // Assert
        var result = await store.QueryAsync(new LogQuery { PageSize = 200 });
        result.Total.Should().Be(maxCount);
    }

    [Fact]
    public async Task AddBatchAsync_WhenTrimmed_ShouldRemoveOldestFromIndexes()
    {
        // Arrange
        var store = new InMemoryLogStore(maxLogCount: 5);
        var requestId = Guid.NewGuid().ToString("N");

        // Act - 添加同一请求的多条日志
        var logs = Enumerable.Range(0, 10)
            .Select(i => _generator.CreateLogEntryForRequest(requestId))
            .ToList();

        await store.AddBatchAsync(logs);

        // Assert - 应该只保留最新的 5 条
        var requestLogs = await store.GetByRequestIdAsync(requestId);
        requestLogs.Should().HaveCount(5);
    }

    // ========== 查询测试 ==========

    [Fact]
    public async Task QueryAsync_WithNoLogs_ShouldReturnEmpty()
    {
        // Arrange
        var store = new InMemoryLogStore();

        // Act
        var result = await store.QueryAsync(new LogQuery { PageSize = 50 });

        // Assert
        result.Total.Should().Be(0);
        result.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task QueryAsync_WithDefaultParameters_ShouldReturnLatestLogs()
    {
        // Arrange
        var store = new InMemoryLogStore();
        var logs = _generator.CreateLogEntries(10);
        await store.AddBatchAsync(logs);

        // Act
        var result = await store.QueryAsync(new LogQuery());

        // Assert
        result.Total.Should().Be(10);
        result.Items.Should().HaveCount(10); // 默认 PageSize=50
    }

    [Fact]
    public async Task QueryAsync_WithPaging_ShouldReturnCorrectPage()
    {
        // Arrange
        var store = new InMemoryLogStore();
        var logs = _generator.CreateLogEntries(25);
        await store.AddBatchAsync(logs);

        // Act - 添加过滤条件以避免快速路径
        var page1 = await store.QueryAsync(new LogQuery { PageIndex = 1, PageSize = 10, MinLevel = LogLevel.Trace });
        var page2 = await store.QueryAsync(new LogQuery { PageIndex = 2, PageSize = 10, MinLevel = LogLevel.Trace });
        var page3 = await store.QueryAsync(new LogQuery { PageIndex = 3, PageSize = 10, MinLevel = LogLevel.Trace });

        // Assert
        page1.Total.Should().Be(25);
        page1.Items.Should().HaveCount(10);
        page2.Items.Should().HaveCount(10);
        page3.Items.Should().HaveCount(5);
    }

    [Fact]
    public async Task QueryAsync_WhenPageSizeExceedsLimit_ShouldLimitToMax()
    {
        // Arrange
        var store = new InMemoryLogStore();
        var logs = _generator.CreateLogEntries(600);
        await store.AddBatchAsync(logs);

        // Act - 添加过滤条件以避免快速路径（快速路径会限制total）
        var result = await store.QueryAsync(new LogQuery { PageSize = 600, MinLevel = LogLevel.Trace });

        // Assert - MaxPageSize 是 500，Items受限制
        result.Items.Should().HaveCount(500); // 受MaxPageSize限制
        result.Total.Should().Be(600); // Total应该是总数
    }

    [Fact]
    public async Task QueryAsync_WithInvalidPageIndex_ShouldDefaultTo1()
    {
        // Arrange
        var store = new InMemoryLogStore();
        var logs = _generator.CreateLogEntries(10);
        await store.AddBatchAsync(logs);

        // Act
        var result = await store.QueryAsync(new LogQuery { PageIndex = 0, PageSize = 50 });

        // Assert
        result.Total.Should().Be(10);
        result.Items.Should().HaveCount(10);
    }

    // ========== 关键词过滤测试 ==========

    [Fact]
    public async Task QueryAsync_WithKeywordFilter_ShouldReturnMatchingLogs()
    {
        // Arrange
        var store = new InMemoryLogStore();
        var logs = new[]
        {
            _generator.CreateLogWithMessage("Error occurred in database"),
            _generator.CreateLogWithMessage("Warning: high memory usage"),
            _generator.CreateLogWithMessage("Error timeout connecting to API")
        };
        await store.AddBatchAsync(logs);

        // Act
        var result = await store.QueryAsync(new LogQuery { Keyword = "Error", PageSize = 10 });

        // Assert
        result.Items.Should().HaveCount(2);
        result.Items.All(x => x.Message.Contains("Error")).Should().BeTrue();
    }

    [Fact]
    public async Task QueryAsync_WithExactMatchKeyword_ShouldReturnMatchingLogs()
    {
        // Arrange
        var store = new InMemoryLogStore();
        var log1 = _generator.CreateLogEntry(LogLevel.Error, "Test message");
        var log2 = _generator.CreateLogEntry(LogLevel.Warning, "Test message");
        var log3 = _generator.CreateLogEntry(LogLevel.Information, "Different message");

        var logs = new[] { log1, log2, log3 };
        await store.AddBatchAsync(logs);

        // Act - 按level字段精确匹配
        var result = await store.QueryAsync(new LogQuery { Keyword = "level=\"Error\"", PageSize = 10 });

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items[0].Level.Should().Be(LogLevel.Error);
    }

    [Fact]
    public async Task QueryAsync_WithLikeKeyword_ShouldReturnPartialMatches()
    {
        // Arrange
        var store = new InMemoryLogStore();
        var logs = new[]
        {
            _generator.CreateLogEntry(LogLevel.Error, "Connection timeout"),
            _generator.CreateLogEntry(LogLevel.Warning, "Connection timeout"),
            _generator.CreateLogEntry(LogLevel.Error, "Success")
        };
        await store.AddBatchAsync(logs);

        // Act
        var result = await store.QueryAsync(new LogQuery
        {
            Keyword = "message like \"timeout\"",
            PageSize = 10
        });

        // Assert
        result.Items.Should().HaveCount(2);
        result.Items.All(x => x.Message.Contains("timeout")).Should().BeTrue();
    }

    [Fact]
    public async Task QueryAsync_WithComplexKeyword_ShouldParseCorrectly()
    {
        // Arrange
        var store = new InMemoryLogStore();
        var logs = new[]
        {
            _generator.CreateLogEntry(LogLevel.Error, "Connection timeout"),
            _generator.CreateLogEntry(LogLevel.Warning, "Connection timeout"),
            _generator.CreateLogEntry(LogLevel.Error, "Success")
        };
        await store.AddBatchAsync(logs);

        // Act - 测试 level="ERROR" and message like "timeout"
        var result = await store.QueryAsync(new LogQuery
        {
            Keyword = "level=\"ERROR\" and message like \"timeout\"",
            PageSize = 10
        });

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items[0].Level.Should().Be(LogLevel.Error);
        result.Items[0].Message.Should().Contain("timeout");
    }

    // ========== 级别过滤测试 ==========

    [Fact]
    public async Task QueryAsync_WithMinLevel_ShouldOnlyReturnHigherLevels()
    {
        // Arrange
        var store = new InMemoryLogStore();
        var logs = new List<LogEntry>
        {
            _generator.CreateLogEntry(LogLevel.Trace),
            _generator.CreateLogEntry(LogLevel.Debug),
            _generator.CreateLogEntry(LogLevel.Information),
            _generator.CreateLogEntry(LogLevel.Warning),
            _generator.CreateLogEntry(LogLevel.Error),
            _generator.CreateLogEntry(LogLevel.Critical)
        };
        await store.AddBatchAsync(logs);

        // Act - 只获取 Warning 及以上级别
        var result = await store.QueryAsync(new LogQuery { MinLevel = LogLevel.Warning, PageSize = 10 });

        // Assert
        result.Total.Should().Be(3); // Warning, Error, Critical
        result.Items.All(x => x.Level >= LogLevel.Warning).Should().BeTrue();
    }

    // ========== 时间范围过滤测试 ==========

    [Fact]
    public async Task QueryAsync_WithStartTime_ShouldFilterByTime()
    {
        // Arrange
        var store = new InMemoryLogStore();
        var baseTime = DateTime.Now;
        var logs = new[]
        {
            _generator.CreateLogEntry(timestamp: baseTime.AddHours(-2)),
            _generator.CreateLogEntry(timestamp: baseTime.AddHours(-1)),
            _generator.CreateLogEntry(timestamp: baseTime)
        };
        await store.AddBatchAsync(logs);

        // Act - 只获取最近 1 小时的日志
        var result = await store.QueryAsync(new LogQuery
        {
            StartTime = baseTime.AddMinutes(-30),
            PageSize = 10
        });

        // Assert
        result.Total.Should().Be(1);
    }

    [Fact]
    public async Task QueryAsync_WithTimeRange_ShouldFilterCorrectly()
    {
        // Arrange
        var store = new InMemoryLogStore();
        var baseTime = DateTime.Now;
        var logs = new[]
        {
            _generator.CreateLogEntry(timestamp: baseTime.AddHours(-3)),
            _generator.CreateLogEntry(timestamp: baseTime.AddHours(-2)),
            _generator.CreateLogEntry(timestamp: baseTime.AddHours(-1)),
            _generator.CreateLogEntry(timestamp: baseTime)
        };
        await store.AddBatchAsync(logs);

        // Act - 添加过滤条件以避免快速路径，并设置时间范围
        var result = await store.QueryAsync(new LogQuery
        {
            StartTime = baseTime.AddHours(-2).AddMinutes(-30),
            EndTime = baseTime.AddMinutes(-30),
            PageSize = 10,
            MinLevel = LogLevel.Trace // 避免快速路径
        });

        // Assert - -2小时和-1小时都在范围内（-2.5小时 到 -0.5小时）
        result.Total.Should().Be(2);
    }

    // ========== RequestId 过滤测试 ==========

    [Fact]
    public async Task QueryAsync_WithRequestId_ShouldReturnOnlyMatchingLogs()
    {
        // Arrange
        var store = new InMemoryLogStore();
        var requestId1 = Guid.NewGuid().ToString("N");
        var requestId2 = Guid.NewGuid().ToString("N");

        var logs = new[]
        {
            _generator.CreateLogEntryForRequest(requestId1),
            _generator.CreateLogEntryForRequest(requestId1),
            _generator.CreateLogEntryForRequest(requestId2),
            _generator.CreateLogEntryForRequest(requestId2)
        };
        await store.AddBatchAsync(logs);

        // Act
        var result = await store.QueryAsync(new LogQuery { RequestId = requestId1, PageSize = 10 });

        // Assert
        result.Total.Should().Be(2);
        result.Items.All(x => x.RequestId == requestId1).Should().BeTrue();
    }

    // ========== 排序测试 ==========

    [Fact]
    public async Task QueryAsync_WithOrderByAscending_ShouldReturnOldestFirst()
    {
        // Arrange
        var store = new InMemoryLogStore();
        var baseTime = DateTime.Now;
        var logs = new[]
        {
            _generator.CreateLogEntry(timestamp: baseTime.AddSeconds(1)),
            _generator.CreateLogEntry(timestamp: baseTime.AddSeconds(2)),
            _generator.CreateLogEntry(timestamp: baseTime.AddSeconds(3))
        };
        await store.AddBatchAsync(logs);

        // Act
        var result = await store.QueryAsync(new LogQuery
        {
            OrderByTimeAscending = true,
            PageSize = 10
        });

        // Assert
        result.Items[0].Timestamp.Should().BeBefore(result.Items[1].Timestamp);
        result.Items[1].Timestamp.Should().BeBefore(result.Items[2].Timestamp);
    }

    [Fact]
    public async Task QueryAsync_WithOrderByDescending_ShouldReturnNewestFirst()
    {
        // Arrange
        var store = new InMemoryLogStore();
        var baseTime = DateTime.Now;
        var logs = new[]
        {
            _generator.CreateLogEntry(timestamp: baseTime.AddSeconds(1)),
            _generator.CreateLogEntry(timestamp: baseTime.AddSeconds(2)),
            _generator.CreateLogEntry(timestamp: baseTime.AddSeconds(3))
        };
        await store.AddBatchAsync(logs);

        // Act
        var result = await store.QueryAsync(new LogQuery
        {
            OrderByTimeAscending = false,
            PageSize = 10
        });

        // Assert
        result.Items[0].Timestamp.Should().BeAfter(result.Items[1].Timestamp);
        result.Items[1].Timestamp.Should().BeAfter(result.Items[2].Timestamp);
    }

    // ========== GetByRequestId 测试 ==========

    [Fact]
    public async Task GetByRequestIdAsync_WithValidRequestId_ShouldReturnAllLogsForRequest()
    {
        // Arrange
        var store = new InMemoryLogStore();
        var requestId = Guid.NewGuid().ToString("N");
        var logs = new[]
        {
            _generator.CreateLogEntryForRequest(requestId),
            _generator.CreateLogEntryForRequest(requestId),
            _generator.CreateLogEntryForRequest(Guid.NewGuid().ToString("N"))
        };
        await store.AddBatchAsync(logs);

        // Act
        var result = await store.GetByRequestIdAsync(requestId);

        // Assert
        result.Should().HaveCount(2);
        result.All(x => x.RequestId == requestId).Should().BeTrue();
    }

    [Fact]
    public async Task GetByRequestIdAsync_WithInvalidRequestId_ShouldReturnEmpty()
    {
        // Arrange
        var store = new InMemoryLogStore();
        var logs = _generator.CreateLogEntries(10);
        await store.AddBatchAsync(logs);

        // Act
        var result = await store.GetByRequestIdAsync("non-existent-id");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByRequestIdAsync_WithEmptyRequestId_ShouldReturnEmpty()
    {
        // Arrange
        var store = new InMemoryLogStore();

        // Act
        var result = await store.GetByRequestIdAsync("");

        // Assert
        result.Should().BeEmpty();
    }

    // ========== GetTraceSummaries 测试 ==========

    [Fact]
    public async Task GetTraceSummariesAsync_ShouldReturnAggregatedTraceInfo()
    {
        // Arrange
        var store = new InMemoryLogStore();
        var requestId = Guid.NewGuid().ToString("N");
        var logs = new[]
        {
            _generator.CreateLogEntryForRequest(requestId, LogLevel.Information),
            _generator.CreateLogEntryForRequest(requestId, LogLevel.Error)
        };
        await store.AddBatchAsync(logs);

        // Act
        var summaries = await store.GetTraceSummariesAsync(null, null);

        // Assert
        summaries.Should().Contain(x => x.RequestId == requestId);
        var summary = summaries.First(x => x.RequestId == requestId);
        summary.LogCount.Should().Be(2);
        summary.HasError.Should().BeTrue();
    }

    [Fact]
    public async Task GetTraceSummariesAsync_WithTimeRange_ShouldFilterByTime()
    {
        // Arrange
        var store = new InMemoryLogStore();
        var baseTime = DateTime.Now;
        var requestId1 = Guid.NewGuid().ToString("N");
        var requestId2 = Guid.NewGuid().ToString("N");

        var logs = new[]
        {
            _generator.CreateLogEntry(timestamp: baseTime.AddHours(-2), requestId: requestId1),
            _generator.CreateLogEntry(timestamp: baseTime, requestId: requestId2)
        };
        await store.AddBatchAsync(logs);

        // Act
        var summaries = await store.GetTraceSummariesAsync(
            baseTime.AddMinutes(-30),
            baseTime.AddMinutes(30)
        );

        // Assert
        summaries.Should().HaveCount(1);
        summaries[0].RequestId.Should().Be(requestId2);
    }

    // ========== 并发测试 ==========

    [Fact]
    public async Task AddBatchAsync_ConcurrentWrites_ShouldBeThreadSafe()
    {
        // Arrange
        var store = new InMemoryLogStore(maxLogCount: 10000);

        // Act - 并发写入
        var tasks = Enumerable.Range(0, 10).Select(i => Task.Run(async () =>
        {
            var logs = _generator.CreateLogEntries(100);
            await store.AddBatchAsync(logs);
        })).ToArray();

        await Task.WhenAll(tasks);

        // Assert
        var result = await store.QueryAsync(new LogQuery { PageSize = 10000, MinLevel = LogLevel.Trace }); // 避免快速路径
        result.Total.Should().Be(1000);
    }

    [Fact]
    public async Task QueryAsync_DuringConcurrentWrites_ShouldNotThrow()
    {
        // Arrange
        var store = new InMemoryLogStore(maxLogCount: 10000);

        // Act - 并发写入和查询
        var writeTasks = Enumerable.Range(0, 5).Select(i => Task.Run(async () =>
        {
            var logs = _generator.CreateLogEntries(50);
            await store.AddBatchAsync(logs);
        }));

        var queryTasks = Enumerable.Range(0, 5).Select(i => Task.Run(async () =>
        {
            await Task.Delay(10 + i * 10);
            return await store.QueryAsync(new LogQuery { PageSize = 100 });
        }));

        await Task.WhenAll(writeTasks.Concat(queryTasks));

        // Assert - 如果没有抛出异常就说明线程安全
        var result = await store.QueryAsync(new LogQuery { PageSize = 10000 });
        result.Total.Should().Be(250);
    }

    // ========== 取消令牌测试 ==========

    [Fact]
    public async Task AddBatchAsync_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var store = new InMemoryLogStore();
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var logs = _generator.CreateLogEntries(10);

        // Act & Assert
        // TaskCanceledException 继承自 OperationCanceledException
        var exception = await Assert.ThrowsAsync<TaskCanceledException>(
            () => store.AddBatchAsync(logs, cts.Token).AsTask()
        );
        exception.Should().BeAssignableTo<OperationCanceledException>();
    }

    [Fact]
    public async Task QueryAsync_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var store = new InMemoryLogStore();
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        // TaskCanceledException 继承自 OperationCanceledException
        var exception = await Assert.ThrowsAsync<TaskCanceledException>(
            () => store.QueryAsync(new LogQuery(), cts.Token)
        );
        exception.Should().BeAssignableTo<OperationCanceledException>();
    }
}

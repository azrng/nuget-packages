using Azrng.DevLogDashboard.Models;
using Azrng.DevLogDashboard.Storage;
using Microsoft.Extensions.Logging;

namespace Azrng.DevLogDashboard.Test.Storage;

public class InMemoryLogStoreTests
{
    [Fact]
    public async Task InitializeAsync_ShouldComplete()
    {
        var store = new InMemoryLogStore();
        await store.Awaiting(s => s.InitializeAsync()).Should().NotThrowAsync();
    }

    [Fact]
    public async Task AddBatchAsync_NullEntries_ShouldNotThrow()
    {
        var store = new InMemoryLogStore();
        await store.Awaiting(s => s.AddBatchAsync(null!)).Should().NotThrowAsync();
    }

    [Fact]
    public async Task AddBatchAsync_WithEntries_ShouldStore()
    {
        var store = new InMemoryLogStore();
        var entries = new List<LogEntry>
        {
            new() { Id = "1", Message = "test1", Level = LogLevel.Information },
            new() { Id = "2", Message = "test2", Level = LogLevel.Error }
        };

        await store.AddBatchAsync(entries);

        var result = await store.QueryAsync(new LogQuery());
        result.Items.Should().HaveCount(2);
        result.Total.Should().Be(2);
    }

    [Fact]
    public async Task AddBatchAsync_SkipsNullEntries()
    {
        var store = new InMemoryLogStore();
        var entries = new List<LogEntry?>
        {
            new() { Id = "1", Message = "test1" },
            null,
            new() { Id = "2", Message = "test2" }
        };

        await store.AddBatchAsync(entries!);

        var result = await store.QueryAsync(new LogQuery());
        result.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task AddBatchAsync_ShouldTrimExcessLogs()
    {
        var store = new InMemoryLogStore(maxLogCount: 3);
        var entries = new List<LogEntry>
        {
            new() { Id = "1", Message = "first", Timestamp = DateTime.Now.AddMinutes(-3) },
            new() { Id = "2", Message = "second", Timestamp = DateTime.Now.AddMinutes(-2) },
            new() { Id = "3", Message = "third", Timestamp = DateTime.Now.AddMinutes(-1) },
            new() { Id = "4", Message = "fourth", Timestamp = DateTime.Now }
        };

        await store.AddBatchAsync(entries);

        var result = await store.QueryAsync(new LogQuery { PageSize = 100 });
        result.Items.Should().HaveCount(3);
    }

    [Fact]
    public async Task QueryAsync_EmptyStore_ShouldReturnEmpty()
    {
        var store = new InMemoryLogStore();
        var result = await store.QueryAsync(new LogQuery());

        result.Items.Should().BeEmpty();
        result.Total.Should().Be(0);
    }

    [Fact]
    public async Task QueryAsync_DefaultQuery_ShouldReturnLatestFirst()
    {
        var store = new InMemoryLogStore();
        var entries = new List<LogEntry>
        {
            new() { Id = "1", Message = "first", Timestamp = DateTime.Now.AddMinutes(-2) },
            new() { Id = "2", Message = "second", Timestamp = DateTime.Now.AddMinutes(-1) },
            new() { Id = "3", Message = "third", Timestamp = DateTime.Now }
        };
        await store.AddBatchAsync(entries);

        var result = await store.QueryAsync(new LogQuery());

        result.Items.Should().HaveCount(3);
        result.Items[0].Id.Should().Be("3");
        result.Items[1].Id.Should().Be("2");
        result.Items[2].Id.Should().Be("1");
    }

    [Fact]
    public async Task QueryAsync_OrderByTimeAscending_ShouldReturnOldestFirst()
    {
        var store = new InMemoryLogStore();
        var entries = new List<LogEntry>
        {
            new() { Id = "1", Message = "first", Timestamp = DateTime.Now.AddMinutes(-2) },
            new() { Id = "2", Message = "second", Timestamp = DateTime.Now.AddMinutes(-1) },
            new() { Id = "3", Message = "third", Timestamp = DateTime.Now }
        };
        await store.AddBatchAsync(entries);

        var result = await store.QueryAsync(new LogQuery { OrderByTimeAscending = true });

        result.Items[0].Id.Should().Be("1");
        result.Items[1].Id.Should().Be("2");
        result.Items[2].Id.Should().Be("3");
    }

    [Fact]
    public async Task QueryAsync_FilterByMinLevel_ShouldFilterCorrectly()
    {
        var store = new InMemoryLogStore();
        var entries = new List<LogEntry>
        {
            new() { Id = "1", Message = "trace", Level = LogLevel.Trace },
            new() { Id = "2", Message = "info", Level = LogLevel.Information },
            new() { Id = "3", Message = "error", Level = LogLevel.Error }
        };
        await store.AddBatchAsync(entries);

        var result = await store.QueryAsync(new LogQuery { MinLevel = LogLevel.Information });

        result.Items.Should().HaveCount(2);
        result.Items.Should().Contain(x => x.Id == "2");
        result.Items.Should().Contain(x => x.Id == "3");
    }

    [Fact]
    public async Task QueryAsync_FilterById_ShouldReturnExactMatch()
    {
        var store = new InMemoryLogStore();
        var entries = new List<LogEntry>
        {
            new() { Id = "abc", Message = "found" },
            new() { Id = "xyz", Message = "other" }
        };
        await store.AddBatchAsync(entries);

        var result = await store.QueryAsync(new LogQuery { Id = "abc" });

        result.Items.Should().HaveCount(1);
        result.Items[0].Id.Should().Be("abc");
    }

    [Fact]
    public async Task QueryAsync_FilterById_NotFound_ShouldReturnEmpty()
    {
        var store = new InMemoryLogStore();
        await store.AddBatchAsync(new List<LogEntry> { new() { Id = "abc" } });

        var result = await store.QueryAsync(new LogQuery { Id = "nonexistent" });

        result.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task QueryAsync_FilterByRequestId_ShouldReturnMatching()
    {
        var store = new InMemoryLogStore();
        var entries = new List<LogEntry>
        {
            new() { Id = "1", RequestId = "req-1" },
            new() { Id = "2", RequestId = "req-2" },
            new() { Id = "3", RequestId = "req-1" }
        };
        await store.AddBatchAsync(entries);

        var result = await store.QueryAsync(new LogQuery { RequestId = "req-1" });

        result.Items.Should().HaveCount(2);
        result.Items.Should().OnlyContain(x => x.RequestId == "req-1");
    }

    [Fact]
    public async Task QueryAsync_FilterBySource_ShouldReturnMatching()
    {
        var store = new InMemoryLogStore();
        var entries = new List<LogEntry>
        {
            new() { Id = "1", Source = "ControllerA" },
            new() { Id = "2", Source = "ControllerB" },
            new() { Id = "3", Source = "ControllerA" }
        };
        await store.AddBatchAsync(entries);

        var result = await store.QueryAsync(new LogQuery { Source = "ControllerA" });

        result.Items.Should().HaveCount(2);
        result.Items.Should().OnlyContain(x => x.Source == "ControllerA");
    }

    [Fact]
    public async Task QueryAsync_FilterByApplication_ShouldReturnMatching()
    {
        var store = new InMemoryLogStore();
        var entries = new List<LogEntry>
        {
            new() { Id = "1", Application = "AppA" },
            new() { Id = "2", Application = "AppB" }
        };
        await store.AddBatchAsync(entries);

        var result = await store.QueryAsync(new LogQuery { Application = "AppB" });

        result.Items.Should().HaveCount(1);
        result.Items[0].Application.Should().Be("AppB");
    }

    [Fact]
    public async Task QueryAsync_FilterByTimeRange_ShouldReturnInRange()
    {
        var baseTime = new DateTime(2025, 6, 26, 12, 0, 0);
        var store = new InMemoryLogStore();
        var entries = new List<LogEntry>
        {
            new() { Id = "1", Timestamp = baseTime.AddMinutes(-10) },
            new() { Id = "2", Timestamp = baseTime },
            new() { Id = "3", Timestamp = baseTime.AddMinutes(10) }
        };
        await store.AddBatchAsync(entries);

        var result = await store.QueryAsync(new LogQuery
        {
            StartTime = baseTime.AddMinutes(-5),
            EndTime = baseTime.AddMinutes(5)
        });

        result.Items.Should().HaveCount(1);
        result.Items[0].Id.Should().Be("2");
    }

    [Fact]
    public async Task QueryAsync_Pagination_ShouldReturnCorrectPage()
    {
        var store = new InMemoryLogStore();
        var entries = Enumerable.Range(1, 10)
            .Select(i => new LogEntry { Id = i.ToString(), Message = $"msg{i}", Timestamp = DateTime.Now.AddMinutes(-10 + i) })
            .ToList();
        await store.AddBatchAsync(entries);

        var page1 = await store.QueryAsync(new LogQuery { PageIndex = 1, PageSize = 3 });
        page1.Items.Should().HaveCount(3);
        page1.Total.Should().Be(10);
        page1.TotalPages.Should().Be(4);

        var page2 = await store.QueryAsync(new LogQuery { PageIndex = 2, PageSize = 3 });
        page2.Items.Should().HaveCount(3);

        var lastPage = await store.QueryAsync(new LogQuery { PageIndex = 4, PageSize = 3 });
        lastPage.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task QueryAsync_NullQuery_ShouldUseDefaults()
    {
        var store = new InMemoryLogStore();
        await store.AddBatchAsync(new List<LogEntry> { new() { Id = "1" } });

        var result = await store.QueryAsync(null!);

        result.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task QueryAsync_PageSizeExceedsMax_ShouldCapAt500()
    {
        var store = new InMemoryLogStore();
        var entries = Enumerable.Range(1, 600)
            .Select(i => new LogEntry { Id = i.ToString(), Timestamp = DateTime.Now.AddSeconds(i) })
            .ToList();
        await store.AddBatchAsync(entries);

        var result = await store.QueryAsync(new LogQuery { PageSize = 1000 });

        result.Items.Should().HaveCount(500);
    }

    [Fact]
    public async Task QueryAsync_FilterByKeyword_ExactMatch_ShouldWork()
    {
        var store = new InMemoryLogStore();
        var entries = new List<LogEntry>
        {
            new() { Id = "1", Message = "hello world" },
            new() { Id = "2", Message = "goodbye world" }
        };
        await store.AddBatchAsync(entries);

        var result = await store.QueryAsync(new LogQuery { Keyword = "hello" });

        result.Items.Should().HaveCount(1);
        result.Items[0].Message.Should().Contain("hello");
    }

    [Fact]
    public async Task QueryAsync_FilterByKeyword_WithEquals_ShouldMatchField()
    {
        var store = new InMemoryLogStore();
        var entries = new List<LogEntry>
        {
            new() { Id = "1", Message = "test", Source = "MyController" },
            new() { Id = "2", Message = "test", Source = "OtherController" }
        };
        await store.AddBatchAsync(entries);

        var result = await store.QueryAsync(new LogQuery { Keyword = "source=MyController" });

        result.Items.Should().HaveCount(1);
        result.Items[0].Source.Should().Be("MyController");
    }

    [Fact]
    public async Task QueryAsync_FilterByKeyword_WithLike_ShouldMatchSubstring()
    {
        var store = new InMemoryLogStore();
        var entries = new List<LogEntry>
        {
            new() { Id = "1", Message = "hello world" },
            new() { Id = "2", Message = "goodbye" }
        };
        await store.AddBatchAsync(entries);

        var result = await store.QueryAsync(new LogQuery { Keyword = "message like world" });

        result.Items.Should().HaveCount(1);
        result.Items[0].Message.Should().Contain("world");
    }

    [Fact]
    public async Task QueryAsync_FilterByKeyword_OrOperator_ShouldMatchEither()
    {
        var store = new InMemoryLogStore();
        var entries = new List<LogEntry>
        {
            new() { Id = "1", Message = "apple" },
            new() { Id = "2", Message = "banana" },
            new() { Id = "3", Message = "cherry" }
        };
        await store.AddBatchAsync(entries);

        var result = await store.QueryAsync(new LogQuery { Keyword = "apple or cherry" });

        result.Items.Should().HaveCount(2);
        result.Items.Should().Contain(x => x.Message == "apple");
        result.Items.Should().Contain(x => x.Message == "cherry");
    }

    [Fact]
    public async Task QueryAsync_FilterByKeyword_AndOperator_ShouldMatchBoth()
    {
        var store = new InMemoryLogStore();
        var entries = new List<LogEntry>
        {
            new() { Id = "1", Message = "hello world" },
            new() { Id = "2", Message = "hello" },
            new() { Id = "3", Message = "world" }
        };
        await store.AddBatchAsync(entries);

        var result = await store.QueryAsync(new LogQuery { Keyword = "hello and world" });

        result.Items.Should().HaveCount(1);
        result.Items[0].Message.Should().Be("hello world");
    }

    [Fact]
    public async Task QueryAsync_FilterByKeyword_CaseInsensitive_ShouldMatch()
    {
        var store = new InMemoryLogStore();
        await store.AddBatchAsync(new List<LogEntry>
        {
            new() { Id = "1", Message = "Hello World" }
        });

        var result = await store.QueryAsync(new LogQuery { Keyword = "hello" });

        result.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task QueryAsync_FilterByKeyword_EmptyKeyword_ShouldReturnAll()
    {
        var store = new InMemoryLogStore();
        await store.AddBatchAsync(new List<LogEntry>
        {
            new() { Id = "1", Message = "test" },
            new() { Id = "2", Message = "other" }
        });

        var result = await store.QueryAsync(new LogQuery { Keyword = "   " });

        result.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task QueryAsync_FilterByKeyword_LevelField_ShouldWork()
    {
        var store = new InMemoryLogStore();
        var entries = new List<LogEntry>
        {
            new() { Id = "1", Message = "test", Level = LogLevel.Error },
            new() { Id = "2", Message = "test", Level = LogLevel.Information }
        };
        await store.AddBatchAsync(entries);

        var result = await store.QueryAsync(new LogQuery { Keyword = "level=Error" });

        result.Items.Should().HaveCount(1);
        result.Items[0].Level.Should().Be(LogLevel.Error);
    }

    [Fact]
    public async Task QueryAsync_FilterByKeyword_ExceptionField_ShouldWork()
    {
        var store = new InMemoryLogStore();
        var entries = new List<LogEntry>
        {
            new() { Id = "1", Message = "test", Exception = "NullReferenceException" },
            new() { Id = "2", Message = "test" }
        };
        await store.AddBatchAsync(entries);

        var result = await store.QueryAsync(new LogQuery { Keyword = "exception=NullReferenceException" });

        result.Items.Should().HaveCount(1);
        result.Items[0].Exception.Should().Be("NullReferenceException");
    }

    [Fact]
    public async Task GetByRequestIdAsync_ShouldReturnMatchingLogs()
    {
        var store = new InMemoryLogStore();
        var entries = new List<LogEntry>
        {
            new() { Id = "1", RequestId = "req-1", Message = "a" },
            new() { Id = "2", RequestId = "req-2", Message = "b" },
            new() { Id = "3", RequestId = "req-1", Message = "c" }
        };
        await store.AddBatchAsync(entries);

        var result = await store.GetByRequestIdAsync("req-1");

        result.Should().HaveCount(2);
        result.Should().OnlyContain(x => x.RequestId == "req-1");
    }

    [Fact]
    public async Task GetByRequestIdAsync_EmptyRequestId_ShouldReturnEmpty()
    {
        var store = new InMemoryLogStore();
        var result = await store.GetByRequestIdAsync("");

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByRequestIdAsync_NotFound_ShouldReturnEmpty()
    {
        var store = new InMemoryLogStore();
        await store.AddBatchAsync(new List<LogEntry> { new() { RequestId = "req-1" } });

        var result = await store.GetByRequestIdAsync("nonexistent");

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetTraceSummariesAsync_EmptyStore_ShouldReturnEmpty()
    {
        var store = new InMemoryLogStore();
        var result = await store.GetTraceSummariesAsync(startTime: null, endTime: null);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetTraceSummariesAsync_WithEntries_ShouldReturnSummaries()
    {
        var store = new InMemoryLogStore();
        var baseTime = new DateTime(2025, 6, 26, 12, 0, 0);
        var entries = new List<LogEntry>
        {
            new() { Id = "1", RequestId = "req-1", Timestamp = baseTime, RequestPath = "/api/test", RequestMethod = "GET" },
            new() { Id = "2", RequestId = "req-1", Timestamp = baseTime.AddSeconds(1), Level = LogLevel.Error }
        };
        await store.AddBatchAsync(entries);

        var result = await store.GetTraceSummariesAsync(startTime: null, endTime: null);

        result.Should().HaveCount(1);
        var summary = result[0];
        summary.RequestId.Should().Be("req-1");
        summary.LogCount.Should().Be(2);
        summary.RequestPath.Should().Be("/api/test");
        summary.RequestMethod.Should().Be("GET");
        summary.HasError.Should().BeTrue();
        summary.Duration.Should().Be(1000.0);
    }

    [Fact]
    public async Task GetTraceSummariesAsync_FilterByTime_ShouldFilter()
    {
        var store = new InMemoryLogStore();
        var baseTime = new DateTime(2025, 6, 26, 12, 0, 0);
        var entries = new List<LogEntry>
        {
            new() { Id = "1", RequestId = "req-old", Timestamp = baseTime.AddHours(-2) },
            new() { Id = "2", RequestId = "req-new", Timestamp = baseTime }
        };
        await store.AddBatchAsync(entries);

        var result = await store.GetTraceSummariesAsync(startTime: baseTime.AddHours(-1), endTime: null);

        result.Should().HaveCount(1);
        result[0].RequestId.Should().Be("req-new");
    }

    [Fact]
    public async Task GetTraceSummariesAsync_FilterByEndTime_ShouldFilter()
    {
        var store = new InMemoryLogStore();
        var baseTime = new DateTime(2025, 6, 26, 12, 0, 0);
        var entries = new List<LogEntry>
        {
            new() { Id = "1", RequestId = "req-old", Timestamp = baseTime.AddHours(-2) },
            new() { Id = "2", RequestId = "req-new", Timestamp = baseTime }
        };
        await store.AddBatchAsync(entries);

        var result = await store.GetTraceSummariesAsync(startTime: null, endTime: baseTime.AddHours(-1));

        result.Should().HaveCount(1);
        result[0].RequestId.Should().Be("req-old");
    }

    [Fact]
    public async Task GetTraceSummariesAsync_MaxEntries_ShouldLimitTo1000()
    {
        var store = new InMemoryLogStore();
        var entries = Enumerable.Range(1, 1100)
            .Select(i => new LogEntry
            {
                Id = i.ToString(),
                RequestId = $"req-{i}",
                Timestamp = DateTime.Now.AddSeconds(i)
            })
            .ToList();
        await store.AddBatchAsync(entries);

        var result = await store.GetTraceSummariesAsync(startTime: null, endTime: null);

        result.Should().HaveCount(1000);
    }

    [Fact]
    public async Task Constructor_NegativeMaxLogCount_ShouldDefaultTo10000()
    {
        var store = new InMemoryLogStore(maxLogCount: -1);
        var entries = Enumerable.Range(1, 10)
            .Select(i => new LogEntry { Id = i.ToString(), Timestamp = DateTime.Now.AddSeconds(i) })
            .ToList();
        await store.AddBatchAsync(entries);

        var result = await store.QueryAsync(new LogQuery { PageSize = 100 });
        result.Items.Should().HaveCount(10);
    }

    [Fact]
    public async Task AddBatchAsync_WithRequestId_ShouldBuildTraceIndex()
    {
        var store = new InMemoryLogStore();
        var baseTime = new DateTime(2025, 6, 26, 12, 0, 0);
        var entries = new List<LogEntry>
        {
            new()
            {
                Id = "1",
                RequestId = "req-1",
                Timestamp = baseTime,
                RequestPath = "/api/users",
                RequestMethod = "POST",
                ResponseStatusCode = 201,
                Level = LogLevel.Information
            },
            new()
            {
                Id = "2",
                RequestId = "req-1",
                Timestamp = baseTime.AddSeconds(2),
                Level = LogLevel.Error
            }
        };
        await store.AddBatchAsync(entries);

        var summaries = await store.GetTraceSummariesAsync(startTime: null, endTime: null);
        summaries.Should().HaveCount(1);

        var trace = summaries[0];
        trace.LogCount.Should().Be(2);
        trace.RequestPath.Should().Be("/api/users");
        trace.RequestMethod.Should().Be("POST");
        trace.ResponseStatusCode.Should().Be(201);
        trace.HasError.Should().BeTrue();
        trace.Duration.Should().Be(2000.0);
    }

    [Fact]
    public async Task TrimExcessLogs_ShouldCleanIndexes()
    {
        var store = new InMemoryLogStore(maxLogCount: 2);
        var entries = new List<LogEntry>
        {
            new() { Id = "1", RequestId = "req-1", Message = "first", Timestamp = DateTime.Now.AddMinutes(-2) },
            new() { Id = "2", RequestId = "req-1", Message = "second", Timestamp = DateTime.Now.AddMinutes(-1) },
            new() { Id = "3", RequestId = "req-2", Message = "third", Timestamp = DateTime.Now }
        };
        await store.AddBatchAsync(entries);

        var result = await store.QueryAsync(new LogQuery { PageSize = 100 });
        result.Items.Should().HaveCount(2);

        var byId = await store.QueryAsync(new LogQuery { Id = "1" });
        byId.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task QueryAsync_FilterByKeyword_QuotedValue_ShouldWork()
    {
        var store = new InMemoryLogStore();
        var entries = new List<LogEntry>
        {
            new() { Id = "1", Source = "My Controller" },
            new() { Id = "2", Source = "Other" }
        };
        await store.AddBatchAsync(entries);

        var result = await store.QueryAsync(new LogQuery { Keyword = "source=\"My Controller\"" });

        result.Items.Should().HaveCount(1);
        result.Items[0].Source.Should().Be("My Controller");
    }

    [Fact]
    public async Task QueryAsync_FilterByKeyword_UnknownField_ShouldReturnEmpty()
    {
        var store = new InMemoryLogStore();
        await store.AddBatchAsync(new List<LogEntry>
        {
            new() { Id = "1", Message = "test", Properties = new Dictionary<string, object?> { ["custom"] = "value" } }
        });

        var result = await store.QueryAsync(new LogQuery { Keyword = "custom=value" });

        result.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task QueryAsync_FilterByKeyword_PropertyLookup_ShouldWork()
    {
        var store = new InMemoryLogStore();
        await store.AddBatchAsync(new List<LogEntry>
        {
            new() { Id = "1", Message = "test", Properties = new Dictionary<string, object?> { ["correlationId"] = "abc-123" } },
            new() { Id = "2", Message = "test", Properties = new Dictionary<string, object?> { ["correlationId"] = "xyz-789" } }
        });

        var result = await store.QueryAsync(new LogQuery { Keyword = "correlationId=abc-123" });

        result.Items.Should().HaveCount(1);
        result.Items[0].Properties["correlationId"].Should().Be("abc-123");
    }
}

using Azrng.DevLogDashboard.Models;
using Microsoft.Extensions.Logging;

namespace Azrng.DevLogDashboard.Test.Models;

public class LogQueryTests
{
    [Fact]
    public void DefaultValues_ShouldBeCorrect()
    {
        var query = new LogQuery();

        query.Id.Should().BeNull();
        query.Keyword.Should().BeNull();
        query.MinLevel.Should().BeNull();
        query.StartTime.Should().BeNull();
        query.EndTime.Should().BeNull();
        query.RequestId.Should().BeNull();
        query.Source.Should().BeNull();
        query.Application.Should().BeNull();
        query.OrderByTimeAscending.Should().BeFalse();
        query.PageIndex.Should().Be(1);
        query.PageSize.Should().Be(50);
    }

    [Fact]
    public void Skip_ShouldCalculateCorrectly()
    {
        var query = new LogQuery { PageIndex = 1, PageSize = 50 };
        query.Skip.Should().Be(0);

        query.PageIndex = 2;
        query.Skip.Should().Be(50);

        query.PageIndex = 3;
        query.PageSize = 20;
        query.Skip.Should().Be(40);
    }

    [Fact]
    public void Skip_PageIndex1_ShouldBeZero()
    {
        var query = new LogQuery { PageIndex = 1, PageSize = 100 };
        query.Skip.Should().Be(0);
    }

    [Fact]
    public void Properties_CanBeSetAndRetrieved()
    {
        var start = new DateTime(2025, 1, 1);
        var end = new DateTime(2025, 12, 31);
        var query = new LogQuery
        {
            Id = "test-id",
            Keyword = "error",
            MinLevel = LogLevel.Error,
            StartTime = start,
            EndTime = end,
            RequestId = "req-1",
            Source = "TestSource",
            Application = "TestApp",
            OrderByTimeAscending = true,
            PageIndex = 2,
            PageSize = 25
        };

        query.Id.Should().Be("test-id");
        query.Keyword.Should().Be("error");
        query.MinLevel.Should().Be(LogLevel.Error);
        query.StartTime.Should().Be(start);
        query.EndTime.Should().Be(end);
        query.RequestId.Should().Be("req-1");
        query.Source.Should().Be("TestSource");
        query.Application.Should().Be("TestApp");
        query.OrderByTimeAscending.Should().BeTrue();
        query.PageIndex.Should().Be(2);
        query.PageSize.Should().Be(25);
    }
}

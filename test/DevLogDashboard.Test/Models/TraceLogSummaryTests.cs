using Azrng.DevLogDashboard.Models;

namespace Azrng.DevLogDashboard.Test.Models;

public class TraceLogSummaryTests
{
    [Fact]
    public void DefaultValues_ShouldBeCorrect()
    {
        var summary = new TraceLogSummary();

        summary.RequestId.Should().BeEmpty();
        summary.LogCount.Should().Be(0);
        summary.FirstTimestamp.Should().Be(default);
        summary.LastTimestamp.Should().Be(default);
        summary.RequestPath.Should().BeNull();
        summary.RequestMethod.Should().BeNull();
        summary.ResponseStatusCode.Should().BeNull();
        summary.HasError.Should().BeFalse();
    }

    [Fact]
    public void Duration_ShouldCalculateCorrectly()
    {
        var summary = new TraceLogSummary
        {
            FirstTimestamp = new DateTime(2025, 1, 1, 0, 0, 0),
            LastTimestamp = new DateTime(2025, 1, 1, 0, 0, 1)
        };

        summary.Duration.Should().Be(1000.0);
    }

    [Fact]
    public void Duration_SameTimestamp_ShouldBeZero()
    {
        var ts = new DateTime(2025, 6, 26, 12, 0, 0);
        var summary = new TraceLogSummary
        {
            FirstTimestamp = ts,
            LastTimestamp = ts
        };

        summary.Duration.Should().Be(0.0);
    }

    [Fact]
    public void Properties_CanBeSetAndRetrieved()
    {
        var first = new DateTime(2025, 1, 1);
        var last = new DateTime(2025, 1, 2);
        var summary = new TraceLogSummary
        {
            RequestId = "req-123",
            LogCount = 5,
            FirstTimestamp = first,
            LastTimestamp = last,
            RequestPath = "/api/test",
            RequestMethod = "GET",
            ResponseStatusCode = 200,
            HasError = true
        };

        summary.RequestId.Should().Be("req-123");
        summary.LogCount.Should().Be(5);
        summary.FirstTimestamp.Should().Be(first);
        summary.LastTimestamp.Should().Be(last);
        summary.RequestPath.Should().Be("/api/test");
        summary.RequestMethod.Should().Be("GET");
        summary.ResponseStatusCode.Should().Be(200);
        summary.HasError.Should().BeTrue();
    }
}

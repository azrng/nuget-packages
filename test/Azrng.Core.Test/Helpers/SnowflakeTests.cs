using Azrng.Core.Helpers;
using FluentAssertions;
using Xunit;

namespace Azrng.Core.Test.Helpers;

public class SnowflakeTests
{
    [Fact]
    public void NewId_ShouldGenerateDifferentValues()
    {
        var id1 = Snowflake.NewId();
        var id2 = Snowflake.NewId();

        id2.Should().NotBe(id1);
    }

    [Fact]
    public void NewId_ShouldIncludeTimestampBits()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);

        var id = Snowflake.NewId();

        id.Should().BeGreaterOrEqualTo(1L << 22);
        Snowflake.TryParse(id, out var time, out _, out _).Should().BeTrue();
        time.Should().BeOnOrAfter(before);
        time.Should().BeOnOrBefore(DateTime.UtcNow.AddSeconds(1));
    }

    [Fact]
    public void TryParse_ShouldReturnReasonableParts()
    {
        var id = Snowflake.NewId();

        var result = Snowflake.TryParse(id, out var time, out var workerId, out var sequence);

        result.Should().BeTrue();
        time.Should().BeAfter(new DateTime(2018, 3, 15));
        workerId.Should().BeGreaterOrEqualTo(0);
        sequence.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public void TryParse_ShouldReturnFalseForInvalidIds()
    {
        var shortIdWithoutTimestamp = (930L << 12) | 1L;

        Snowflake.TryParse(0, out _, out _, out _).Should().BeFalse();
        Snowflake.TryParse(-1, out _, out _, out _).Should().BeFalse();
        Snowflake.TryParse(shortIdWithoutTimestamp, out _, out _, out _).Should().BeFalse();
    }

    [Fact]
    public void NewId_WithSpecifiedTime_ShouldRoundTripTimestamp()
    {
        var time = new DateTime(2024, 1, 1, 0, 0, 0, 123, DateTimeKind.Utc);

        var id = Snowflake.NewId(time);
        var result = Snowflake.TryParse(id, out var parsedTime, out _, out _);

        result.Should().BeTrue();
        parsedTime.Should().Be(time);
    }

    [Fact]
    public void NewId_WithSpecifiedTime_ShouldGenerateUniqueValuesConcurrently()
    {
        var time = new DateTime(2024, 1, 1, 0, 0, 0, 123, DateTimeKind.Utc);

        var ids = Enumerable.Range(0, 1000)
            .AsParallel()
            .Select(_ => Snowflake.NewId(time))
            .ToArray();

        ids.Distinct().Should().HaveCount(ids.Length);
        foreach (var id in ids)
        {
            Snowflake.TryParse(id, out var parsedTime, out _, out _).Should().BeTrue();
            parsedTime.Should().Be(time);
        }
    }

    [Fact]
    public void NewId_WithSpecifiedTime_ShouldRejectTimeBeforeStartTimestamp()
    {
        var time = new DateTime(2018, 3, 14, 23, 59, 59, DateTimeKind.Utc);

        var action = () => Snowflake.NewId(time);

        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void NewId_WithSpecifiedTime_ShouldRejectTimeOutsideTimestampRange()
    {
        var time = new DateTime(2090, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var action = () => Snowflake.NewId(time);

        action.Should().Throw<ArgumentOutOfRangeException>();
    }
}

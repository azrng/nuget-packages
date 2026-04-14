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
    public void TryParse_ShouldReturnReasonableParts()
    {
        var id = Snowflake.NewId();
        var snowflake = new Snowflake();

        var result = snowflake.TryParse(id, out var time, out var workerId, out var sequence);

        result.Should().BeTrue();
        time.Should().BeAfter(new DateTime(2018, 3, 15));
        workerId.Should().BeGreaterOrEqualTo(0);
        sequence.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public void GetId_ShouldBeLessThanOrEqualToFullGeneratedIdAtSameTime()
    {
        var snowflake = new Snowflake();
        var time = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var baseId = snowflake.GetId(time);
        var fullId = snowflake.NewId(time);

        fullId.Should().BeGreaterThanOrEqualTo(baseId);
    }
}

using Azrng.Core.CommonDto;
using FluentAssertions;
using Xunit;

namespace Common.Db.Core.Test.CommonDto;

public class RefAsyncTests
{
    [Fact]
    public void ImplicitConversions_ShouldRoundTripValue()
    {
        RefAsync<int> number = 42;

        int result = number;

        result.Should().Be(42);
        number.Value.Should().Be(42);
    }

    [Fact]
    public void ToString_ShouldReturnWrappedValueOrEmptyString()
    {
        var text = new RefAsync<string>("hello");
        var empty = new RefAsync<string?>();

        text.ToString().Should().Be("hello");
        empty.ToString().Should().BeEmpty();
    }
}

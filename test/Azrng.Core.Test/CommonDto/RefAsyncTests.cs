using Azrng.Core.CommonDto;
using FluentAssertions;
using Xunit;

namespace Azrng.Core.Test.CommonDto;

public class RefAsyncTests
{
    [Fact]
    public void ImplicitConversionFromValue_ShouldWrapValue()
    {
        RefAsync<int> value = 12;

        value.Value.Should().Be(12);
    }

    [Fact]
    public void ImplicitConversionToValue_ShouldUnwrapValue()
    {
        var wrapper = new RefAsync<string>("hello");

        string value = wrapper;

        value.Should().Be("hello");
    }

    [Fact]
    public void ToString_ShouldReturnEmptyString_WhenValueIsNull()
    {
        var wrapper = new RefAsync<string?>(null);

        wrapper.ToString().Should().BeEmpty();
    }

    [Fact]
    public void ToString_ShouldDelegateToUnderlyingValue()
    {
        var wrapper = new RefAsync<int>(123);

        wrapper.ToString().Should().Be("123");
    }
}

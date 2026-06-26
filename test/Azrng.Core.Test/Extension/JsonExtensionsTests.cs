using Azrng.Core.Extension;
using FluentAssertions;
using Xunit;

namespace Azrng.Core.Test.Extension;

public class JsonExtensionsTests
{
    #region IsJArrayString

    [Theory]
    [InlineData("[]", true)]
    [InlineData("[1,2,3]", true)]
    [InlineData("[{\"key\":\"value\"}]", true)]
    [InlineData(" [ ]", true)]
    [InlineData("  [{\"a\":1}]", true)]
    [InlineData("{\"key\":\"value\"}", false)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData(null, false)]
    [InlineData("abc", false)]
    [InlineData("null", false)]
    [InlineData("123", false)]
    public void IsJArrayString_ShouldReturnExpected(string? input, bool expected)
    {
        input.IsJArrayString().Should().Be(expected);
    }

    #endregion
}

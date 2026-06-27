using Azrng.Core.Helpers;
using FluentAssertions;
using Xunit;

namespace Azrng.Core.Test.Helpers;

public class NumberHelperTests
{
    [Theory]
    [InlineData(0, "0")]
    [InlineData(1, "1")]
    [InlineData(999, "999")]
    [InlineData(-1, "-1")]
    [InlineData(-999, "-999")]
    public void FormatLongNumber_Below1000_ReturnsPlainString(long input, string expected)
    {
        NumberHelper.FormatLongNumber(input).Should().Be(expected);
    }

    [Theory]
    [InlineData(1000, "0.1万")]
    [InlineData(10000, "1.0万")]
    [InlineData(99999999, "10000.0万")]
    [InlineData(50000, "5.0万")]
    [InlineData(12345678, "1234.6万")]
    public void FormatLongNumber_Between1000And1Yi_FormatsWithWan(long input, string expected)
    {
        NumberHelper.FormatLongNumber(input).Should().Be(expected);
    }

    [Theory]
    [InlineData(100000000, "1.0亿")]
    [InlineData(1000000000, "10.0亿")]
    [InlineData(999999999999, "10000.0亿")]
    [InlineData(500000000000, "5000.0亿")]
    public void FormatLongNumber_Between1YiAnd1WanYi_FormatsWithYi(long input, string expected)
    {
        NumberHelper.FormatLongNumber(input).Should().Be(expected);
    }

    [Theory]
    [InlineData(1000000000000, "1.0万亿")]
    [InlineData(10000000000000, "10.0万亿")]
    [InlineData(500000000000000, "500.0万亿")]
    public void FormatLongNumber_Above1WanYi_FormatsWithWanYi(long input, string expected)
    {
        NumberHelper.FormatLongNumber(input).Should().Be(expected);
    }

    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    public void ConvertToChinese_NullOrEmpty_ReturnsEmpty(string? input, string expected)
    {
        NumberHelper.ConvertToChinese(input!).Should().Be(expected);
    }

    [Theory]
    [InlineData("0", "零")]
    [InlineData("1", "一")]
    [InlineData("9", "九")]
    [InlineData("1234567890", "一二三四五六七八九零")]
    public void ConvertToChinese_Digits_ReturnsChineseMapping(string input, string expected)
    {
        NumberHelper.ConvertToChinese(input).Should().Be(expected);
    }

    [Fact]
    public void ConvertToChinese_WithSpaces_PreservesSpaces()
    {
        NumberHelper.ConvertToChinese("1 2 3").Should().Be("一 二 三");
    }

    [Fact]
    public void ConvertToChinese_WithNonDigitNonSpace_IgnoresThem()
    {
        NumberHelper.ConvertToChinese("1a2b3").Should().Be("一二三");
    }

    [Fact]
    public void ConvertToChinese_OnlyNonDigits_ReturnsEmpty()
    {
        NumberHelper.ConvertToChinese("abc").Should().BeEmpty();
    }

    [Fact]
    public void ConvertToChinese_LeadingTrailingSpaces_Trimmed()
    {
        NumberHelper.ConvertToChinese(" 123 ").Should().Be("一二三");
    }
}

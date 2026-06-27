using Azrng.Core.Extension;
using FluentAssertions;
using Xunit;

namespace Azrng.Core.Test.Extension;

public class DoubleExtensionTests
{
    [Theory]
    [InlineData(1.005, 2, "1")]          // banker's rounding: 5后皆零看奇偶, 0是偶数 -> 舍去; ToString无尾零
    [InlineData(1.004, 2, "1")]
    [InlineData(0, 2, "0")]
    [InlineData(-1.234, 2, "-1.23")]
    [InlineData(123.456, 1, "123.5")]
    [InlineData(1.2345, 3, "1.234")]     // banker's rounding: 5后皆零看奇偶, 4是偶数 -> 舍去
    [InlineData(1.555, 2, "1.56")]       // banker's rounding: 5后皆零看奇偶, 5是奇数 -> 进一
    [InlineData(1.545, 2, "1.54")]       // banker's rounding: 5后皆零看奇偶, 4是偶数 -> 舍去
    [InlineData(1.565, 2, "1.56")]       // banker's rounding: 5后皆零看奇偶, 6是偶数 -> 舍去
    public void ToStandardString_Double_ShouldRoundCorrectly(double input, int number, string expected)
    {
        input.ToStandardString(number).Should().Be(expected);
    }

    [Fact]
    public void ToStandardString_Double_DefaultPrecision_ShouldBe2()
    {
        1.235.ToStandardString().Should().Be("1.24"); // banker's rounding: 5 -> 3是奇数 -> 进一
    }

    [Fact]
    public void ToStandardString_Double_NegativeNumber_ShouldDefaultTo2()
    {
        1.235.ToStandardString(-1).Should().Be(1.235.ToStandardString(2));
    }

    [Fact]
    public void ToStandardString_Nullable_WithValue_ShouldBehaveLikeNonNullable()
    {
        ((double?)1.005).ToStandardString(2).Should().Be("1");
        ((double?)1.555).ToStandardString(2).Should().Be("1.56");
        ((double?)-1.234).ToStandardString(2).Should().Be("-1.23");
        ((double?)123.456).ToStandardString(1).Should().Be("123.5");
        ((double?)0).ToStandardString(2).Should().Be("0");
    }

    [Fact]
    public void ToStandardString_Nullable_Null_ShouldReturnZero()
    {
        ((double?)null).ToStandardString(2).Should().Be("0");
    }

    [Fact]
    public void ToStandardString_Nullable_DefaultPrecision_ShouldBe2()
    {
        ((double?)1.235).ToStandardString().Should().Be("1.24");
    }

    [Theory]
    [InlineData(1.10, 2, "1.1")]
    [InlineData(1.00, 2, "1")]
    [InlineData(1.123, 2, "1.12")]
    [InlineData(0, 2, "0")]
    [InlineData(-1.10, 2, "-1.1")]
    [InlineData(1.123, 3, "1.123")]
    [InlineData(1.1234, 3, "1.123")]
    public void ToNoZeroString_Double_ShouldTrimTrailingZeros(double input, int number, string expected)
    {
        input.ToNoZeroString(number).Should().Be(expected);
    }

    [Fact]
    public void ToNoZeroString_Double_DefaultPrecision_ShouldBe2()
    {
        1.10.ToNoZeroString().Should().Be("1.1");
    }

    [Fact]
    public void ToNoZeroString_Nullable_WithValue_ShouldBehaveLikeNonNullable()
    {
        ((double?)1.10).ToNoZeroString(2).Should().Be("1.1");
        ((double?)1.00).ToNoZeroString(2).Should().Be("1");
        ((double?)-1.10).ToNoZeroString(2).Should().Be("-1.1");
    }

    [Fact]
    public void ToNoZeroString_Nullable_Null_ShouldReturnZero()
    {
        ((double?)null).ToNoZeroString(2).Should().Be("0");
    }

    [Fact]
    public void ToNoZeroString_Nullable_DefaultPrecision_ShouldBe2()
    {
        ((double?)1.10).ToNoZeroString().Should().Be("1.1");
    }

    [Theory]
    [InlineData(1.5, 1.5)]
    [InlineData(-1.5, 1.5)]
    [InlineData(0, 0)]
    [InlineData(-100.25, 100.25)]
    [InlineData(100.25, 100.25)]
    public void ToAbs_ShouldReturnAbsoluteValue(double input, double expected)
    {
        input.ToAbs().Should().Be(expected);
    }

    [Theory]
    [InlineData(1.1, 2, "1.10")]
    [InlineData(1.0, 2, "1.00")]
    [InlineData(1.123, 2, "1.12")]
    [InlineData(0, 2, "0.00")]
    [InlineData(-1.1, 2, "-1.10")]
    [InlineData(1.126, 2, "1.13")]
    [InlineData(1.123, 3, "1.123")]
    [InlineData(1.1, 0, "1")]
    public void ToFixedString_Double_ShouldPreserveTrailingZeros(double input, int number, string expected)
    {
        input.ToFixedString(number).Should().Be(expected);
    }

    [Fact]
    public void ToFixedString_Double_DefaultPrecision_ShouldBe2()
    {
        1.1.ToFixedString().Should().Be("1.10");
    }

    [Fact]
    public void ToFixedString_Double_NegativeNumber_ShouldDefaultTo2()
    {
        1.1.ToFixedString(-1).Should().Be(1.1.ToFixedString(2));
    }

    [Fact]
    public void ToFixedString_Nullable_WithValue_ShouldBehaveLikeNonNullable()
    {
        ((double?)1.1).ToFixedString(2).Should().Be("1.10");
        ((double?)1.0).ToFixedString(2).Should().Be("1.00");
        ((double?)-1.1).ToFixedString(2).Should().Be("-1.10");
    }

    [Fact]
    public void ToFixedString_Nullable_Null_ShouldReturnZero()
    {
        ((double?)null).ToFixedString(2).Should().Be("0");
    }

    [Fact]
    public void ToFixedString_Nullable_DefaultPrecision_ShouldBe2()
    {
        ((double?)1.1).ToFixedString().Should().Be("1.10");
    }

    [Theory]
    [InlineData(0.1234, 2, "12.34%")]
    [InlineData(0.5, 2, "50.00%")]
    [InlineData(1, 2, "100.00%")]
    [InlineData(0, 2, "0.00%")]
    [InlineData(-0.1234, 2, "-12.34%")]
    [InlineData(0.1234, 1, "12.3%")]
    [InlineData(0.1234, 0, "12%")]
    public void ToPercentString_Double_ShouldMultiplyBy100AndAppendPercent(double input, int number, string expected)
    {
        input.ToPercentString(number).Should().Be(expected);
    }

    [Fact]
    public void ToPercentString_Double_DefaultPrecision_ShouldBe2()
    {
        0.5.ToPercentString().Should().Be("50.00%");
    }

    [Fact]
    public void ToPercentString_Double_NegativeNumber_ShouldDefaultTo2()
    {
        0.5.ToPercentString(-1).Should().Be(0.5.ToPercentString(2));
    }

    [Fact]
    public void ToPercentString_Nullable_WithValue_ShouldBehaveLikeNonNullable()
    {
        ((double?)0.1234).ToPercentString(2).Should().Be("12.34%");
        ((double?)0.5).ToPercentString(2).Should().Be("50.00%");
        ((double?)-0.1234).ToPercentString(2).Should().Be("-12.34%");
    }

    [Fact]
    public void ToPercentString_Nullable_Null_ShouldReturnZeroPercent()
    {
        ((double?)null).ToPercentString(2).Should().Be("0%");
    }

    [Fact]
    public void ToPercentString_Nullable_DefaultPrecision_ShouldBe2()
    {
        ((double?)0.5).ToPercentString().Should().Be("50.00%");
    }

    [Theory]
    [InlineData(1.999, 2, "1.99")]
    [InlineData(1.126, 2, "1.12")]
    [InlineData(1.124, 2, "1.12")]
    [InlineData(0, 2, "0.00")]
    [InlineData(-1.999, 2, "-1.99")]
    [InlineData(1.1234, 3, "1.123")]
    [InlineData(1.999, 0, "1")]
    public void ToTruncateString_Double_ShouldTruncateWithoutRounding(double input, int number, string expected)
    {
        input.ToTruncateString(number).Should().Be(expected);
    }

    [Fact]
    public void ToTruncateString_Double_DefaultPrecision_ShouldBe2()
    {
        1.999.ToTruncateString().Should().Be("1.99");
    }

    [Fact]
    public void ToTruncateString_Double_NegativeNumber_ShouldDefaultTo2()
    {
        1.999.ToTruncateString(-1).Should().Be(1.999.ToTruncateString(2));
    }

    [Fact]
    public void ToTruncateString_Nullable_WithValue_ShouldBehaveLikeNonNullable()
    {
        ((double?)1.999).ToTruncateString(2).Should().Be("1.99");
        ((double?)0).ToTruncateString(2).Should().Be("0.00");
        ((double?)-1.999).ToTruncateString(2).Should().Be("-1.99");
    }

    [Fact]
    public void ToTruncateString_Nullable_Null_ShouldReturnZero()
    {
        ((double?)null).ToTruncateString(2).Should().Be("0");
    }

    [Fact]
    public void ToTruncateString_Nullable_DefaultPrecision_ShouldBe2()
    {
        ((double?)1.999).ToTruncateString().Should().Be("1.99");
    }
}

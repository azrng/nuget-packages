using Azrng.Core.Extension;
using FluentAssertions;
using Xunit;

namespace Azrng.Core.Test.Extension;

public class DecimalExtensionTests
{
    [Theory]
    [InlineData(1.005, 2, "1.00")]      // banker's rounding: 5后皆零看奇偶, 0是偶数 -> 舍去; ToString保留小数位
    [InlineData(1.004, 2, "1.00")]
    [InlineData(0, 2, "0")]
    [InlineData(-1.234, 2, "-1.23")]
    [InlineData(123.456, 1, "123.5")]
    [InlineData(1.2345, 3, "1.234")]    // banker's rounding: 5后皆零看奇偶, 4是偶数 -> 舍去
    [InlineData(1.555, 2, "1.56")]      // banker's rounding: 5后皆零看奇偶, 5是奇数 -> 进一
    [InlineData(1.545, 2, "1.54")]      // banker's rounding: 5后皆零看奇偶, 4是偶数 -> 舍去
    [InlineData(1.565, 2, "1.56")]      // banker's rounding: 5后皆零看奇偶, 6是偶数 -> 舍去
    public void ToStandardString_Decimal_ShouldRoundCorrectly(decimal input, int number, string expected)
    {
        input.ToStandardString(number).Should().Be(expected);
    }

    [Fact]
    public void ToStandardString_Decimal_DefaultPrecision_ShouldBe2()
    {
        1.235m.ToStandardString().Should().Be("1.24"); // banker's rounding: 5 -> 3是奇数 -> 进一
    }

    [Fact]
    public void ToStandardString_Decimal_NegativeNumber_ShouldDefaultTo2()
    {
        1.235m.ToStandardString(-1).Should().Be(1.235m.ToStandardString(2));
    }

    [Fact]
    public void ToStandardString_Nullable_WithValue_ShouldBehaveLikeNonNullable()
    {
        ((decimal?)1.005).ToStandardString(2).Should().Be("1.00");
        ((decimal?)1.555).ToStandardString(2).Should().Be("1.56");
        ((decimal?)-1.234).ToStandardString(2).Should().Be("-1.23");
        ((decimal?)123.456).ToStandardString(1).Should().Be("123.5");
        ((decimal?)0).ToStandardString(2).Should().Be("0");
    }

    [Fact]
    public void ToStandardString_Nullable_Null_ShouldReturnZero()
    {
        ((decimal?)null).ToStandardString(2).Should().Be("0");
    }

    [Fact]
    public void ToStandardString_Nullable_DefaultPrecision_ShouldBe2()
    {
        ((decimal?)1.235).ToStandardString().Should().Be("1.24");
    }

    [Theory]
    [InlineData(1.10, 2, "1.1")]
    [InlineData(1.00, 2, "1")]
    [InlineData(1.123, 2, "1.12")]
    [InlineData(0, 2, "0")]
    [InlineData(-1.10, 2, "-1.1")]
    [InlineData(1.123, 3, "1.123")]
    [InlineData(1.1234, 3, "1.123")]
    public void ToNoZeroString_Decimal_ShouldTrimTrailingZeros(decimal input, int number, string expected)
    {
        input.ToNoZeroString(number).Should().Be(expected);
    }

    [Fact]
    public void ToNoZeroString_Decimal_DefaultPrecision_ShouldBe2()
    {
        1.10m.ToNoZeroString().Should().Be("1.1");
    }

    [Fact]
    public void ToNoZeroString_Nullable_WithValue_ShouldBehaveLikeNonNullable()
    {
        ((decimal?)1.10).ToNoZeroString(2).Should().Be("1.1");
        ((decimal?)1.00).ToNoZeroString(2).Should().Be("1");
        ((decimal?)-1.10).ToNoZeroString(2).Should().Be("-1.1");
    }

    [Fact]
    public void ToNoZeroString_Nullable_Null_ShouldReturnZero()
    {
        ((decimal?)null).ToNoZeroString(2).Should().Be("0");
    }

    [Fact]
    public void ToNoZeroString_Nullable_DefaultPrecision_ShouldBe2()
    {
        ((decimal?)1.10).ToNoZeroString().Should().Be("1.1");
    }

    [Theory]
    [InlineData(1.5, 1.5)]
    [InlineData(-1.5, 1.5)]
    [InlineData(0, 0)]
    [InlineData(-100.25, 100.25)]
    [InlineData(100.25, 100.25)]
    public void ToAbs_ShouldReturnAbsoluteValue(decimal input, decimal expected)
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
    public void ToFixedString_Decimal_ShouldPreserveTrailingZeros(decimal input, int number, string expected)
    {
        input.ToFixedString(number).Should().Be(expected);
    }

    [Fact]
    public void ToFixedString_Decimal_DefaultPrecision_ShouldBe2()
    {
        1.1m.ToFixedString().Should().Be("1.10");
    }

    [Fact]
    public void ToFixedString_Decimal_NegativeNumber_ShouldDefaultTo2()
    {
        1.1m.ToFixedString(-1).Should().Be(1.1m.ToFixedString(2));
    }

    [Fact]
    public void ToFixedString_Nullable_WithValue_ShouldBehaveLikeNonNullable()
    {
        ((decimal?)1.1).ToFixedString(2).Should().Be("1.10");
        ((decimal?)1.0).ToFixedString(2).Should().Be("1.00");
        ((decimal?)-1.1).ToFixedString(2).Should().Be("-1.10");
    }

    [Fact]
    public void ToFixedString_Nullable_Null_ShouldReturnZero()
    {
        ((decimal?)null).ToFixedString(2).Should().Be("0");
    }

    [Fact]
    public void ToFixedString_Nullable_DefaultPrecision_ShouldBe2()
    {
        ((decimal?)1.1).ToFixedString().Should().Be("1.10");
    }

    [Theory]
    [InlineData(0.1234, 2, "12.34%")]
    [InlineData(0.5, 2, "50.00%")]
    [InlineData(1, 2, "100.00%")]
    [InlineData(0, 2, "0.00%")]
    [InlineData(-0.1234, 2, "-12.34%")]
    [InlineData(0.1234, 1, "12.3%")]
    [InlineData(0.1234, 0, "12%")]
    public void ToPercentString_Decimal_ShouldMultiplyBy100AndAppendPercent(decimal input, int number, string expected)
    {
        input.ToPercentString(number).Should().Be(expected);
    }

    [Fact]
    public void ToPercentString_Decimal_DefaultPrecision_ShouldBe2()
    {
        0.5m.ToPercentString().Should().Be("50.00%");
    }

    [Fact]
    public void ToPercentString_Decimal_NegativeNumber_ShouldDefaultTo2()
    {
        0.5m.ToPercentString(-1).Should().Be(0.5m.ToPercentString(2));
    }

    [Fact]
    public void ToPercentString_Nullable_WithValue_ShouldBehaveLikeNonNullable()
    {
        ((decimal?)0.1234).ToPercentString(2).Should().Be("12.34%");
        ((decimal?)0.5).ToPercentString(2).Should().Be("50.00%");
        ((decimal?)-0.1234).ToPercentString(2).Should().Be("-12.34%");
    }

    [Fact]
    public void ToPercentString_Nullable_Null_ShouldReturnZeroPercent()
    {
        ((decimal?)null).ToPercentString(2).Should().Be("0%");
    }

    [Fact]
    public void ToPercentString_Nullable_DefaultPrecision_ShouldBe2()
    {
        ((decimal?)0.5).ToPercentString().Should().Be("50.00%");
    }

    [Theory]
    [InlineData(1.999, 2, "1.99")]
    [InlineData(1.126, 2, "1.12")]
    [InlineData(1.124, 2, "1.12")]
    [InlineData(0, 2, "0.00")]
    [InlineData(-1.999, 2, "-1.99")]
    [InlineData(1.1234, 3, "1.123")]
    [InlineData(1.999, 0, "1")]
    public void ToTruncateString_Decimal_ShouldTruncateWithoutRounding(decimal input, int number, string expected)
    {
        input.ToTruncateString(number).Should().Be(expected);
    }

    [Fact]
    public void ToTruncateString_Decimal_DefaultPrecision_ShouldBe2()
    {
        1.999m.ToTruncateString().Should().Be("1.99");
    }

    [Fact]
    public void ToTruncateString_Decimal_NegativeNumber_ShouldDefaultTo2()
    {
        1.999m.ToTruncateString(-1).Should().Be(1.999m.ToTruncateString(2));
    }

    [Fact]
    public void ToTruncateString_Nullable_WithValue_ShouldBehaveLikeNonNullable()
    {
        ((decimal?)1.999).ToTruncateString(2).Should().Be("1.99");
        ((decimal?)0).ToTruncateString(2).Should().Be("0.00");
        ((decimal?)-1.999).ToTruncateString(2).Should().Be("-1.99");
    }

    [Fact]
    public void ToTruncateString_Nullable_Null_ShouldReturnZero()
    {
        ((decimal?)null).ToTruncateString(2).Should().Be("0");
    }

    [Fact]
    public void ToTruncateString_Nullable_DefaultPrecision_ShouldBe2()
    {
        ((decimal?)1.999).ToTruncateString().Should().Be("1.99");
    }
}

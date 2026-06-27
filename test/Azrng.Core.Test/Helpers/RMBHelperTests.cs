using Azrng.Core.Helpers;
using FluentAssertions;
using Xunit;

namespace Azrng.Core.Test.Helpers;

public class RMBHelperTests
{
    [Fact]
    public void ToRmbUpper_Zero_ReturnsZeroYuanZheng()
    {
        RmbHelper.ToRmbUpper(0m).Should().Be("零元整");
    }

    [Theory]
    [InlineData(0.01, "壹分")]
    [InlineData(0.10, "壹角整")]
    [InlineData(0.11, "壹角壹分")]
    [InlineData(0.99, "玖角玖分")]
    public void ToRmbUpper_LessThanOneYuan_ReturnsCorrectFenJiao(decimal input, string expected)
    {
        RmbHelper.ToRmbUpper(input).Should().Be(expected);
    }

    [Theory]
    [InlineData(1, "壹元整")]
    [InlineData(10, "壹拾元整")]
    [InlineData(100, "壹佰元整")]
    [InlineData(1000, "壹仟元整")]
    [InlineData(10000, "壹万元整")]
    public void ToRmbUpper_WholeYuan_ReturnsZheng(decimal input, string expected)
    {
        RmbHelper.ToRmbUpper(input).Should().Be(expected);
    }

    [Theory]
    [InlineData(1.23, "壹元贰角叁分")]
    [InlineData(12.34, "壹拾贰元叁角肆分")]
    [InlineData(123.45, "壹佰贰拾叁元肆角伍分")]
    [InlineData(1234.56, "壹仟贰佰叁拾肆元伍角陆分")]
    public void ToRmbUpper_YuanWithJiaoFen_ReturnsCorrect(decimal input, string expected)
    {
        RmbHelper.ToRmbUpper(input).Should().Be(expected);
    }

    [Theory]
    [InlineData(10000, "壹万元整")]
    [InlineData(10001, "壹万零壹元整")]
    [InlineData(10010, "壹万零壹拾元整")]
    [InlineData(10100, "壹万零壹佰元整")]
    [InlineData(11000, "壹万壹仟元整")]
    [InlineData(12345, "壹万贰仟叁佰肆拾伍元整")]
    public void ToRmbUpper_WanLevel_ReturnsCorrect(decimal input, string expected)
    {
        RmbHelper.ToRmbUpper(input).Should().Be(expected);
    }

    [Theory]
    [InlineData(100000000, "壹亿元整")]
    [InlineData(100000001, "壹亿零壹元整")]
    [InlineData(123456789, "壹亿贰仟叁佰肆拾伍万陆仟柒佰捌拾玖元整")]
    public void ToRmbUpper_YiLevel_ReturnsCorrect(decimal input, string expected)
    {
        RmbHelper.ToRmbUpper(input).Should().Be(expected);
    }

    [Theory]
    [InlineData(1000000000000, "壹万亿元整")]
    [InlineData(1234567890123, "壹万贰仟叁佰肆拾伍亿陆仟柒佰捌拾玖万零壹佰贰拾叁元整")]
    public void ToRmbUpper_WanYiLevel_ReturnsCorrect(decimal input, string expected)
    {
        RmbHelper.ToRmbUpper(input).Should().Be(expected);
    }

    [Theory]
    [InlineData(-1, "壹元整")]
    [InlineData(-100.50, "壹佰元零伍角整")]
    public void ToRmbUpper_NegativeNumber_TakesAbsoluteValue(decimal input, string expected)
    {
        RmbHelper.ToRmbUpper(input).Should().Be(expected);
    }

    [Theory]
    [InlineData(1.255, "壹元贰角陆分")]
    [InlineData(1.254, "壹元贰角伍分")]
    public void ToRmbUpper_Rounding_RoundsToTwoDecimals(decimal input, string expected)
    {
        RmbHelper.ToRmbUpper(input).Should().Be(expected);
    }

    [Theory]
    [InlineData(10001.01, "壹万零壹元零壹分")]
    [InlineData(100000001.01, "壹亿零壹元零壹分")]
    public void ToRmbUpper_MiddleZeros_InsertsZeroPrefix(decimal input, string expected)
    {
        RmbHelper.ToRmbUpper(input).Should().Be(expected);
    }

    [Fact]
    public void ToRmbUpper_MaxValidValue_ReturnsCorrect()
    {
        var maxValid = 9999999999999.99m;
        var result = RmbHelper.ToRmbUpper(maxValid);
        result.Should().NotBe("溢出");
        result.Should().Contain("元");
    }

    [Fact]
    public void ToRmbUpper_OverflowValue_ReturnsOverflow()
    {
        var overflow = 100000000000000m;
        RmbHelper.ToRmbUpper(overflow).Should().Be("溢出");
    }

    [Fact]
    public void ToRmbUpper_StringValidDecimal_ReturnsCorrect()
    {
        RmbHelper.ToRmbUpper("123.45").Should().Be("壹佰贰拾叁元肆角伍分");
    }

    [Fact]
    public void ToRmbUpper_StringInteger_ReturnsCorrect()
    {
        RmbHelper.ToRmbUpper("100").Should().Be("壹佰元整");
    }

    [Fact]
    public void ToRmbUpper_StringZero_ReturnsZeroYuanZheng()
    {
        RmbHelper.ToRmbUpper("0").Should().Be("零元整");
    }

    [Fact]
    public void ToRmbUpper_StringWithLeadingTrailingSpaces_ParsesCorrectly()
    {
        RmbHelper.ToRmbUpper(" 100 ").Should().Be("壹佰元整");
    }

    [Fact]
    public void ToRmbUpper_StringNegative_ReturnsCorrect()
    {
        RmbHelper.ToRmbUpper("-50.25").Should().Be("伍拾元零贰角伍分");
    }

    [Fact]
    public void ToRmbUpper_StringInvalid_ThrowsArgumentException()
    {
        var act = () => RmbHelper.ToRmbUpper("abc");
        act.Should().Throw<ArgumentException>().WithMessage("参数无效");
    }

    [Fact]
    public void ToRmbUpper_StringEmpty_ThrowsArgumentException()
    {
        var act = () => RmbHelper.ToRmbUpper("");
        act.Should().Throw<ArgumentException>().WithMessage("参数无效");
    }

    [Fact]
    public void ToRmbUpper_StringNull_ThrowsArgumentException()
    {
        var act = () => RmbHelper.ToRmbUpper(null!);
        act.Should().Throw<ArgumentException>().WithMessage("参数无效");
    }

    [Theory]
    [InlineData(0.10, "壹角整")]
    [InlineData(0.20, "贰角整")]
    [InlineData(0.30, "叁角整")]
    [InlineData(0.40, "肆角整")]
    [InlineData(0.50, "伍角整")]
    [InlineData(0.60, "陆角整")]
    [InlineData(0.70, "柒角整")]
    [InlineData(0.80, "捌角整")]
    [InlineData(0.90, "玖角整")]
    public void ToRmbUpper_OnlyJiao_ReturnsCorrect(decimal input, string expected)
    {
        RmbHelper.ToRmbUpper(input).Should().Be(expected);
    }

    [Theory]
    [InlineData(0.01, "壹分")]
    [InlineData(0.02, "贰分")]
    [InlineData(0.03, "叁分")]
    [InlineData(0.04, "肆分")]
    [InlineData(0.05, "伍分")]
    [InlineData(0.06, "陆分")]
    [InlineData(0.07, "柒分")]
    [InlineData(0.08, "捌分")]
    [InlineData(0.09, "玖分")]
    public void ToRmbUpper_OnlyFen_ReturnsCorrect(decimal input, string expected)
    {
        RmbHelper.ToRmbUpper(input).Should().Be(expected);
    }
}

using Azrng.Core.Helpers;
using FluentAssertions;
using Xunit;

namespace Azrng.Core.Test.Helpers;

public class ChineseTextHelperTests
{
    #region IsChineseCharacter - 汉字判断

    [Theory]
    [InlineData('中')]
    [InlineData('国')]
    [InlineData('人')]
    [InlineData('你')]
    [InlineData('好')]
    public void IsChineseCharacter_ChineseChar_ShouldReturnTrue(char c)
    {
        var result = ChineseTextHelper.IsChineseCharacter(c);

        result.Should().BeTrue();
    }

    [Theory]
    [InlineData('a')]
    [InlineData('Z')]
    [InlineData('0')]
    [InlineData(' ')]
    [InlineData('!')]
    [InlineData('@')]
    public void IsChineseCharacter_NonChineseChar_ShouldReturnFalse(char c)
    {
        var result = ChineseTextHelper.IsChineseCharacter(c);

        result.Should().BeFalse();
    }

    [Fact]
    public void IsChineseCharacter_BoundaryLow_ShouldReturnTrue()
    {
        var result = ChineseTextHelper.IsChineseCharacter((char)0x4E00);

        result.Should().BeTrue();
    }

    [Fact]
    public void IsChineseCharacter_BoundaryHigh_ShouldReturnTrue()
    {
        var result = ChineseTextHelper.IsChineseCharacter((char)0x9FFF);

        result.Should().BeTrue();
    }

    [Fact]
    public void IsChineseCharacter_BelowBoundary_ShouldReturnFalse()
    {
        var result = ChineseTextHelper.IsChineseCharacter((char)0x4DFF);

        result.Should().BeFalse();
    }

    [Fact]
    public void IsChineseCharacter_AboveBoundary_ShouldReturnFalse()
    {
        var result = ChineseTextHelper.IsChineseCharacter((char)0xA000);

        result.Should().BeFalse();
    }

    #endregion

    #region ContainsChinese - 包含汉字检测

    [Fact]
    public void ContainsChinese_Null_ShouldReturnFalse()
    {
        var result = ChineseTextHelper.ContainsChinese(null);

        result.Should().BeFalse();
    }

    [Fact]
    public void ContainsChinese_Empty_ShouldReturnFalse()
    {
        var result = ChineseTextHelper.ContainsChinese(string.Empty);

        result.Should().BeFalse();
    }

    [Fact]
    public void ContainsChinese_ChineseText_ShouldReturnTrue()
    {
        var result = ChineseTextHelper.ContainsChinese("你好世界");

        result.Should().BeTrue();
    }

    [Fact]
    public void ContainsChinese_EnglishText_ShouldReturnFalse()
    {
        var result = ChineseTextHelper.ContainsChinese("Hello World");

        result.Should().BeFalse();
    }

    [Fact]
    public void ContainsChinese_MixedText_ShouldReturnTrue()
    {
        var result = ChineseTextHelper.ContainsChinese("Hello你好World");

        result.Should().BeTrue();
    }

    [Fact]
    public void ContainsChinese_OnlyNumbers_ShouldReturnFalse()
    {
        var result = ChineseTextHelper.ContainsChinese("12345");

        result.Should().BeFalse();
    }

    [Fact]
    public void ContainsChinese_SpecialChars_ShouldReturnFalse()
    {
        var result = ChineseTextHelper.ContainsChinese("!@#$%^&*()");

        result.Should().BeFalse();
    }

    [Fact]
    public void ContainsChinese_WhitespaceOnly_ShouldReturnFalse()
    {
        var result = ChineseTextHelper.ContainsChinese("   ");

        result.Should().BeFalse();
    }

    #endregion

    #region RemoveNonChineseFromStartAndEnd - 去除首尾非汉字

    [Fact]
    public void RemoveNonChineseFromStartAndEnd_Null_ShouldReturnEmpty()
    {
        var result = ChineseTextHelper.RemoveNonChineseFromStartAndEnd(null);

        result.Should().BeEmpty();
    }

    [Fact]
    public void RemoveNonChineseFromStartAndEnd_Empty_ShouldReturnEmpty()
    {
        var result = ChineseTextHelper.RemoveNonChineseFromStartAndEnd(string.Empty);

        result.Should().BeEmpty();
    }

    [Fact]
    public void RemoveNonChineseFromStartAndEnd_PureChinese_ShouldReturnSame()
    {
        var result = ChineseTextHelper.RemoveNonChineseFromStartAndEnd("你好世界");

        result.Should().Be("你好世界");
    }

    [Fact]
    public void RemoveNonChineseFromStartAndEnd_ChineseWithLeadingEnglish_ShouldRemoveLeading()
    {
        var result = ChineseTextHelper.RemoveNonChineseFromStartAndEnd("Hello你好世界");

        result.Should().Be("你好世界");
    }

    [Fact]
    public void RemoveNonChineseFromStartAndEnd_ChineseWithTrailingEnglish_ShouldRemoveTrailing()
    {
        var result = ChineseTextHelper.RemoveNonChineseFromStartAndEnd("你好世界Hello");

        result.Should().Be("你好世界");
    }

    [Fact]
    public void RemoveNonChineseFromStartAndEnd_ChineseWithBothSidesEnglish_ShouldRemoveBoth()
    {
        var result = ChineseTextHelper.RemoveNonChineseFromStartAndEnd("Hello你好世界World");

        result.Should().Be("你好世界");
    }

    [Fact]
    public void RemoveNonChineseFromStartAndEnd_ChineseWithNumbers_ShouldRemoveNumbers()
    {
        var result = ChineseTextHelper.RemoveNonChineseFromStartAndEnd("123你好世界456");

        result.Should().Be("你好世界");
    }

    [Fact]
    public void RemoveNonChineseFromStartAndEnd_ChineseWithSpaces_ShouldRemoveSpaces()
    {
        var result = ChineseTextHelper.RemoveNonChineseFromStartAndEnd("  你好世界  ");

        result.Should().Be("你好世界");
    }

    [Fact]
    public void RemoveNonChineseFromStartAndEnd_ChineseWithMixedNonChinese_ShouldRemoveAll()
    {
        var result = ChineseTextHelper.RemoveNonChineseFromStartAndEnd("AB12 你好世界 !@#");

        result.Should().Be("你好世界");
    }

    [Fact]
    public void RemoveNonChineseFromStartAndEnd_PureEnglish_ShouldReturnEmpty()
    {
        var result = ChineseTextHelper.RemoveNonChineseFromStartAndEnd("Hello World");

        result.Should().BeEmpty();
    }

    [Fact]
    public void RemoveNonChineseFromStartAndEnd_PureNumbers_ShouldReturnEmpty()
    {
        var result = ChineseTextHelper.RemoveNonChineseFromStartAndEnd("12345");

        result.Should().BeEmpty();
    }

    [Fact]
    public void RemoveNonChineseFromStartAndEnd_OnlySpaces_ShouldReturnEmpty()
    {
        var result = ChineseTextHelper.RemoveNonChineseFromStartAndEnd("   ");

        result.Should().BeEmpty();
    }

    [Fact]
    public void RemoveNonChineseFromStartAndEnd_SingleChineseChar_ShouldReturnSame()
    {
        var result = ChineseTextHelper.RemoveNonChineseFromStartAndEnd("中");

        result.Should().Be("中");
    }

    [Fact]
    public void RemoveNonChineseFromStartAndEnd_ChineseWithPunctuation_ShouldStripTrailingPunctuation()
    {
        var result = ChineseTextHelper.RemoveNonChineseFromStartAndEnd("你好，世界！");

        result.Should().Be("你好，世界");
    }

    #endregion
}

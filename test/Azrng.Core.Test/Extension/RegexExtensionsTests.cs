using System.Text.RegularExpressions;
using Azrng.Core.Extension;
using FluentAssertions;
using Xunit;

namespace Azrng.Core.Test.Extension;

public class RegexExtensionsTests
{
    #region ToRegex

    [Theory]
    [InlineData("abc", "abc")]
    [InlineData("a.b", @"a\.b")]
    [InlineData("a+b*c", @"a\+b\*c")]
    [InlineData("hello world", @"hello\ world")]
    [InlineData("test(1)", @"test\(1\)")]
    [InlineData("a\\b", @"a\\b")]
    [InlineData("123", "123")]
    [InlineData("a-b", @"a\-b")]
    public void ToRegex_VariousInputs_ShouldEscapeCorrectly(string input, string expected)
    {
        var result = input.ToRegex();

        result.Should().Be(expected);
    }

    #endregion

    #region IsMatch (string, string, RegexOptions)

    [Fact]
    public void IsMatch_WithRegexOptions_MatchingPattern_ShouldReturnTrue()
    {
        var result = "Hello World".IsMatch(@"hello world", RegexOptions.IgnoreCase);

        result.Should().BeTrue();
    }

    [Fact]
    public void IsMatch_WithRegexOptions_CaseSensitiveMismatch_ShouldReturnFalse()
    {
        var result = "Hello World".IsMatch(@"hello world", RegexOptions.None);

        result.Should().BeFalse();
    }

    [Fact]
    public void IsMatch_WithRegexOptions_MultilineMatch_ShouldReturnTrue()
    {
        var input = "line1\nline2";
        var result = input.IsMatch(@"^line2$", RegexOptions.Multiline);

        result.Should().BeTrue();
    }

    #endregion

    #region IsIdCard

    [Theory]
    [InlineData(null, true)]
    [InlineData("", true)]
    public void IsIdCard_NullOrEmpty_ShouldReturnTrue(string? input, bool expected)
    {
        var result = input.IsIdCard();

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("110101199003071234")]
    [InlineData("11010119900307123x")]
    [InlineData("440106199003071234")]
    public void IsIdCard_ValidIdCard_ShouldReturnTrue(string idCard)
    {
        var result = idCard.IsIdCard();

        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("000101199003071234")]
    [InlineData("990101199003071234")]
    [InlineData("11010119900307")]
    [InlineData("1101011990030712345")]
    [InlineData("11010119900307123X")]
    [InlineData("abcdefg")]
    public void IsIdCard_InvalidIdCard_ShouldReturnFalse(string idCard)
    {
        var result = idCard.IsIdCard();

        result.Should().BeFalse();
    }

    #endregion

    #region IsPhone

    [Theory]
    [InlineData(null, true)]
    [InlineData("", true)]
    public void IsPhone_NullOrEmpty_ShouldReturnTrue(string? input, bool expected)
    {
        var result = input.IsPhone();

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("13812345678")]
    [InlineData("15912345678")]
    [InlineData("18612345678")]
    [InlineData("17712345678")]
    [InlineData("19912345678")]
    public void IsPhone_ValidPhone_ShouldReturnTrue(string phone)
    {
        var result = phone.IsPhone();

        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("12345678901")]
    [InlineData("01381234567")]
    [InlineData("1381234567")]
    [InlineData("138123456789")]
    [InlineData("abcdefghijk")]
    [InlineData("23812345678")]
    public void IsPhone_InvalidPhone_ShouldReturnFalse(string phone)
    {
        var result = phone.IsPhone();

        result.Should().BeFalse();
    }

    #endregion

    #region IsMatch (string, string)

    [Fact]
    public void IsMatch_SimpleMatch_ShouldReturnTrue()
    {
        var result = "abc123".IsMatch(@"\d+");

        result.Should().BeTrue();
    }

    [Fact]
    public void IsMatch_SimpleNoMatch_ShouldReturnFalse()
    {
        var result = "abc".IsMatch(@"\d+");

        result.Should().BeFalse();
    }

    [Fact]
    public void IsMatch_CaseSensitiveByDefault()
    {
        var result = "Hello".IsMatch(@"hello");

        result.Should().BeFalse();
    }

    #endregion

    #region IsMatch (string, string, bool, bool)

    [Fact]
    public void IsMatch_IgnoreCaseTrue_ShouldMatchCaseInsensitive()
    {
        var result = "Hello".IsMatch(@"hello", true, false);

        result.Should().BeTrue();
    }

    [Fact]
    public void IsMatch_IgnoreCaseFalse_ShouldMatchCaseSensitive()
    {
        var result = "Hello".IsMatch(@"hello", false, false);

        result.Should().BeFalse();
    }

    [Fact]
    public void IsMatch_ValidateWhiteSpaceFalse_EmptyInput_ShouldReturnFalse()
    {
        var result = "".IsMatch(@"\d*", false, false);

        result.Should().BeFalse();
    }

    [Fact]
    public void IsMatch_ValidateWhiteSpaceTrue_EmptyInput_ShouldReturnTrue()
    {
        var result = "".IsMatch(@"^$", false, true);

        result.Should().BeTrue();
    }

    [Fact]
    public void IsMatch_ValidateWhiteSpaceFalse_WhiteSpaceInput_ShouldReturnFalse()
    {
        var result = "   ".IsMatch(@"\s+", false, false);

        result.Should().BeFalse();
    }

    [Fact]
    public void IsMatch_ValidateWhiteSpaceTrue_WhiteSpaceInput_ShouldReturnTrue()
    {
        var result = "   ".IsMatch(@"\s+", false, true);

        result.Should().BeTrue();
    }

    #endregion
}

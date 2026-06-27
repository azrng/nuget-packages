using System.Text;
using Azrng.Core.Extension;
using FluentAssertions;
using Xunit;

namespace Azrng.Core.Test.Extension;

public class UrlExtensionsTests
{
    #region UrlEncode(this string?)

    [Theory]
    [InlineData(null, null)]
    [InlineData("", "")]
    [InlineData("abc", "abc")]
    [InlineData("hello world", "hello+world")]
    [InlineData("a&b=c", "a%26b%3dc")]
    [InlineData("中文", "%e4%b8%ad%e6%96%87")]
    [InlineData("a+b", "a%2bb")]
    public void UrlEncode_ShouldReturnExpected(string? input, string? expected)
    {
        input.UrlEncode().Should().Be(expected);
    }

    #endregion

    #region UrlEncode(this string?, Encoding)

    [Theory]
    [InlineData(null, null)]
    [InlineData("", "")]
    [InlineData("abc", "abc")]
    [InlineData("hello world", "hello+world")]
    [InlineData("a&b=c", "a%26b%3dc")]
    public void UrlEncode_WithEncoding_ShouldReturnExpected(string? input, string? expected)
    {
        input.UrlEncode(Encoding.UTF8).Should().Be(expected);
    }

    [Fact]
    public void UrlEncode_WithAsciiEncoding_ShouldEncodeChineseDifferently()
    {
        var utf8Result = "中文".UrlEncode(Encoding.UTF8);
        var asciiResult = "中文".UrlEncode(Encoding.ASCII);

        utf8Result.Should().NotBe(asciiResult);
    }

    #endregion

    #region UrlDecode(this string?)

    [Theory]
    [InlineData(null, null)]
    [InlineData("", "")]
    [InlineData("abc", "abc")]
    [InlineData("hello+world", "hello world")]
    [InlineData("a%26b%3dc", "a&b=c")]
    [InlineData("%e4%b8%ad%e6%96%87", "中文")]
    public void UrlDecode_ShouldReturnExpected(string? input, string? expected)
    {
        input.UrlDecode().Should().Be(expected);
    }

    [Fact]
    public void UrlDecode_ShouldReverseUrlEncode()
    {
        var original = "hello world&a=b";
        var encoded = original.UrlEncode();
        encoded.UrlDecode().Should().Be(original);
    }

    #endregion

    #region UrlDecode(this string?, Encoding)

    [Theory]
    [InlineData(null, null)]
    [InlineData("", "")]
    [InlineData("abc", "abc")]
    [InlineData("hello+world", "hello world")]
    [InlineData("a%26b%3dc", "a&b=c")]
    public void UrlDecode_WithEncoding_ShouldReturnExpected(string? input, string? expected)
    {
        input.UrlDecode(Encoding.UTF8).Should().Be(expected);
    }

    [Fact]
    public void UrlDecode_WithEncoding_ShouldReverseUrlEncode()
    {
        var original = "测试数据";
        var encoded = original.UrlEncode(Encoding.UTF8);
        encoded.UrlDecode(Encoding.UTF8).Should().Be(original);
    }

    #endregion

    #region AttributeEncode

    [Theory]
    [InlineData("abc", "abc")]
    [InlineData("", "")]
    [InlineData("a\"b", "a&quot;b")]
    [InlineData("a&b", "a&amp;b")]
    [InlineData("<script>alert('xss')</script>", "&lt;script>alert(&#39;xss&#39;)&lt;/script>")]
    public void AttributeEncode_ShouldReturnExpected(string input, string expected)
    {
        input.AttributeEncode().Should().Be(expected);
    }

    #endregion

    #region HtmlEncode

    [Theory]
    [InlineData("abc", "abc")]
    [InlineData("", "")]
    [InlineData("<b>bold</b>", "&lt;b&gt;bold&lt;/b&gt;")]
    [InlineData("a&b", "a&amp;b")]
    [InlineData("\"quoted\"", "&quot;quoted&quot;")]
    public void HtmlEncode_ShouldReturnExpected(string input, string expected)
    {
        input.HtmlEncode().Should().Be(expected);
    }

    #endregion

    #region HtmlDecode

    [Theory]
    [InlineData("abc", "abc")]
    [InlineData("", "")]
    [InlineData("&lt;b&gt;bold&lt;/b&gt;", "<b>bold</b>")]
    [InlineData("a&amp;b", "a&b")]
    [InlineData("&quot;quoted&quot;", "\"quoted\"")]
    public void HtmlDecode_ShouldReturnExpected(string input, string expected)
    {
        input.HtmlDecode().Should().Be(expected);
    }

    [Fact]
    public void HtmlDecode_ShouldReverseHtmlEncode()
    {
        var original = "<div class=\"test\">Hello & Welcome</div>";
        var encoded = original.HtmlEncode();
        encoded.HtmlDecode().Should().Be(original);
    }

    #endregion
}

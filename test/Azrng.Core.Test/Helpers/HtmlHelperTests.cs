using Azrng.Core.Helpers;
using FluentAssertions;
using Xunit;

namespace Azrng.Core.Test.Helpers;

public class HtmlHelperTests
{
    [Fact]
    public void HtmlToText_ShouldReturnEmpty_WhenInputEmpty()
    {
        HtmlHelper.HtmlToText("").Should().Be("");
    }

    [Fact]
    public void HtmlToText_ShouldReturnPlainText_WhenNoHtmlTags()
    {
        HtmlHelper.HtmlToText("hello world").Should().Be("hello world");
    }

    [Fact]
    public void HtmlToText_ShouldRemoveSimpleTags()
    {
        HtmlHelper.HtmlToText("<p>hello</p>").Should().Be("hello");
    }

    [Fact]
    public void HtmlToText_ShouldRemoveNestedTags()
    {
        HtmlHelper.HtmlToText("<div><span>text</span></div>").Should().Be("text");
    }

    [Fact]
    public void HtmlToText_ShouldRemoveScriptTags()
    {
        HtmlHelper.HtmlToText("<script>alert('xss')</script>safe").Should().Be("safe");
    }

    [Fact]
    public void HtmlToText_ShouldRemoveStyleTags()
    {
        HtmlHelper.HtmlToText("<style>body{color:red}</style>content").Should().Be("content");
    }

    [Fact]
    public void HtmlToText_ShouldRemoveAttributes()
    {
        HtmlHelper.HtmlToText("<a href=\"http://example.com\">link</a>").Should().Be("link");
    }

    [Fact]
    public void HtmlToText_ShouldStripMultilineScriptAndStyleContent()
    {
        // 多行 script/style 块的 JS/CSS 正文不应残留在文本里（潜在信息泄漏）
        var html = "<script>\nvar secret = 1;\nalert(secret);\n</script>safe";
        HtmlHelper.HtmlToText(html).Should().Be("safe");

        var html2 = "<style>\nbody { color: red; }\n</style>content";
        HtmlHelper.HtmlToText(html2).Should().Be("content");
    }

    [Fact]
    public void HtmlToText_ShouldDecodeCommonEntities()
    {
        // 实体应还原为对应字符（原实现替换为空串，R&amp;D 会丢失 & ）
        HtmlHelper.HtmlToText("a&amp;b").Should().Be("a&b");
        HtmlHelper.HtmlToText("a&lt;b").Should().Be("a<b");
        HtmlHelper.HtmlToText("a&gt;b").Should().Be("a>b");
        HtmlHelper.HtmlToText("a&quot;b").Should().Be("a\"b");
        HtmlHelper.HtmlToText("a&nbsp;b").Should().Be("a\u00A0b");
    }

    [Fact]
    public void HtmlToText_ShouldDecodeNumericEntities()
    {
        HtmlHelper.HtmlToText("&#65;B").Should().Be("AB");
    }

    [Fact]
    public void HtmlToText_ShouldDecodeNamedEntitiesIexclCentPoundCopy()
    {
        HtmlHelper.HtmlToText("&iexcl;x").Should().Be("¡x");
        HtmlHelper.HtmlToText("&cent;x").Should().Be("¢x");
        HtmlHelper.HtmlToText("&pound;x").Should().Be("£x");
        HtmlHelper.HtmlToText("&copy;x").Should().Be("©x");
    }

    [Fact]
    public void HtmlToText_ShouldRemoveCommentWithTrailingNewline()
    {
        HtmlHelper.HtmlToText("<!-- comment\n-->visible").Should().Be("visible");
    }

    [Fact]
    public void HtmlToText_ShouldCollapseNewlinePrefixedWhitespace()
    {
        HtmlHelper.HtmlToText("hello\r\n   world").Should().Be("helloworld");
    }

    [Fact]
    public void HtmlToText_ShouldTrimResult()
    {
        HtmlHelper.HtmlToText("  <p>text</p>  ").Should().Be("text");
    }

    [Fact]
    public void HtmlToText_ShouldHandleComplexHtml()
    {
        var html = "<html><body><h1>Title</h1><p>Paragraph</p></body></html>";
        HtmlHelper.HtmlToText(html).Should().Be("TitleParagraph");
    }

    [Fact]
    public void HtmlToText_ShouldHandleSelfClosingTags()
    {
        HtmlHelper.HtmlToText("line1<br/>line2").Should().Be("line1line2");
    }
}

using Azrng.NTika.Core.Model;
using Azrng.NTika.Core.Sax;
using FluentAssertions;
using Xunit;

namespace Azrng.NTika.Core.Test
{
    public class ToMarkdownContentHandlerTests
    {
        [Fact]
        public void Heading_ShouldOutputHashPrefix()
        {
            var handler = new ToMarkdownContentHandler();
            handler.StartDocument();
            handler.StartElement("", "h1", "h1", new AttributesImpl());
            handler.Characters("Title".ToCharArray(), 0, 5);
            handler.EndElement("", "h1", "h1");
            handler.EndDocument();

            handler.ToString().Should().Contain("# Title");
        }

        [Fact]
        public void Paragraph_ShouldOutputDoubleNewline()
        {
            var handler = new ToMarkdownContentHandler();
            handler.StartDocument();
            handler.StartElement("", "p", "p", new AttributesImpl());
            handler.Characters("Hello".ToCharArray(), 0, 5);
            handler.EndElement("", "p", "p");
            handler.EndDocument();

            handler.ToString().Should().Contain("Hello\n\n");
        }

        [Fact]
        public void Bold_ShouldOutputDoubleAsterisks()
        {
            var handler = new ToMarkdownContentHandler();
            handler.StartDocument();
            handler.StartElement("", "b", "b", new AttributesImpl());
            handler.Characters("bold".ToCharArray(), 0, 4);
            handler.EndElement("", "b", "b");
            handler.EndDocument();

            handler.ToString().Should().Contain("**bold**");
        }

        [Fact]
        public void Italic_ShouldOutputSingleAsterisk()
        {
            var handler = new ToMarkdownContentHandler();
            handler.StartDocument();
            handler.StartElement("", "i", "i", new AttributesImpl());
            handler.Characters("italic".ToCharArray(), 0, 6);
            handler.EndElement("", "i", "i");
            handler.EndDocument();

            handler.ToString().Should().Contain("*italic*");
        }

        [Fact]
        public void Link_ShouldOutputMarkdownLink()
        {
            var handler = new ToMarkdownContentHandler();
            var atts = new AttributesImpl();
            atts.AddAttribute("", "href", "href", "CDATA", "https://example.com");

            handler.StartDocument();
            handler.StartElement("", "a", "a", atts);
            handler.Characters("click".ToCharArray(), 0, 5);
            handler.EndElement("", "a", "a");
            handler.EndDocument();

            handler.ToString().Should().Contain("[click](https://example.com)");
        }

        [Fact]
        public void LinkHref_ShouldEscapeMarkdownDelimiters()
        {
            var handler = new ToMarkdownContentHandler();
            var atts = new AttributesImpl();
            atts.AddAttribute("", "href", "href", "CDATA", "https://example.com/a)b[c]");

            handler.StartDocument();
            handler.StartElement("", "a", "a", atts);
            handler.Characters("click".ToCharArray(), 0, 5);
            handler.EndElement("", "a", "a");
            handler.EndDocument();

            handler.ToString().Should().Contain("[click](https://example.com/a\\)b\\[c\\])");
        }

        [Fact]
        public void UnorderedList_ShouldOutputDashPrefix()
        {
            var handler = new ToMarkdownContentHandler();
            handler.StartDocument();
            handler.StartElement("", "ul", "ul", new AttributesImpl());
            handler.StartElement("", "li", "li", new AttributesImpl());
            handler.Characters("Item 1".ToCharArray(), 0, 6);
            handler.EndElement("", "li", "li");
            handler.EndElement("", "ul", "ul");
            handler.EndDocument();

            handler.ToString().Should().Contain("- Item 1");
        }

        [Fact]
        public void OrderedList_ShouldOutputNumberPrefix()
        {
            var handler = new ToMarkdownContentHandler();
            handler.StartDocument();
            handler.StartElement("", "ol", "ol", new AttributesImpl());
            handler.StartElement("", "li", "li", new AttributesImpl());
            handler.Characters("First".ToCharArray(), 0, 5);
            handler.EndElement("", "li", "li");
            handler.EndElement("", "ol", "ol");
            handler.EndDocument();

            handler.ToString().Should().Contain("1. First");
        }

        [Fact]
        public void Code_ShouldOutputBackticks()
        {
            var handler = new ToMarkdownContentHandler();
            handler.StartDocument();
            handler.StartElement("", "code", "code", new AttributesImpl());
            handler.Characters("var x = 1".ToCharArray(), 0, 9);
            handler.EndElement("", "code", "code");
            handler.EndDocument();

            handler.ToString().Should().Contain("`var x = 1`");
        }

        [Fact]
        public void PreCode_ShouldOutputCodeBlock()
        {
            var handler = new ToMarkdownContentHandler();
            handler.StartDocument();
            handler.StartElement("", "pre", "pre", new AttributesImpl());
            handler.StartElement("", "code", "code", new AttributesImpl());
            handler.Characters("line1\nline2".ToCharArray(), 0, 11);
            handler.EndElement("", "code", "code");
            handler.EndElement("", "pre", "pre");
            handler.EndDocument();

            var result = handler.ToString();
            result.Should().Contain("```");
            result.Should().Contain("line1\nline2");
        }

        [Fact]
        public void Table_ShouldOutputPipeDelimited()
        {
            var handler = new ToMarkdownContentHandler();
            handler.StartDocument();
            handler.StartElement("", "table", "table", new AttributesImpl());
            handler.StartElement("", "tr", "tr", new AttributesImpl());
            handler.StartElement("", "th", "th", new AttributesImpl());
            handler.Characters("Header".ToCharArray(), 0, 6);
            handler.EndElement("", "th", "th");
            handler.EndElement("", "tr", "tr");
            handler.EndElement("", "table", "table");
            handler.EndDocument();

            handler.ToString().Should().Contain("| Header |");
        }

        [Fact]
        public void HorizontalRule_ShouldOutputDashes()
        {
            var handler = new ToMarkdownContentHandler();
            handler.StartDocument();
            handler.StartElement("", "hr", "hr", new AttributesImpl());
            handler.EndElement("", "hr", "hr");
            handler.EndDocument();

            handler.ToString().Should().Contain("---");
        }

        [Fact]
        public void Blockquote_ShouldOutputAngleBracket()
        {
            var handler = new ToMarkdownContentHandler();
            handler.StartDocument();
            handler.StartElement("", "blockquote", "blockquote", new AttributesImpl());
            handler.Characters("quoted".ToCharArray(), 0, 6);
            handler.EndElement("", "blockquote", "blockquote");
            handler.EndDocument();

            handler.ToString().Should().Contain("> quoted");
        }

        [Fact]
        public void HeadSection_ShouldBeSuppressed()
        {
            var handler = new ToMarkdownContentHandler();
            handler.StartDocument();
            handler.StartElement("", "head", "head", new AttributesImpl());
            handler.Characters("meta content".ToCharArray(), 0, 12);
            handler.EndElement("", "head", "head");
            handler.StartElement("", "p", "p", new AttributesImpl());
            handler.Characters("visible".ToCharArray(), 0, 7);
            handler.EndElement("", "p", "p");
            handler.EndDocument();

            var result = handler.ToString();
            result.Should().NotContain("meta content");
            result.Should().Contain("visible");
        }

        [Fact]
        public void NestedSuppressedElements_ShouldRemainSuppressedUntilOuterElementEnds()
        {
            var handler = new ToMarkdownContentHandler();
            handler.StartDocument();
            handler.StartElement("", "head", "head", new AttributesImpl());
            handler.StartElement("", "style", "style", new AttributesImpl());
            handler.Characters("hidden".ToCharArray(), 0, 6);
            handler.EndElement("", "style", "style");
            handler.Characters("also hidden".ToCharArray(), 0, 11);
            handler.EndElement("", "head", "head");
            handler.StartElement("", "p", "p", new AttributesImpl());
            handler.Characters("visible".ToCharArray(), 0, 7);
            handler.EndElement("", "p", "p");
            handler.EndDocument();

            var result = handler.ToString();
            result.Should().NotContain("hidden");
            result.Should().Contain("visible");
        }
    }
}

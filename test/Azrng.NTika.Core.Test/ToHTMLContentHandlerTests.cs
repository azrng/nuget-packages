using Azrng.NTika.Core.Model;
using Azrng.NTika.Core.Sax;
using FluentAssertions;
using Xunit;

namespace Azrng.NTika.Core.Test
{
    public class ToHTMLContentHandlerTests
    {
        [Fact]
        public void Paragraph_ShouldOutputPTag()
        {
            var handler = new ToHTMLContentHandler();
            handler.StartDocument();
            // Simulate body context
            handler.StartElement("", "body", "body", new AttributesImpl());
            handler.StartElement("", "p", "p", new AttributesImpl());
            handler.Characters("Hello".ToCharArray(), 0, 5);
            handler.EndElement("", "p", "p");
            handler.EndElement("", "body", "body");
            handler.EndDocument();

            handler.ToString().Should().Contain("<p>Hello</p>");
        }

        [Fact]
        public void Heading_ShouldOutputHTag()
        {
            var handler = new ToHTMLContentHandler();
            handler.StartDocument();
            handler.StartElement("", "body", "body", new AttributesImpl());
            handler.StartElement("", "h1", "h1", new AttributesImpl());
            handler.Characters("Title".ToCharArray(), 0, 5);
            handler.EndElement("", "h1", "h1");
            handler.EndElement("", "body", "body");
            handler.EndDocument();

            handler.ToString().Should().Contain("<h1>Title</h1>");
        }

        [Fact]
        public void Link_ShouldOutputATagWithHref()
        {
            var handler = new ToHTMLContentHandler();
            var atts = new AttributesImpl();
            atts.AddAttribute("", "href", "href", "CDATA", "https://example.com");

            handler.StartDocument();
            handler.StartElement("", "body", "body", new AttributesImpl());
            handler.StartElement("", "a", "a", atts);
            handler.Characters("click".ToCharArray(), 0, 5);
            handler.EndElement("", "a", "a");
            handler.EndElement("", "body", "body");
            handler.EndDocument();

            handler.ToString().Should().Contain("<a href=\"https://example.com\">click</a>");
        }

        [Fact]
        public void Bold_ShouldOutputStrongTag()
        {
            var handler = new ToHTMLContentHandler();
            handler.StartDocument();
            handler.StartElement("", "body", "body", new AttributesImpl());
            handler.StartElement("", "strong", "strong", new AttributesImpl());
            handler.Characters("bold".ToCharArray(), 0, 4);
            handler.EndElement("", "strong", "strong");
            handler.EndElement("", "body", "body");
            handler.EndDocument();

            handler.ToString().Should().Contain("<strong>bold</strong>");
        }

        [Fact]
        public void VoidElement_ShouldSelfClose()
        {
            var handler = new ToHTMLContentHandler();
            handler.StartDocument();
            handler.StartElement("", "body", "body", new AttributesImpl());
            handler.StartElement("", "br", "br", new AttributesImpl());
            handler.EndElement("", "br", "br");
            handler.EndElement("", "body", "body");
            handler.EndDocument();

            handler.ToString().Should().Contain("<br />");
        }

        [Fact]
        public void Script_ShouldBeSuppressed()
        {
            var handler = new ToHTMLContentHandler();
            handler.StartDocument();
            handler.StartElement("", "body", "body", new AttributesImpl());
            handler.StartElement("", "script", "script", new AttributesImpl());
            handler.Characters("alert('xss')".ToCharArray(), 0, 12);
            handler.EndElement("", "script", "script");
            handler.StartElement("", "p", "p", new AttributesImpl());
            handler.Characters("safe".ToCharArray(), 0, 4);
            handler.EndElement("", "p", "p");
            handler.EndElement("", "body", "body");
            handler.EndDocument();

            var result = handler.ToString();
            result.Should().NotContain("alert");
            result.Should().Contain("<p>safe</p>");
        }

        [Fact]
        public void NestedSuppressedElements_ShouldRemainSuppressedUntilOuterElementEnds()
        {
            var handler = new ToHTMLContentHandler();
            handler.StartDocument();
            handler.StartElement("", "body", "body", new AttributesImpl());
            handler.StartElement("", "head", "head", new AttributesImpl());
            handler.StartElement("", "style", "style", new AttributesImpl());
            handler.Characters("hidden".ToCharArray(), 0, 6);
            handler.EndElement("", "style", "style");
            handler.Characters("also hidden".ToCharArray(), 0, 11);
            handler.EndElement("", "head", "head");
            handler.StartElement("", "p", "p", new AttributesImpl());
            handler.Characters("visible".ToCharArray(), 0, 7);
            handler.EndElement("", "p", "p");
            handler.EndElement("", "body", "body");
            handler.EndDocument();

            var result = handler.ToString();
            result.Should().NotContain("hidden");
            result.Should().Contain("<p>visible</p>");
        }

        [Fact]
        public void AttributeValue_ShouldEscapeSingleQuote()
        {
            var handler = new ToHTMLContentHandler();
            var atts = new AttributesImpl();
            atts.AddAttribute("", "title", "title", "CDATA", "it's ok");

            handler.StartDocument();
            handler.StartElement("", "body", "body", new AttributesImpl());
            handler.StartElement("", "p", "p", atts);
            handler.EndElement("", "p", "p");
            handler.EndElement("", "body", "body");
            handler.EndDocument();

            handler.ToString().Should().Contain("title=\"it&#39;s ok\"");
        }

        [Fact]
        public void TextContent_ShouldEscapeHtmlEntities()
        {
            var handler = new ToHTMLContentHandler();
            handler.StartDocument();
            handler.StartElement("", "body", "body", new AttributesImpl());
            handler.StartElement("", "p", "p", new AttributesImpl());
            handler.Characters("<script>&\"".ToCharArray(), 0, 10);
            handler.EndElement("", "p", "p");
            handler.EndElement("", "body", "body");
            handler.EndDocument();

            var result = handler.ToString();
            // In text content, only &, <, > need escaping (not quotes)
            result.Should().Contain("&lt;script&gt;&amp;\"");
        }
    }
}

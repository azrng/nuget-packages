using System.Text;
using Azrng.NTika.Core.IO;
using Azrng.NTika.Core.Model;
using Azrng.NTika.Core.Sax;
using FluentAssertions;
using Xunit;

namespace Azrng.NTika.Parsers.Html.Test
{
    public class HtmlParserTests
    {
        [Fact]
        public void GetSupportedTypes_ShouldReturnHtmlTypes()
        {
            var parser = new Azrng.NTika.Parsers.Html.HtmlParser();
            var types = parser.GetSupportedTypes(new ParseContext());
            types.Should().HaveCount(2);
        }

        [Fact]
        public void Parse_SimpleHtml_ShouldExtractText()
        {
            var parser = new Azrng.NTika.Parsers.Html.HtmlParser();
            var html = "<html><body><h1>Hello</h1><p>World</p></body></html>";
            var data = Encoding.UTF8.GetBytes(html);
            using var stream = TikaInputStream.Get(data);
            var handler = new WriteOutContentHandler();
            var metadata = new Metadata();
            var context = new ParseContext();

            parser.Parse(stream, handler, metadata, context);

            var text = handler.ToString();
            text.Should().Contain("Hello");
            text.Should().Contain("World");
        }

        [Fact]
        public void Parse_HtmlWithTitle_ShouldExtractTitle()
        {
            var parser = new Azrng.NTika.Parsers.Html.HtmlParser();
            var html = "<html><head><title>Test Title</title></head><body><p>Content</p></body></html>";
            var data = Encoding.UTF8.GetBytes(html);
            using var stream = TikaInputStream.Get(data);
            var handler = new WriteOutContentHandler();
            var metadata = new Metadata();
            var context = new ParseContext();

            parser.Parse(stream, handler, metadata, context);

            metadata.Get(TikaCoreProperties.TITLE).Should().Be("Test Title");
        }

        [Fact]
        public void Parse_HtmlWithMeta_ShouldExtractMetadata()
        {
            var parser = new Azrng.NTika.Parsers.Html.HtmlParser();
            var html = "<html><head><meta name='description' content='A test page'><meta name='keywords' content='test,html'></head><body><p>Content</p></body></html>";
            var data = Encoding.UTF8.GetBytes(html);
            using var stream = TikaInputStream.Get(data);
            var handler = new WriteOutContentHandler();
            var metadata = new Metadata();
            var context = new ParseContext();

            parser.Parse(stream, handler, metadata, context);

            metadata.Get(TikaCoreProperties.DESCRIPTION).Should().Be("A test page");
            metadata.Get(TikaCoreProperties.SUBJECT).Should().Be("test,html");
        }

        [Fact]
        public void Parse_HtmlWithNestedElements_ShouldPreserveStructure()
        {
            var parser = new Azrng.NTika.Parsers.Html.HtmlParser();
            var html = "<html><body><div><span>Nested</span> text</div></body></html>";
            var data = Encoding.UTF8.GetBytes(html);
            using var stream = TikaInputStream.Get(data);
            var handler = new WriteOutContentHandler();
            var metadata = new Metadata();
            var context = new ParseContext();

            parser.Parse(stream, handler, metadata, context);

            var text = handler.ToString();
            text.Should().Contain("Nested");
            text.Should().Contain("text");
        }

        [Fact]
        public void Parse_EmptyHtml_ShouldNotThrow()
        {
            var parser = new Azrng.NTika.Parsers.Html.HtmlParser();
            var html = "<html><body></body></html>";
            var data = Encoding.UTF8.GetBytes(html);
            using var stream = TikaInputStream.Get(data);
            var handler = new WriteOutContentHandler();
            var metadata = new Metadata();
            var context = new ParseContext();

            var act = () => parser.Parse(stream, handler, metadata, context);
            act.Should().NotThrow();
        }
    }
}

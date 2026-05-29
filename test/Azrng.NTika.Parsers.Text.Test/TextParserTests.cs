using System.Text;
using Azrng.NTika.Core.Abstractions;
using Azrng.NTika.Core.IO;
using Azrng.NTika.Core.Model;
using Azrng.NTika.Core.Sax;
using FluentAssertions;
using Xunit;

namespace Azrng.NTika.Parsers.Text.Test
{
    public class TextParserTests
    {
        [Fact]
        public void GetSupportedTypes_ShouldReturnTextPlain()
        {
            var parser = new TextParser();
            var types = parser.GetSupportedTypes(new ParseContext());
            types.Should().Contain(MediaType.TextPlain);
            types.Should().HaveCount(1);
        }

        [Fact]
        public void Parse_PlainText_ShouldExtractContent()
        {
            var parser = new TextParser();
            var data = Encoding.UTF8.GetBytes("Hello, world!");
            using var stream = TikaInputStream.Get(data);
            var handler = new WriteOutContentHandler();
            var metadata = new Metadata();
            var context = new ParseContext();

            parser.Parse(stream, handler, metadata, context);

            var text = handler.ToString();
            text.Should().Contain("Hello, world!");
        }

        [Fact]
        public void Parse_ShouldSetContentTypeWithCharset()
        {
            var parser = new TextParser();
            var data = Encoding.UTF8.GetBytes("test");
            using var stream = TikaInputStream.Get(data);
            var handler = new WriteOutContentHandler();
            var metadata = new Metadata();
            var context = new ParseContext();

            parser.Parse(stream, handler, metadata, context);

            var contentType = metadata.Get(TikaCoreProperties.CONTENT_TYPE);
            contentType.Should().Contain("text/plain");
            contentType.Should().Contain("charset");
        }

        [Fact]
        public void Parse_EmptyText_ShouldNotThrow()
        {
            var parser = new TextParser();
            var data = Encoding.UTF8.GetBytes("");
            using var stream = TikaInputStream.Get(data);
            var handler = new WriteOutContentHandler();
            var metadata = new Metadata();
            var context = new ParseContext();

            var act = () => parser.Parse(stream, handler, metadata, context);
            act.Should().NotThrow();
        }

        [Fact]
        public void Parse_LargeText_ShouldExtractAllContent()
        {
            var parser = new TextParser();
            var largeText = new string('A', 10000);
            var data = Encoding.UTF8.GetBytes(largeText);
            using var stream = TikaInputStream.Get(data);
            var handler = new WriteOutContentHandler();
            var metadata = new Metadata();
            var context = new ParseContext();

            parser.Parse(stream, handler, metadata, context);

            var text = handler.ToString();
            text.Should().Contain(new string('A', 10000));
        }

        [Fact]
        public void Parse_UnicodeText_ShouldPreserveContent()
        {
            var parser = new TextParser();
            var unicodeText = "你好世界 こんにちは 🌍";
            var data = Encoding.UTF8.GetBytes(unicodeText);
            using var stream = TikaInputStream.Get(data);
            var handler = new WriteOutContentHandler();
            var metadata = new Metadata();
            var context = new ParseContext();

            parser.Parse(stream, handler, metadata, context);

            var text = handler.ToString();
            text.Should().Contain("你好世界");
            text.Should().Contain("こんにちは");
        }

        [Fact]
        public void Parse_MultilineText_ShouldPreserveLines()
        {
            var parser = new TextParser();
            var multilineText = "Line 1\nLine 2\nLine 3";
            var data = Encoding.UTF8.GetBytes(multilineText);
            using var stream = TikaInputStream.Get(data);
            var handler = new WriteOutContentHandler();
            var metadata = new Metadata();
            var context = new ParseContext();

            parser.Parse(stream, handler, metadata, context);

            var text = handler.ToString();
            text.Should().Contain("Line 1");
            text.Should().Contain("Line 2");
            text.Should().Contain("Line 3");
        }
    }
}

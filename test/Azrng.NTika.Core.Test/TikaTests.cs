using Xunit;
using System.Text;
using Azrng.NTika.Core.Abstractions;
using Azrng.NTika.Core.Exception;
using Azrng.NTika.Core.IO;
using Azrng.NTika.Core.Model;
using Azrng.NTika.Core.Sax;
using FluentAssertions;

namespace Azrng.NTika.Core.Test
{
    public class TikaTests
    {
        [Fact]
        public void Detect_PlainText_ShouldReturnTextPlain()
        {
            var tika = new Tika();
            var data = Encoding.UTF8.GetBytes("Hello, world!");
            using var stream = new MemoryStream(data);
            var result = tika.Detect(stream);
            result.Should().Contain("text");
        }

        [Fact]
        public void Parse_ShouldReturnMetadata()
        {
            var tika = new Tika();
            var data = Encoding.UTF8.GetBytes("Hello, world!");
            using var stream = new MemoryStream(data);
            var metadata = tika.Parse(stream);
            metadata.Should().NotBeNull();
        }

        [Fact]
        public void ToHtml_WhenOutputExceedsLimit_ShouldThrow()
        {
            var tika = new Tika(new FixedTextParser("0123456789"));
            tika.MaxStringLength = 5;

            using var stream = new MemoryStream(Encoding.UTF8.GetBytes("ignored"));

            var act = () => tika.ToHtml(stream);
            act.Should().Throw<WriteLimitReachedException>();
        }

        [Fact]
        public void ToMarkdown_WhenOutputExceedsLimit_ShouldThrow()
        {
            var tika = new Tika(new FixedTextParser("0123456789"));
            tika.MaxStringLength = 5;

            using var stream = new MemoryStream(Encoding.UTF8.GetBytes("ignored"));

            var act = () => tika.ToMarkdown(stream);
            act.Should().Throw<WriteLimitReachedException>();
        }

        private sealed class FixedTextParser : IParser
        {
            private readonly string _text;

            public FixedTextParser(string text)
            {
                _text = text;
            }

            public ISet<MediaType> GetSupportedTypes(ParseContext context)
            {
                return new HashSet<MediaType> { MediaType.TextPlain };
            }

            public void Parse(TikaInputStream stream, IContentHandler handler, Metadata metadata, ParseContext context)
            {
                var xhtml = new XHTMLContentHandler(handler);
                xhtml.StartDocument();
                xhtml.ElementWithCharacters(XHTMLContentHandler.P, _text);
                xhtml.EndDocument();
            }
        }
    }
}

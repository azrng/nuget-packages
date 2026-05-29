using Xunit;
using System.Text;
using Azrng.NTika.Core.Abstractions;
using Azrng.NTika.Core.IO;
using Azrng.NTika.Core.Model;
using Azrng.NTika.Core.Parser;
using Azrng.NTika.Core.Sax;
using FluentAssertions;

namespace Azrng.NTika.Core.Test.Parser
{
    public class CompositeParserTests
    {
        [Fact]
        public void GetSupportedTypes_ShouldCollectAll()
        {
            var registry = new MediaTypeRegistry();
            var parser1 = new TestParser(MediaType.TextPlain);
            var parser2 = new TestParser(MediaType.TextHtml);
            var composite = new CompositeParser(registry, parser1, parser2);

            var types = composite.GetSupportedTypes(new ParseContext());
            types.Should().Contain(MediaType.TextPlain);
            types.Should().Contain(MediaType.TextHtml);
        }

        [Fact]
        public void Parse_ShouldResolveCorrectParser()
        {
            var registry = new MediaTypeRegistry();
            var textParser = new TestParser(MediaType.TextPlain);
            var htmlParser = new TestParser(MediaType.TextHtml);
            var composite = new CompositeParser(registry, textParser, htmlParser);

            var metadata = new Metadata();
            metadata.Set(TikaCoreProperties.CONTENT_TYPE, "text/plain");
            var context = new ParseContext();
            var handler = new WriteOutContentHandler();

            using var tis = TikaInputStream.Get(Encoding.UTF8.GetBytes("test"));
            composite.Parse(tis, handler, metadata, context);

            textParser.ParseCalled.Should().BeTrue();
            htmlParser.ParseCalled.Should().BeFalse();
        }

        private class TestParser : IParser
        {
            private readonly MediaType _supportedType;
            public bool ParseCalled { get; private set; }

            public TestParser(MediaType supportedType)
            {
                _supportedType = supportedType;
            }

            public ISet<MediaType> GetSupportedTypes(ParseContext context)
            {
                return new HashSet<MediaType> { _supportedType };
            }

            public void Parse(TikaInputStream stream, IContentHandler handler,
                Metadata metadata, ParseContext context)
            {
                ParseCalled = true;
            }
        }
    }
}

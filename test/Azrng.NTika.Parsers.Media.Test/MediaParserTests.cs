using Azrng.NTika.Core.IO;
using Azrng.NTika.Core.Model;
using Azrng.NTika.Core.Sax;
using FluentAssertions;
using Xunit;

namespace Azrng.NTika.Parsers.Media.Test
{
    public class MediaParserTests
    {
        [Fact]
        public void GetSupportedTypes_ShouldReturnMediaTypes()
        {
            var parser = new MediaParser();
            var types = parser.GetSupportedTypes(new ParseContext());
            types.Should().HaveCount(4);
        }

        [Fact]
        public void Parse_UnknownFormat_ShouldNotThrow()
        {
            var parser = new MediaParser();
            var data = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05 };
            using var stream = TikaInputStream.Get(data);
            var handler = new WriteOutContentHandler();
            var metadata = new Metadata();
            var context = new ParseContext();

            var act = () => parser.Parse(stream, handler, metadata, context);
            act.Should().NotThrow();
        }
    }
}

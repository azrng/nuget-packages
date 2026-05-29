using System.Text;
using Azrng.NTika.Core.IO;
using Azrng.NTika.Core.Model;
using Azrng.NTika.Core.Sax;
using FluentAssertions;
using Xunit;

namespace Azrng.NTika.Parsers.Rtf.Test
{
    public class RtfParserTests
    {
        [Fact]
        public void GetSupportedTypes_ShouldReturnRtf()
        {
            var parser = new RtfParser();
            var types = parser.GetSupportedTypes(new ParseContext());
            types.Should().HaveCount(1);
        }

        [Fact]
        public void Parse_SimpleRtf_ShouldExtractText()
        {
            var parser = new RtfParser();
            var rtf = @"{\rtf1\ansi\deff0 {\fonttbl {\f0 Times New Roman;}} \f0\fs24 Hello RTF World!}";
            var data = Encoding.UTF8.GetBytes(rtf);
            using var stream = TikaInputStream.Get(data);
            var handler = new WriteOutContentHandler();
            var metadata = new Metadata();
            var context = new ParseContext();

            parser.Parse(stream, handler, metadata, context);

            var text = handler.ToString();
            text.Should().Contain("Hello RTF World!");
        }

        [Fact]
        public void Parse_RtfWithParagraphs_ShouldExtractText()
        {
            var parser = new RtfParser();
            var rtf = @"{\rtf1\ansi\deff0 {\fonttbl {\f0 Times New Roman;}} \f0\fs24 First paragraph.\par Second paragraph.}";
            var data = Encoding.UTF8.GetBytes(rtf);
            using var stream = TikaInputStream.Get(data);
            var handler = new WriteOutContentHandler();
            var metadata = new Metadata();
            var context = new ParseContext();

            parser.Parse(stream, handler, metadata, context);

            var text = handler.ToString();
            text.Should().Contain("First paragraph.");
            text.Should().Contain("Second paragraph.");
        }

        [Fact]
        public void Parse_EmptyRtf_ShouldNotThrow()
        {
            var parser = new RtfParser();
            var data = Encoding.UTF8.GetBytes("");
            using var stream = TikaInputStream.Get(data);
            var handler = new WriteOutContentHandler();
            var metadata = new Metadata();
            var context = new ParseContext();

            var act = () => parser.Parse(stream, handler, metadata, context);
            act.Should().NotThrow();
        }

        [Fact]
        public void Parse_NegativeUnicodeEscape_ShouldDecodeUnsignedCharacter()
        {
            var parser = new RtfParser();
            var rtf = @"{\rtf1\ansi \u-30616?}";
            var data = Encoding.UTF8.GetBytes(rtf);
            using var stream = TikaInputStream.Get(data);
            var handler = new WriteOutContentHandler();
            var metadata = new Metadata();
            var context = new ParseContext();

            parser.Parse(stream, handler, metadata, context);

            handler.ToString().Should().Contain("表");
        }

        [Fact]
        public void Parse_HexEscape_ShouldUseAnsiCodePage()
        {
            var parser = new RtfParser();
            var rtf = @"{\rtf1\ansi\ansicpg1252 Caf\'e9}";
            var data = Encoding.UTF8.GetBytes(rtf);
            using var stream = TikaInputStream.Get(data);
            var handler = new WriteOutContentHandler();
            var metadata = new Metadata();
            var context = new ParseContext();

            parser.Parse(stream, handler, metadata, context);

            handler.ToString().Should().Contain("Café");
        }
    }
}

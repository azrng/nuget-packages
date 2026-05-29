using System.Text;
using Azrng.NTika.Core.IO;
using Azrng.NTika.Core.Model;
using Azrng.NTika.Core.Sax;
using FluentAssertions;
using Xunit;

namespace Azrng.NTika.Parsers.Xml.Test
{
    public class XmlParserTests
    {
        [Fact]
        public void GetSupportedTypes_ShouldReturnXmlAndSvg()
        {
            var parser = new XmlParser();
            var types = parser.GetSupportedTypes(new ParseContext());
            types.Should().HaveCount(2);
            types.Should().Contain(MediaType.ApplicationXml);
            types.Should().Contain(MediaType.Parse("image/svg+xml")!);
        }

        [Fact]
        public void Parse_SimpleXml_ShouldExtractText()
        {
            var parser = new XmlParser();
            var xml = "<?xml version=\"1.0\"?><root><title>Hello</title><body>World</body></root>";
            var data = Encoding.UTF8.GetBytes(xml);
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
        public void Parse_ShouldSetContentType()
        {
            var parser = new XmlParser();
            var xml = "<?xml version=\"1.0\"?><root>text</root>";
            var data = Encoding.UTF8.GetBytes(xml);
            using var stream = TikaInputStream.Get(data);
            var handler = new WriteOutContentHandler();
            var metadata = new Metadata();
            var context = new ParseContext();

            parser.Parse(stream, handler, metadata, context);

            metadata.Get(TikaCoreProperties.CONTENT_TYPE).Should().Contain("application/xml");
        }

        [Fact]
        public void Parse_XmlWithCData_ShouldExtractCDataContent()
        {
            var parser = new XmlParser();
            var xml = "<?xml version=\"1.0\"?><root><data><![CDATA[CDATA content here]]></data></root>";
            var data = Encoding.UTF8.GetBytes(xml);
            using var stream = TikaInputStream.Get(data);
            var handler = new WriteOutContentHandler();
            var metadata = new Metadata();
            var context = new ParseContext();

            parser.Parse(stream, handler, metadata, context);

            var text = handler.ToString();
            text.Should().Contain("CDATA content here");
        }

        [Fact]
        public void Parse_NestedXml_ShouldExtractAllText()
        {
            var parser = new XmlParser();
            var xml = "<?xml version=\"1.0\"?><root><a><b><c>deep text</c></b></a></root>";
            var data = Encoding.UTF8.GetBytes(xml);
            using var stream = TikaInputStream.Get(data);
            var handler = new WriteOutContentHandler();
            var metadata = new Metadata();
            var context = new ParseContext();

            parser.Parse(stream, handler, metadata, context);

            var text = handler.ToString();
            text.Should().Contain("deep text");
        }

        [Fact]
        public void Parse_EmptyXml_ShouldNotThrow()
        {
            var parser = new XmlParser();
            var xml = "<?xml version=\"1.0\"?><root/>";
            var data = Encoding.UTF8.GetBytes(xml);
            using var stream = TikaInputStream.Get(data);
            var handler = new WriteOutContentHandler();
            var metadata = new Metadata();
            var context = new ParseContext();

            var act = () => parser.Parse(stream, handler, metadata, context);
            act.Should().NotThrow();
        }
    }
}

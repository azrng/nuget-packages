using System.IO.Compression;
using System.Text;
using Azrng.NTika.Core.IO;
using Azrng.NTika.Core.Model;
using Azrng.NTika.Core.Sax;
using FluentAssertions;
using Xunit;

namespace Azrng.NTika.Parsers.Odf.Test
{
    public class OpenDocumentParserTests
    {
        [Fact]
        public void GetSupportedTypes_ShouldReturnOdfTypes()
        {
            var parser = new OpenDocumentParser();
            var types = parser.GetSupportedTypes(new ParseContext());
            types.Should().HaveCount(3);
        }

        [Fact]
        public void Parse_OdtDocument_ShouldExtractText()
        {
            var parser = new OpenDocumentParser();
            var odtBytes = CreateTestOdt();
            using var stream = TikaInputStream.Get(odtBytes);
            var handler = new WriteOutContentHandler();
            var metadata = new Metadata();
            var context = new ParseContext();

            parser.Parse(stream, handler, metadata, context);

            var text = handler.ToString();
            text.Should().Contain("Hello ODF World");
            text.Should().Contain("Second paragraph");
        }

        [Fact]
        public void Parse_OdtDocument_ShouldSetContentType()
        {
            var parser = new OpenDocumentParser();
            var odtBytes = CreateTestOdt();
            using var stream = TikaInputStream.Get(odtBytes);
            var handler = new WriteOutContentHandler();
            var metadata = new Metadata();
            var context = new ParseContext();

            parser.Parse(stream, handler, metadata, context);

            metadata.Get(TikaCoreProperties.CONTENT_TYPE).Should().Contain("opendocument.text");
        }

        [Fact]
        public void Parse_InvalidFile_ShouldNotThrow()
        {
            var parser = new OpenDocumentParser();
            var data = Encoding.UTF8.GetBytes("not a zip file");
            using var stream = TikaInputStream.Get(data);
            var handler = new WriteOutContentHandler();
            var metadata = new Metadata();
            var context = new ParseContext();

            var act = () => parser.Parse(stream, handler, metadata, context);
            act.Should().NotThrow();
        }

        private static byte[] CreateTestOdt()
        {
            using var ms = new MemoryStream();
            using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
            {
                // mimetype entry
                var mimetypeEntry = archive.CreateEntry("mimetype");
                using (var writer = new StreamWriter(mimetypeEntry.Open()))
                {
                    writer.Write("application/vnd.oasis.opendocument.text");
                }

                // content.xml entry
                var contentEntry = archive.CreateEntry("content.xml");
                using (var writer = new StreamWriter(contentEntry.Open(), Encoding.UTF8))
                {
                    writer.Write(@"<?xml version=""1.0"" encoding=""UTF-8""?>
<office:document-content xmlns:office=""urn:oasis:names:tc:opendocument:xmlns:office:1.0""
    xmlns:text=""urn:oasis:names:tc:opendocument:xmlns:text:1.0"">
    <office:body>
        <office:text>
            <text:p>Hello ODF World</text:p>
            <text:p>Second paragraph</text:p>
        </office:text>
    </office:body>
</office:document-content>");
                }
            }
            return ms.ToArray();
        }
    }
}

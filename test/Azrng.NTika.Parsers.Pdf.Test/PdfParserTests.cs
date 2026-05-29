using Azrng.NTika.Core.IO;
using Azrng.NTika.Core.Model;
using Azrng.NTika.Core.Sax;
using FluentAssertions;
using Xunit;

namespace Azrng.NTika.Parsers.Pdf.Test
{
    public class PdfParserTests
    {
        [Fact]
        public void GetSupportedTypes_ShouldReturnPdf()
        {
            var parser = new PdfParser();
            var types = parser.GetSupportedTypes(new ParseContext());
            types.Should().HaveCount(1);
            types.Should().Contain(MediaType.Application("pdf"));
        }

        [Fact]
        public void Parse_MinimalPdf_ShouldExtractText()
        {
            var parser = new PdfParser();
            var pdfBytes = CreateTestPdf.CreateMinimalPdf("Test PDF Content");
            using var stream = TikaInputStream.Get(pdfBytes);
            var handler = new WriteOutContentHandler();
            var metadata = new Metadata();
            var context = new ParseContext();

            parser.Parse(stream, handler, metadata, context);

            var text = handler.ToString();
            text.Should().Contain("Test PDF Content");
        }

        [Fact]
        public void Parse_ShouldSetContentType()
        {
            var parser = new PdfParser();
            var pdfBytes = CreateTestPdf.CreateMinimalPdf();
            using var stream = TikaInputStream.Get(pdfBytes);
            var handler = new WriteOutContentHandler();
            var metadata = new Metadata();
            var context = new ParseContext();

            parser.Parse(stream, handler, metadata, context);

            metadata.Get(TikaCoreProperties.CONTENT_TYPE).Should().Contain("application/pdf");
        }

        [Fact]
        public void Parse_ShouldExtractPageCount()
        {
            var parser = new PdfParser();
            var pdfBytes = CreateTestPdf.CreateMinimalPdf();
            using var stream = TikaInputStream.Get(pdfBytes);
            var handler = new WriteOutContentHandler();
            var metadata = new Metadata();
            var context = new ParseContext();

            parser.Parse(stream, handler, metadata, context);

            metadata.Get("pdf:pageCount").Should().Be("1");
        }
    }
}

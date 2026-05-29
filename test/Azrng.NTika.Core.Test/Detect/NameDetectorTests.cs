using Xunit;
using Azrng.NTika.Core.Detect;
using Azrng.NTika.Core.IO;
using Azrng.NTika.Core.Model;
using FluentAssertions;

namespace Azrng.NTika.Core.Test.Detect
{
    public class NameDetectorTests
    {
        private readonly NameDetector _detector = new();

        [Fact]
        public void Detect_PdfExtension_ShouldReturnPdf()
        {
            var metadata = new Metadata();
            metadata.Set(TikaCoreProperties.RESOURCE_NAME_KEY, "document.pdf");
            var result = _detector.Detect(null, metadata, new ParseContext());
            result.Type.Should().Be("application");
            result.Subtype.Should().Be("pdf");
        }

        [Fact]
        public void Detect_HtmlExtension_ShouldReturnHtml()
        {
            var metadata = new Metadata();
            metadata.Set(TikaCoreProperties.RESOURCE_NAME_KEY, "page.html");
            var result = _detector.Detect(null, metadata, new ParseContext());
            result.Should().Be(MediaType.TextHtml);
        }

        [Fact]
        public void Detect_UnknownExtension_ShouldReturnOctetStream()
        {
            var metadata = new Metadata();
            metadata.Set(TikaCoreProperties.RESOURCE_NAME_KEY, "file.xyz123");
            var result = _detector.Detect(null, metadata, new ParseContext());
            result.Should().Be(MediaType.OctetStream);
        }

        [Fact]
        public void Detect_NoName_ShouldReturnOctetStream()
        {
            var result = _detector.Detect(null, new Metadata(), new ParseContext());
            result.Should().Be(MediaType.OctetStream);
        }
    }
}

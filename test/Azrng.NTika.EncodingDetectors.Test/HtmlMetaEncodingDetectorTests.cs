using System.Text;
using Azrng.NTika.Core.Model;
using Azrng.NTika.EncodingDetectors;
using FluentAssertions;
using Xunit;

namespace Azrng.NTika.EncodingDetectors.Test
{
    public class HtmlMetaEncodingDetectorTests
    {
        static HtmlMetaEncodingDetectorTests()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        [Fact]
        public void Detect_CharsetMeta_ShouldDetectEncoding()
        {
            var detector = new HtmlMetaEncodingDetector();
            var html = "<html><head><meta charset=\"utf-8\"></head><body></body></html>";
            var data = Encoding.ASCII.GetBytes(html);
            using var stream = new MemoryStream(data);
            var result = detector.Detect(stream, new Metadata(), new ParseContext());

            result.Should().NotBeNull();
            result!.Encoding.WebName.Should().Be("utf-8");
        }

        [Fact]
        public void Detect_HttpEquivMeta_ShouldDetectEncoding()
        {
            var detector = new HtmlMetaEncodingDetector();
            var html = "<html><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=gb2312\"></head><body></body></html>";
            var data = Encoding.ASCII.GetBytes(html);
            using var stream = new MemoryStream(data);
            var result = detector.Detect(stream, new Metadata(), new ParseContext());

            result.Should().NotBeNull();
            result!.Encoding.WebName.Should().Be("gb2312");
        }

        [Fact]
        public void Detect_NoMeta_ShouldReturnNull()
        {
            var detector = new HtmlMetaEncodingDetector();
            var html = "<html><head><title>Test</title></head><body></body></html>";
            var data = Encoding.ASCII.GetBytes(html);
            using var stream = new MemoryStream(data);
            var result = detector.Detect(stream, new Metadata(), new ParseContext());

            result.Should().BeNull();
        }
    }
}

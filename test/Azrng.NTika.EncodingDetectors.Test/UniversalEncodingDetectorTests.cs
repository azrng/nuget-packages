using System.Text;
using Azrng.NTika.Core.Model;
using Azrng.NTika.EncodingDetectors;
using FluentAssertions;
using Xunit;

namespace Azrng.NTika.EncodingDetectors.Test
{
    public class UniversalEncodingDetectorTests
    {
        static UniversalEncodingDetectorTests()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        [Fact]
        public void Detect_AsciiText_ShouldDetectUtf8()
        {
            var detector = new UniversalEncodingDetector();
            var data = Encoding.UTF8.GetBytes("Hello, world! This is a test.");
            using var stream = new MemoryStream(data);
            var result = detector.Detect(stream, new Metadata(), new ParseContext());

            result.Should().NotBeNull();
            result!.Encoding.Should().NotBeNull();
        }

        [Fact]
        public void Detect_ChineseGb2312_ShouldDetectEncoding()
        {
            var detector = new UniversalEncodingDetector();
            var encoding = Encoding.GetEncoding("GB2312");
            var data = encoding.GetBytes("你好世界，这是一段中文测试文本。");
            using var stream = new MemoryStream(data);
            var result = detector.Detect(stream, new Metadata(), new ParseContext());

            result.Should().NotBeNull();
            result!.Encoding.Should().NotBeNull();
        }

        [Fact]
        public void Detect_EmptyStream_ShouldReturnNull()
        {
            var detector = new UniversalEncodingDetector();
            using var stream = new MemoryStream(Array.Empty<byte>());
            var result = detector.Detect(stream, new Metadata(), new ParseContext());

            result.Should().BeNull();
        }
    }
}

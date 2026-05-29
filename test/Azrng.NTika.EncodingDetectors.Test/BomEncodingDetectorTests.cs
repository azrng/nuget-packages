using System.Text;
using Azrng.NTika.Core.Model;
using Azrng.NTika.EncodingDetectors;
using FluentAssertions;
using Xunit;

namespace Azrng.NTika.EncodingDetectors.Test
{
    public class BomEncodingDetectorTests
    {
        [Fact]
        public void Detect_Utf8Bom_ShouldReturnUtf8()
        {
            var detector = new BomEncodingDetector();
            var data = new byte[] { 0xEF, 0xBB, 0xBF, 0x48, 0x65, 0x6C, 0x6C, 0x6F };
            using var stream = new MemoryStream(data);
            var result = detector.Detect(stream, new Metadata(), new ParseContext());

            result.Should().NotBeNull();
            result!.Encoding.Should().Be(Encoding.UTF8);
            result.Confidence.Should().Be(EncodingConfidence.HIGH);
        }

        [Fact]
        public void Detect_Utf16LeBom_ShouldReturnUnicode()
        {
            var detector = new BomEncodingDetector();
            var data = new byte[] { 0xFF, 0xFE, 0x48, 0x00 };
            using var stream = new MemoryStream(data);
            var result = detector.Detect(stream, new Metadata(), new ParseContext());

            result.Should().NotBeNull();
            result!.Encoding.Should().Be(Encoding.Unicode);
        }

        [Fact]
        public void Detect_Utf16BeBom_ShouldReturnBigEndianUnicode()
        {
            var detector = new BomEncodingDetector();
            var data = new byte[] { 0xFE, 0xFF, 0x00, 0x48 };
            using var stream = new MemoryStream(data);
            var result = detector.Detect(stream, new Metadata(), new ParseContext());

            result.Should().NotBeNull();
            result!.Encoding.Should().Be(Encoding.BigEndianUnicode);
        }

        [Fact]
        public void Detect_NoBom_ShouldReturnNull()
        {
            var detector = new BomEncodingDetector();
            var data = Encoding.UTF8.GetBytes("Hello, world!");
            using var stream = new MemoryStream(data);
            var result = detector.Detect(stream, new Metadata(), new ParseContext());

            result.Should().BeNull();
        }

        [Fact]
        public void Detect_EmptyStream_ShouldReturnNull()
        {
            var detector = new BomEncodingDetector();
            using var stream = new MemoryStream(Array.Empty<byte>());
            var result = detector.Detect(stream, new Metadata(), new ParseContext());

            result.Should().BeNull();
        }
    }
}

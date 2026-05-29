using System.Text;
using Azrng.NTika.Core.Model;
using Azrng.NTika.EncodingDetectors;
using FluentAssertions;
using Xunit;

namespace Azrng.NTika.EncodingDetectors.Test
{
    public class CompositeEncodingDetectorTests
    {
        [Fact]
        public void Detect_BomDetectedFirst_ShouldReturnBomResult()
        {
            var composite = new CompositeEncodingDetector(
                new BomEncodingDetector(),
                new UniversalEncodingDetector());

            var data = new byte[] { 0xEF, 0xBB, 0xBF, 0x48, 0x65, 0x6C, 0x6C, 0x6F };
            using var stream = new MemoryStream(data);
            var result = composite.Detect(stream, new Metadata(), new ParseContext());

            result.Should().NotBeNull();
            result!.Encoding.Should().Be(Encoding.UTF8);
            result.Confidence.Should().Be(EncodingConfidence.HIGH);
        }

        [Fact]
        public void Detect_NoBom_ShouldFallbackToUniversal()
        {
            var composite = new CompositeEncodingDetector(
                new BomEncodingDetector(),
                new UniversalEncodingDetector());

            var data = Encoding.UTF8.GetBytes("Hello, world! This is a test.");
            using var stream = new MemoryStream(data);
            var result = composite.Detect(stream, new Metadata(), new ParseContext());

            result.Should().NotBeNull();
        }
    }
}

using Xunit;
using System.Text;
using Azrng.NTika.Core.Detect;
using Azrng.NTika.Core.IO;
using Azrng.NTika.Core.Model;
using FluentAssertions;

namespace Azrng.NTika.Core.Test.Detect
{
    public class TextDetectorTests
    {
        private readonly TextDetector _detector = new();

        [Fact]
        public void Detect_PlainText_ShouldReturnTextPlain()
        {
            var data = Encoding.UTF8.GetBytes("Hello, world!");
            using var tis = TikaInputStream.Get(data);
            var result = _detector.Detect(tis, new Metadata(), new ParseContext());
            result.Should().Be(MediaType.TextPlain);
        }

        [Fact]
        public void Detect_EmptyFile_ShouldReturnEmpty()
        {
            var data = Array.Empty<byte>();
            using var tis = TikaInputStream.Get(data);
            var result = _detector.Detect(tis, new Metadata(), new ParseContext());
            result.Should().Be(MediaType.Empty);
        }

        [Fact]
        public void Detect_NullStream_ShouldReturnOctetStream()
        {
            var result = _detector.Detect(null, new Metadata(), new ParseContext());
            result.Should().Be(MediaType.OctetStream);
        }

        [Fact]
        public void Detect_BinaryData_ShouldReturnOctetStream()
        {
            var data = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04 };
            using var tis = TikaInputStream.Get(data);
            var result = _detector.Detect(tis, new Metadata(), new ParseContext());
            result.Should().Be(MediaType.OctetStream);
        }
    }
}

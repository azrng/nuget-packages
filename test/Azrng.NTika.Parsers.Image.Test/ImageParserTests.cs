using Azrng.NTika.Core.IO;
using Azrng.NTika.Core.Model;
using Azrng.NTika.Core.Sax;
using FluentAssertions;
using Xunit;

namespace Azrng.NTika.Parsers.Image.Test
{
    public class ImageParserTests
    {
        [Fact]
        public void GetSupportedTypes_ShouldReturnImageTypes()
        {
            var parser = new ImageParser();
            var types = parser.GetSupportedTypes(new ParseContext());
            types.Should().HaveCount(6);
        }

        [Fact]
        public void Parse_PngImage_ShouldExtractMetadata()
        {
            var parser = new ImageParser();
            var pngBytes = CreateMinimalPng();
            using var stream = TikaInputStream.Get(pngBytes);
            var handler = new WriteOutContentHandler();
            var metadata = new Metadata();
            var context = new ParseContext();

            parser.Parse(stream, handler, metadata, context);

            // Should not throw and should set content type
            metadata.Get(TikaCoreProperties.CONTENT_TYPE).Should().Be("image/png");
        }

        [Fact]
        public void Parse_BmpImage_ShouldDetectType()
        {
            var parser = new ImageParser();
            var bmpBytes = CreateMinimalBmp();
            using var stream = TikaInputStream.Get(bmpBytes);
            var handler = new WriteOutContentHandler();
            var metadata = new Metadata();
            var context = new ParseContext();

            parser.Parse(stream, handler, metadata, context);

            metadata.Get(TikaCoreProperties.CONTENT_TYPE).Should().Be("image/bmp");
        }

        [Fact]
        public void Parse_UnknownFormat_ShouldNotThrow()
        {
            var parser = new ImageParser();
            var data = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05 };
            using var stream = TikaInputStream.Get(data);
            var handler = new WriteOutContentHandler();
            var metadata = new Metadata();
            var context = new ParseContext();

            var act = () => parser.Parse(stream, handler, metadata, context);
            act.Should().NotThrow();
        }

        private static byte[] CreateMinimalPng()
        {
            // Minimal valid PNG: 1x1 white pixel
            return new byte[]
            {
                0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, // PNG signature
                0x00, 0x00, 0x00, 0x0D, // IHDR length
                0x49, 0x48, 0x44, 0x52, // IHDR
                0x00, 0x00, 0x00, 0x01, // width: 1
                0x00, 0x00, 0x00, 0x01, // height: 1
                0x08, 0x02,             // bit depth: 8, color type: 2 (RGB)
                0x00, 0x00, 0x00,       // compression, filter, interlace
                0x90, 0x77, 0x53, 0xDE, // CRC
                0x00, 0x00, 0x00, 0x0C, // IDAT length
                0x49, 0x44, 0x41, 0x54, // IDAT
                0x08, 0xD7, 0x63, 0xF8, 0xCF, 0xC0, 0x00, 0x00,
                0x01, 0x01, 0x01, 0x00,
                0x18, 0xDD, 0x8D, 0xB4, // CRC
                0x00, 0x00, 0x00, 0x00, // IEND length
                0x49, 0x45, 0x4E, 0x44, // IEND
                0xAE, 0x42, 0x60, 0x82  // CRC
            };
        }

        private static byte[] CreateMinimalBmp()
        {
            // Minimal BMP header
            return new byte[]
            {
                0x42, 0x4D,             // BMP signature
                0x36, 0x00, 0x00, 0x00, // file size: 54
                0x00, 0x00, 0x00, 0x00, // reserved
                0x36, 0x00, 0x00, 0x00, // pixel data offset: 54
                0x28, 0x00, 0x00, 0x00, // header size: 40
                0x01, 0x00, 0x00, 0x00, // width: 1
                0x01, 0x00, 0x00, 0x00, // height: 1
                0x01, 0x00,             // planes: 1
                0x18, 0x00,             // bits per pixel: 24
                0x00, 0x00, 0x00, 0x00, // compression: none
                0x00, 0x00, 0x00, 0x00, // image size
                0x00, 0x00, 0x00, 0x00, // x pixels per meter
                0x00, 0x00, 0x00, 0x00, // y pixels per meter
                0x00, 0x00, 0x00, 0x00, // colors used
                0x00, 0x00, 0x00, 0x00, // important colors
                0xFF, 0xFF, 0xFF, 0x00  // pixel data (white, padded)
            };
        }
    }
}

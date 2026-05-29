using System.Text;
using Azrng.NTika.Core.Detect;
using Azrng.NTika.Core.IO;
using Azrng.NTika.Core.Model;
using FluentAssertions;
using Xunit;

namespace Azrng.NTika.Core.Test
{
    public class MagicDetectorTests
    {
        private readonly MagicDetector _detector = new();
        private readonly Metadata _metadata = new();
        private readonly ParseContext _context = new();

        [Fact]
        public void Detect_PdfByMagicBytes_ShouldReturnPdf()
        {
            var pdfHeader = new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2D, 0x31, 0x2E, 0x34 };
            using var stream = TikaInputStream.Get(pdfHeader);
            var result = _detector.Detect(stream, _metadata, _context);
            result.ToString().Should().Be("application/pdf");
        }

        [Fact]
        public void Detect_ZipByMagicBytes_ShouldReturnZip()
        {
            var zipHeader = new byte[] { 0x50, 0x4B, 0x03, 0x04, 0x14, 0x00, 0x00, 0x00 };
            using var stream = TikaInputStream.Get(zipHeader);
            var result = _detector.Detect(stream, _metadata, _context);
            result.ToString().Should().Be("application/zip");
        }

        [Fact]
        public void Detect_PngByMagicBytes_ShouldReturnPng()
        {
            var pngHeader = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D };
            using var stream = TikaInputStream.Get(pngHeader);
            var result = _detector.Detect(stream, _metadata, _context);
            result.ToString().Should().Be("image/png");
        }

        [Fact]
        public void Detect_JpegByMagicBytes_ShouldReturnJpeg()
        {
            var jpegHeader = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46 };
            using var stream = TikaInputStream.Get(jpegHeader);
            var result = _detector.Detect(stream, _metadata, _context);
            result.ToString().Should().Be("image/jpeg");
        }

        [Fact]
        public void Detect_GzipByMagicBytes_ShouldReturnGzip()
        {
            var gzipHeader = new byte[] { 0x1F, 0x8B, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00 };
            using var stream = TikaInputStream.Get(gzipHeader);
            var result = _detector.Detect(stream, _metadata, _context);
            result.ToString().Should().Be("application/gzip");
        }

        [Fact]
        public void Detect_Mp3ById3Tag_ShouldReturnAudioMpeg()
        {
            var mp3Header = new byte[] { 0x49, 0x44, 0x33, 0x03, 0x00, 0x00, 0x00, 0x00 };
            using var stream = TikaInputStream.Get(mp3Header);
            var result = _detector.Detect(stream, _metadata, _context);
            result.ToString().Should().Be("audio/mpeg");
        }

        [Fact]
        public void Detect_RtfByMagicBytes_ShouldReturnRtf()
        {
            var rtfHeader = Encoding.ASCII.GetBytes(@"{\rtf1\ansi");
            using var stream = TikaInputStream.Get(rtfHeader);
            var result = _detector.Detect(stream, _metadata, _context);
            result.ToString().Should().Be("application/rtf");
        }

        [Fact]
        public void Detect_XmlByMagicBytes_ShouldReturnXml()
        {
            var xmlHeader = Encoding.UTF8.GetBytes("<?xml version=\"1.0\"?>");
            using var stream = TikaInputStream.Get(xmlHeader);
            var result = _detector.Detect(stream, _metadata, _context);
            result.ToString().Should().Be("application/xml");
        }

        [Fact]
        public void Detect_UnknownBytes_ShouldReturnOctetStream()
        {
            var unknown = new byte[] { 0x00, 0x01, 0x02, 0x03 };
            using var stream = TikaInputStream.Get(unknown);
            var result = _detector.Detect(stream, _metadata, _context);
            result.ToString().Should().Be("application/octet-stream");
        }

        [Fact]
        public void Detect_EmptyStream_ShouldReturnOctetStream()
        {
            var empty = new byte[0];
            using var stream = TikaInputStream.Get(empty);
            var result = _detector.Detect(stream, _metadata, _context);
            result.ToString().Should().Be("application/octet-stream");
        }
    }
}

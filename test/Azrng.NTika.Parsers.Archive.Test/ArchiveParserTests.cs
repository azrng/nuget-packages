using System.IO.Compression;
using Azrng.NTika.Core.Config;
using Azrng.NTika.Core.Exception;
using Azrng.NTika.Core.IO;
using Azrng.NTika.Core.Model;
using Azrng.NTika.Core.Sax;
using FluentAssertions;
using Xunit;

namespace Azrng.NTika.Parsers.Archive.Test
{
    public class ArchiveParserTests
    {
        [Fact]
        public void GetSupportedTypes_ShouldReturnArchiveTypes()
        {
            var parser = new ArchiveParser();
            var types = parser.GetSupportedTypes(new ParseContext());
            types.Should().HaveCount(5);
        }

        [Fact]
        public void Parse_ZipArchive_ShouldListEntries()
        {
            var parser = new ArchiveParser();
            var zipBytes = CreateTestZip();
            using var stream = TikaInputStream.Get(zipBytes);
            var handler = new WriteOutContentHandler();
            var metadata = new Metadata();
            metadata.Set(TikaCoreProperties.CONTENT_TYPE, "application/zip");
            var context = new ParseContext();

            parser.Parse(stream, handler, metadata, context);

            var text = handler.ToString();
            text.Should().Contain("test.txt");
            text.Should().Contain("readme.md");
        }

        [Fact]
        public void Parse_ZipArchive_ShouldSetMetadata()
        {
            var parser = new ArchiveParser();
            var zipBytes = CreateTestZip();
            using var stream = TikaInputStream.Get(zipBytes);
            var handler = new WriteOutContentHandler();
            var metadata = new Metadata();
            metadata.Set(TikaCoreProperties.CONTENT_TYPE, "application/zip");
            var context = new ParseContext();

            parser.Parse(stream, handler, metadata, context);

            metadata.Get("entryCount").Should().Be("2");
        }

        [Fact]
        public void Parse_UnknownFormat_ShouldNotThrow()
        {
            var parser = new ArchiveParser();
            var data = new byte[] { 0x00, 0x01, 0x02, 0x03 };
            using var stream = TikaInputStream.Get(data);
            var handler = new WriteOutContentHandler();
            var metadata = new Metadata();
            var context = new ParseContext();

            var act = () => parser.Parse(stream, handler, metadata, context);
            act.Should().NotThrow();
        }

        [Fact]
        public void Parse_WhenEntryCountExceedsLimit_ShouldThrow()
        {
            var parser = new ArchiveParser();
            var zipBytes = CreateTestZip();
            using var stream = TikaInputStream.Get(zipBytes);
            var handler = new WriteOutContentHandler();
            var metadata = new Metadata();
            metadata.Set(TikaCoreProperties.CONTENT_TYPE, "application/zip");
            var context = new ParseContext();
            context.Set(new EmbeddedLimits { MaxEmbeddedCount = 1 });

            var act = () => parser.Parse(stream, handler, metadata, context);
            act.Should().Throw<EmbeddedLimitReachedException>();
        }

        [Fact]
        public void Parse_WhenEmbeddedEntryExceedsByteLimit_ShouldThrow()
        {
            var parser = new ArchiveParser();
            var zipBytes = CreateTestZip();
            using var stream = TikaInputStream.Get(zipBytes);
            var handler = new WriteOutContentHandler();
            var metadata = new Metadata();
            metadata.Set(TikaCoreProperties.CONTENT_TYPE, "application/zip");
            var context = new ParseContext();
            context.Set(new EmbeddedLimits { MaxEmbeddedBytes = 1 });

            var act = () => parser.Parse(stream, handler, metadata, context);
            act.Should().Throw<EmbeddedLimitReachedException>();
        }

        private static byte[] CreateTestZip()
        {
            using var ms = new MemoryStream();
            using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
            {
                var entry1 = archive.CreateEntry("test.txt");
                using (var writer = new StreamWriter(entry1.Open()))
                {
                    writer.Write("Hello World");
                }

                var entry2 = archive.CreateEntry("readme.md");
                using (var writer = new StreamWriter(entry2.Open()))
                {
                    writer.Write("# README");
                }
            }
            return ms.ToArray();
        }
    }
}

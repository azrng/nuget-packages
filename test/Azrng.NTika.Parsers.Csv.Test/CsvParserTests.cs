using System.Text;
using Azrng.NTika.Core.IO;
using Azrng.NTika.Core.Model;
using Azrng.NTika.Core.Sax;
using FluentAssertions;
using Xunit;

namespace Azrng.NTika.Parsers.Csv.Test
{
    public class CsvParserTests
    {
        [Fact]
        public void GetSupportedTypes_ShouldReturnCsvAndTsv()
        {
            var parser = new CsvParser();
            var types = parser.GetSupportedTypes(new ParseContext());
            types.Should().HaveCount(2);
        }

        [Fact]
        public void Parse_SimpleCsv_ShouldExtractTable()
        {
            var parser = new CsvParser();
            var csv = "Name,Age,City\nAlice,30,Beijing\nBob,25,Shanghai";
            var data = Encoding.UTF8.GetBytes(csv);
            using var stream = TikaInputStream.Get(data);
            var handler = new WriteOutContentHandler();
            var metadata = new Metadata();
            var context = new ParseContext();

            parser.Parse(stream, handler, metadata, context);

            var text = handler.ToString();
            text.Should().Contain("Name");
            text.Should().Contain("Alice");
            text.Should().Contain("30");
            text.Should().Contain("Beijing");
            text.Should().Contain("Bob");
        }

        [Fact]
        public void Parse_ShouldSetMetadata()
        {
            var parser = new CsvParser();
            var csv = "A,B\n1,2\n3,4";
            var data = Encoding.UTF8.GetBytes(csv);
            using var stream = TikaInputStream.Get(data);
            var handler = new WriteOutContentHandler();
            var metadata = new Metadata();
            var context = new ParseContext();

            parser.Parse(stream, handler, metadata, context);

            metadata.Get("numColumns").Should().Be("2");
            metadata.Get("numRows").Should().Be("2");
        }

        [Fact]
        public void Parse_TsvFile_ShouldUseTabDelimiter()
        {
            var parser = new CsvParser();
            var tsv = "Name\tAge\nAlice\t30";
            var data = Encoding.UTF8.GetBytes(tsv);
            using var stream = TikaInputStream.Get(data);
            var handler = new WriteOutContentHandler();
            var metadata = new Metadata();
            metadata.Set(TikaCoreProperties.CONTENT_TYPE, "text/tsv");
            var context = new ParseContext();

            parser.Parse(stream, handler, metadata, context);

            var text = handler.ToString();
            text.Should().Contain("Name");
            text.Should().Contain("Alice");
            text.Should().Contain("30");
        }

        [Fact]
        public void Parse_TsvFile_ShouldNotMutateSharedCsvConfig()
        {
            var parser = new CsvParser();
            var tsv = "Name\tAge\nAlice\t30";
            var data = Encoding.UTF8.GetBytes(tsv);
            using var stream = TikaInputStream.Get(data);
            var handler = new WriteOutContentHandler();
            var metadata = new Metadata();
            metadata.Set(TikaCoreProperties.CONTENT_TYPE, "text/tsv");
            var config = new CsvConfig();
            var context = new ParseContext();
            context.Set(config);

            parser.Parse(stream, handler, metadata, context);

            config.Delimiter.Should().Be(',');
        }

        [Fact]
        public void Parse_EmptyCsv_ShouldNotThrow()
        {
            var parser = new CsvParser();
            var data = Encoding.UTF8.GetBytes("A,B\n");
            using var stream = TikaInputStream.Get(data);
            var handler = new WriteOutContentHandler();
            var metadata = new Metadata();
            var context = new ParseContext();

            var act = () => parser.Parse(stream, handler, metadata, context);
            act.Should().NotThrow();
        }

        [Fact]
        public void Parse_CsvWithSpecialChars_ShouldPreserveContent()
        {
            var parser = new CsvParser();
            var csv = "Name,Description\nTest,\"Hello, World\"";
            var data = Encoding.UTF8.GetBytes(csv);
            using var stream = TikaInputStream.Get(data);
            var handler = new WriteOutContentHandler();
            var metadata = new Metadata();
            var context = new ParseContext();

            parser.Parse(stream, handler, metadata, context);

            var text = handler.ToString();
            text.Should().Contain("Hello, World");
        }
    }
}

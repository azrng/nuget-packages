using System.Text;
using Azrng.NTika.Core.IO;
using Azrng.NTika.Core.Model;
using Azrng.NTika.Core.Sax;
using FluentAssertions;
using Xunit;

namespace Azrng.NTika.Parsers.Data.Test
{
    public class JsonParserTests
    {
        [Fact]
        public void GetSupportedTypes_ShouldReturnJson()
        {
            var parser = new JsonParser();
            var types = parser.GetSupportedTypes(new ParseContext());
            types.Should().HaveCount(1);
        }

        [Fact]
        public void Parse_SimpleJson_ShouldExtractKeysAndValues()
        {
            var parser = new JsonParser();
            var json = """{"name": "John", "age": 30, "city": "New York"}""";
            var data = Encoding.UTF8.GetBytes(json);
            using var stream = TikaInputStream.Get(data);
            var handler = new WriteOutContentHandler();
            var metadata = new Metadata();
            var context = new ParseContext();

            parser.Parse(stream, handler, metadata, context);

            var text = handler.ToString();
            text.Should().Contain("name");
            text.Should().Contain("John");
            text.Should().Contain("age");
            text.Should().Contain("30");
            text.Should().Contain("city");
            text.Should().Contain("New York");
        }

        [Fact]
        public void Parse_NestedJson_ShouldExtractAllText()
        {
            var parser = new JsonParser();
            var json = """{"person": {"name": "Alice", "hobbies": ["reading", "coding"]}}""";
            var data = Encoding.UTF8.GetBytes(json);
            using var stream = TikaInputStream.Get(data);
            var handler = new WriteOutContentHandler();
            var metadata = new Metadata();
            var context = new ParseContext();

            parser.Parse(stream, handler, metadata, context);

            var text = handler.ToString();
            text.Should().Contain("person");
            text.Should().Contain("Alice");
            text.Should().Contain("reading");
            text.Should().Contain("coding");
        }

        [Fact]
        public void Parse_EmptyJson_ShouldNotThrow()
        {
            var parser = new JsonParser();
            var data = Encoding.UTF8.GetBytes("{}");
            using var stream = TikaInputStream.Get(data);
            var handler = new WriteOutContentHandler();
            var metadata = new Metadata();
            var context = new ParseContext();

            var act = () => parser.Parse(stream, handler, metadata, context);
            act.Should().NotThrow();
        }
    }

    public class YamlParserTests
    {
        [Fact]
        public void GetSupportedTypes_ShouldReturnYaml()
        {
            var parser = new YamlParser();
            var types = parser.GetSupportedTypes(new ParseContext());
            types.Should().HaveCount(2);
        }

        [Fact]
        public void Parse_SimpleYaml_ShouldExtractKeysAndValues()
        {
            var parser = new YamlParser();
            var yaml = """
                name: John
                age: 30
                city: New York
                """;
            var data = Encoding.UTF8.GetBytes(yaml);
            using var stream = TikaInputStream.Get(data);
            var handler = new WriteOutContentHandler();
            var metadata = new Metadata();
            var context = new ParseContext();

            parser.Parse(stream, handler, metadata, context);

            var text = handler.ToString();
            text.Should().Contain("name");
            text.Should().Contain("John");
            text.Should().Contain("age");
            text.Should().Contain("30");
            text.Should().Contain("city");
            text.Should().Contain("New York");
        }

        [Fact]
        public void Parse_NestedYaml_ShouldExtractAllText()
        {
            var parser = new YamlParser();
            var yaml = """
                person:
                  name: Alice
                  hobbies:
                    - reading
                    - coding
                """;
            var data = Encoding.UTF8.GetBytes(yaml);
            using var stream = TikaInputStream.Get(data);
            var handler = new WriteOutContentHandler();
            var metadata = new Metadata();
            var context = new ParseContext();

            parser.Parse(stream, handler, metadata, context);

            var text = handler.ToString();
            text.Should().Contain("person");
            text.Should().Contain("Alice");
            text.Should().Contain("reading");
            text.Should().Contain("coding");
        }

        [Fact]
        public void Parse_EmptyYaml_ShouldNotThrow()
        {
            var parser = new YamlParser();
            var data = Encoding.UTF8.GetBytes("");
            using var stream = TikaInputStream.Get(data);
            var handler = new WriteOutContentHandler();
            var metadata = new Metadata();
            var context = new ParseContext();

            var act = () => parser.Parse(stream, handler, metadata, context);
            act.Should().NotThrow();
        }
    }
}

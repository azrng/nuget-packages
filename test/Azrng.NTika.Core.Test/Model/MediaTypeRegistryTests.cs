using Xunit;
using Azrng.NTika.Core.Model;
using FluentAssertions;

namespace Azrng.NTika.Core.Test.Model
{
    public class MediaTypeRegistryTests
    {
        [Fact]
        public void AddType_ShouldWork()
        {
            var registry = new MediaTypeRegistry();
            var type = MediaType.TextPlain;
            registry.AddType(type);
            registry.Types.Should().Contain(type);
        }

        [Fact]
        public void AddAlias_ShouldResolve()
        {
            var registry = new MediaTypeRegistry();
            var canonical = MediaType.Parse("application/json")!;
            var alias = MediaType.Parse("text/x-json")!;
            registry.AddType(canonical);
            registry.AddAlias(canonical, alias);
            registry.Normalize(alias).Should().Be(canonical);
        }

        [Fact]
        public void GetSupertype_TextSubtype_ShouldReturnTextPlain()
        {
            var registry = new MediaTypeRegistry();
            var html = MediaType.TextHtml;
            var supertype = registry.GetSupertype(html);
            supertype.Should().Be(MediaType.TextPlain);
        }

        [Fact]
        public void GetSupertype_XmlSubtype_ShouldReturnApplicationXml()
        {
            var registry = new MediaTypeRegistry();
            var svg = MediaType.Parse("image/svg+xml")!;
            var supertype = registry.GetSupertype(svg);
            supertype.Should().Be(MediaType.ApplicationXml);
        }

        [Fact]
        public void IsSpecializationOf_HtmlToText_ShouldBeTrue()
        {
            var registry = new MediaTypeRegistry();
            registry.IsSpecializationOf(MediaType.TextHtml, MediaType.TextPlain).Should().BeTrue();
        }
    }
}

using Xunit;
using Azrng.NTika.Core.Model;
using FluentAssertions;

namespace Azrng.NTika.Core.Test.Model
{
    public class MediaTypeTests
    {
        [Fact]
        public void Parse_SimpleType_ShouldWork()
        {
            var type = MediaType.Parse("text/plain");
            type.Should().NotBeNull();
            type!.Type.Should().Be("text");
            type.Subtype.Should().Be("plain");
        }

        [Fact]
        public void Parse_WithParameters_ShouldWork()
        {
            var type = MediaType.Parse("text/plain; charset=utf-8");
            type.Should().NotBeNull();
            type!.Type.Should().Be("text");
            type.Subtype.Should().Be("plain");
            type.HasParameters.Should().BeTrue();
            type.Parameters.Should().ContainKey("charset");
            type.Parameters["charset"].Should().Be("utf-8");
        }

        [Fact]
        public void Parse_Null_ShouldReturnNull()
        {
            MediaType.Parse(null).Should().BeNull();
        }

        [Fact]
        public void Parse_Invalid_ShouldReturnNull()
        {
            MediaType.Parse("invalid").Should().BeNull();
        }

        [Fact]
        public void BaseType_WithParameters_ShouldStripParams()
        {
            var type = MediaType.Parse("text/plain; charset=utf-8")!;
            var baseType = type.BaseType;
            baseType.HasParameters.Should().BeFalse();
            baseType.ToString().Should().Be("text/plain");
        }

        [Fact]
        public void Equality_SameType_ShouldBeEqual()
        {
            var a = MediaType.Parse("text/plain");
            var b = MediaType.Parse("text/plain");
            a.Should().Be(b);
        }

        [Fact]
        public void Equality_DifferentType_ShouldNotBeEqual()
        {
            var a = MediaType.Parse("text/plain");
            var b = MediaType.Parse("text/html");
            a.Should().NotBe(b);
        }

        [Fact]
        public void Constants_ShouldExist()
        {
            MediaType.OctetStream.Should().NotBeNull();
            MediaType.TextPlain.Should().NotBeNull();
            MediaType.TextHtml.Should().NotBeNull();
            MediaType.ApplicationXml.Should().NotBeNull();
        }

        [Fact]
        public void Application_Factory_ShouldWork()
        {
            var type = MediaType.Application("pdf");
            type.Type.Should().Be("application");
            type.Subtype.Should().Be("pdf");
        }

        [Fact]
        public void CompareTo_ShouldWork()
        {
            var a = MediaType.Parse("application/pdf")!;
            var b = MediaType.Parse("text/plain")!;
            a.CompareTo(b).Should().BeLessThan(0);
        }
    }
}

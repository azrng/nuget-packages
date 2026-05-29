using Xunit;
using Azrng.NTika.Core.Model;
using FluentAssertions;

namespace Azrng.NTika.Core.Test.Model
{
    public class ParseContextTests
    {
        [Fact]
        public void SetAndGet_ShouldWork()
        {
            var context = new ParseContext();
            context.Set("test");
            context.Get<string>().Should().Be("test");
        }

        [Fact]
        public void Get_Missing_ShouldReturnNull()
        {
            var context = new ParseContext();
            context.Get<string>().Should().BeNull();
        }

        [Fact]
        public void Get_WithDefault_ShouldReturnDefault()
        {
            var context = new ParseContext();
            context.Get("default").Should().Be("default");
        }

        [Fact]
        public void IsEmpty_ShouldWork()
        {
            var context = new ParseContext();
            context.IsEmpty.Should().BeTrue();
            context.Set("test");
            context.IsEmpty.Should().BeFalse();
        }
    }
}

using Xunit;
using Azrng.NTika.Core.Exception;
using Azrng.NTika.Core.Model;
using Azrng.NTika.Core.Sax;
using FluentAssertions;

namespace Azrng.NTika.Core.Test.Sax
{
    public class WriteOutContentHandlerTests
    {
        [Fact]
        public void Characters_ShouldCapture()
        {
            var handler = new WriteOutContentHandler();
            handler.Characters("hello".ToCharArray(), 0, 5);
            handler.ToString().Should().Be("hello");
        }

        [Fact]
        public void WriteLimit_ShouldThrow()
        {
            var handler = new WriteOutContentHandler(5);
            handler.Characters("hello".ToCharArray(), 0, 5);
            var act = () => handler.Characters("!".ToCharArray(), 0, 1);
            act.Should().Throw<WriteLimitReachedException>();
        }

        [Fact]
        public void NoLimit_ShouldNotThrow()
        {
            var handler = new WriteOutContentHandler();
            handler.Characters(new string('a', 100000).ToCharArray(), 0, 100000);
            handler.ToString().Length.Should().Be(100000);
        }
    }
}

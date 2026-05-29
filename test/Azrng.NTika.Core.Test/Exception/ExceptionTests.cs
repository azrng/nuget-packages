using Xunit;
using Azrng.NTika.Core.Exception;
using FluentAssertions;

namespace Azrng.NTika.Core.Test.Exception
{
    public class ExceptionTests
    {
        [Fact]
        public void TikaException_ShouldBeBaseException()
        {
            var ex = new TikaException("test");
            ex.Should().BeAssignableTo<System.Exception>();
            ex.Message.Should().Be("test");
        }

        [Fact]
        public void WriteLimitReachedException_ShouldExtendTikaException()
        {
            var ex = new WriteLimitReachedException(100);
            ex.Should().BeAssignableTo<TikaException>();
            ex.WriteLimit.Should().Be(100);
        }

        [Fact]
        public void ZeroByteFileException_ShouldExtendTikaException()
        {
            var ex = new ZeroByteFileException();
            ex.Should().BeAssignableTo<TikaException>();
        }

        [Fact]
        public void EncryptedDocumentException_ShouldExtendTikaException()
        {
            var ex = new EncryptedDocumentException("encrypted");
            ex.Should().BeAssignableTo<TikaException>();
        }

        [Fact]
        public void WriteLimitReached_IsWriteLimitReached_ShouldWork()
        {
            var ex = new WriteLimitReachedException(100);
            WriteLimitReachedException.IsWriteLimitReached(ex).Should().BeTrue();
            WriteLimitReachedException.IsWriteLimitReached(null).Should().BeFalse();
        }
    }
}

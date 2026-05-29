using Xunit;
using System.Text;
using Azrng.NTika.Core.IO;
using Azrng.NTika.Core.Model;
using FluentAssertions;

namespace Azrng.NTika.Core.Test.IO
{
    public class TikaInputStreamTests
    {
        [Fact]
        public void Get_FromBytes_ShouldWork()
        {
            var data = Encoding.UTF8.GetBytes("hello");
            using var tis = TikaInputStream.Get(data);
            tis.CanRead.Should().BeTrue();
            tis.Length.Should().Be(5);
        }

        [Fact]
        public void Get_FromStream_ShouldWork()
        {
            var data = Encoding.UTF8.GetBytes("hello");
            using var stream = new MemoryStream(data);
            using var tis = TikaInputStream.Get(stream);
            tis.CanRead.Should().BeTrue();
        }

        [Fact]
        public void Read_ShouldWork()
        {
            var data = Encoding.UTF8.GetBytes("hello");
            using var tis = TikaInputStream.Get(data);
            var buffer = new byte[5];
            var read = tis.Read(buffer, 0, 5);
            read.Should().Be(5);
            Encoding.UTF8.GetString(buffer).Should().Be("hello");
        }

        [Fact]
        public void Rewind_ShouldResetPosition()
        {
            var data = Encoding.UTF8.GetBytes("hello");
            using var tis = TikaInputStream.Get(data);
            tis.Read(new byte[3], 0, 3);
            tis.Rewind();
            tis.Position.Should().Be(0);
        }

        [Fact]
        public void Get_WithMetadata_ShouldSetContentLength()
        {
            var data = Encoding.UTF8.GetBytes("hello");
            var metadata = new Metadata();
            using var tis = TikaInputStream.Get(data, metadata);
            metadata.Get("Content-Length").Should().Be("5");
        }

        [Fact]
        public void Get_Twice_ShouldReturnSameInstance()
        {
            var data = Encoding.UTF8.GetBytes("hello");
            using var tis = TikaInputStream.Get(data);
            var tis2 = TikaInputStream.Get(tis);
            tis2.Should().BeSameAs(tis);
        }
    }
}

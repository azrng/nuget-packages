using System.Text;
using Azrng.NMaxCompute.Tunnel.Wire;
using Xunit;

namespace Azrng.NMaxCompute.Test;

public class Crc32Test
{
    [Theory]
    [InlineData("", 0x00000000u)]
    [InlineData("a", 0xE8B7BE43u)]
    [InlineData("123", 0x884863D2u)]
    [InlineData("123456789", 0xCBF43926u)]
    [InlineData("The quick brown fox jumps over the lazy dog", 0x414FA339u)]
    public void Value_MatchesZlibCrc32(string input, uint expected)
    {
        var crc = new Crc32();
        crc.Update(Encoding.UTF8.GetBytes(input));
        Assert.Equal(expected, crc.GetValue());
    }

    [Fact]
    public void Reset_ReturnsToZero()
    {
        var crc = new Crc32();
        crc.Update(Encoding.UTF8.GetBytes("hello"));
        crc.Reset();
        Assert.Equal(0u, crc.GetValue());
    }

    [Fact]
    public void ChunkedUpdate_EqualsSingleUpdate()
    {
        var data = Encoding.UTF8.GetBytes("123456789");

        var chunked = new Crc32();
        chunked.Update(data.AsSpan(0, 3));
        chunked.Update(data.AsSpan(3, 3));
        chunked.Update(data.AsSpan(6, 3));

        var single = new Crc32();
        single.Update(data);

        Assert.Equal(single.GetValue(), chunked.GetValue());
    }
}

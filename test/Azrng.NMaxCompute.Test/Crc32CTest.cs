using System.Text;
using Azrng.NMaxCompute.Tunnel.Wire;
using Xunit;

namespace Azrng.NMaxCompute.Test;

public class Crc32CTest
{
    [Theory]
    [InlineData("", 0x00000000u)]
    [InlineData("123456789", 0xE3069283u)]
    public void Value_MatchesCastagnoli(string input, uint expected)
    {
        var crc = new Crc32C();
        crc.Update(Encoding.UTF8.GetBytes(input));
        Assert.Equal(expected, crc.GetValue());
    }

    [Fact]
    public void Reset_ReturnsToZero()
    {
        var crc = new Crc32C();
        crc.Update(Encoding.UTF8.GetBytes("hello"));
        crc.Reset();
        Assert.Equal(0u, crc.GetValue());
    }

    [Fact]
    public void ChunkedUpdate_EqualsSingleUpdate()
    {
        var data = Encoding.UTF8.GetBytes("123456789");

        var chunked = new Crc32C();
        chunked.Update(data.AsSpan(0, 3));
        chunked.Update(data.AsSpan(3, 3));
        chunked.Update(data.AsSpan(6, 3));

        var single = new Crc32C();
        single.Update(data);

        Assert.Equal(single.GetValue(), chunked.GetValue());
    }
}

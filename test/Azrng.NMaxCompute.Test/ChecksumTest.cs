using Azrng.NMaxCompute.Tunnel.Wire;
using Xunit;

namespace Azrng.NMaxCompute.Test;

public class ChecksumTest
{
    [Fact]
    public void UpdateBool_True_MarkerIs1()
    {
        var cs = new Checksum();
        cs.UpdateBool(true);

        var raw = new Crc32C();
        raw.Update(1);
        Assert.Equal(raw.GetValue(), cs.GetValue());
    }

    [Fact]
    public void UpdateInt_LittleEndian()
    {
        var cs = new Checksum();
        cs.UpdateInt(0x12345678);

        var raw = new Crc32C();
        raw.Update(new byte[] { 0x78, 0x56, 0x34, 0x12 });
        Assert.Equal(raw.GetValue(), cs.GetValue());
    }

    [Fact]
    public void UpdateLong_LittleEndian()
    {
        var cs = new Checksum();
        cs.UpdateLong(0x123456789ABCDEF0L);

        var raw = new Crc32C();
        raw.Update(new byte[] { 0xF0, 0xDE, 0xBC, 0x9A, 0x78, 0x56, 0x34, 0x12 });
        Assert.Equal(raw.GetValue(), cs.GetValue());
    }

    [Fact]
    public void UpdateDouble_LittleEndian()
    {
        var cs = new Checksum();
        cs.UpdateDouble(1.0);

        var raw = new Crc32C();
        raw.Update(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF0, 0x3F });
        Assert.Equal(raw.GetValue(), cs.GetValue());
    }

    [Fact]
    public void Reset_ClearsValue()
    {
        var cs = new Checksum();
        cs.UpdateInt(42);
        Assert.NotEqual(0u, cs.GetValue());

        cs.Reset();
        Assert.Equal(0u, cs.GetValue());
    }
}

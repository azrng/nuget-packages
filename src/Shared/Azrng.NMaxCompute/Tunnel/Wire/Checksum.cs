using System.Buffers.Binary;

namespace Azrng.NMaxCompute.Tunnel.Wire;

/// <summary>
/// Tunnel 协议的字段级 checksum 包装器。
/// <para>对应 PyODPS <c>odps/tunnel/checksum.py::Checksum</c>，默认使用 CRC32C。</para>
/// <para>每种字段按其 wire 表示（小端字节序）累积到内部 CRC。</para>
/// </summary>
public sealed class Checksum
{
    public const byte TrueMarker = 1;
    public const byte FalseMarker = 0;

    private readonly Crc32C _crc = new();

    public Checksum(string method = "crc32c")
    {
        if (!string.Equals(method, "crc32c", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Only crc32c is supported.", nameof(method));
        }
    }

    public void UpdateBool(bool value) => _crc.Update(value ? TrueMarker : FalseMarker);

    public void UpdateInt(int value)
    {
        Span<byte> buf = stackalloc byte[4];
        BinaryPrimitives.WriteInt32LittleEndian(buf, value);
        _crc.Update(buf);
    }

    public void UpdateLong(long value)
    {
        Span<byte> buf = stackalloc byte[8];
        BinaryPrimitives.WriteInt64LittleEndian(buf, value);
        _crc.Update(buf);
    }

    public void UpdateFloat(float value)
    {
        var raw = BitConverter.SingleToUInt32Bits(value);
        Span<byte> buf = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32LittleEndian(buf, raw);
        _crc.Update(buf);
    }

    public void UpdateDouble(double value)
    {
        var raw = BitConverter.DoubleToUInt64Bits(value);
        Span<byte> buf = stackalloc byte[8];
        BinaryPrimitives.WriteUInt64LittleEndian(buf, raw);
        _crc.Update(buf);
    }

    public void Update(ReadOnlySpan<byte> buffer) => _crc.Update(buffer);

    public void Reset() => _crc.Reset();

    public uint GetValue() => _crc.GetValue();
}

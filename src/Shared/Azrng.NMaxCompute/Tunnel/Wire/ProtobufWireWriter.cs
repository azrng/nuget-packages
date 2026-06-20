using System.Buffers.Binary;
using System.Text;

namespace Azrng.NMaxCompute.Tunnel.Wire;

/// <summary>
/// Tunnel 协议的 protobuf wire 编码器（写侧）。
/// <para>对应 PyODPS <c>odps/tunnel/pb/output_stream.py</c>，是 <see cref="ProtobufWireReader"/> 的逆操作。</para>
/// </summary>
public sealed class ProtobufWireWriter
{
    private readonly MemoryStream _ms = new();

    public long Length => _ms.Length;

    /// <summary>写出的字节。</summary>
    public byte[] ToArray() => _ms.ToArray();

    /// <summary>重置（复用实例）。</summary>
    public void Reset() => _ms.SetLength(0);

    public void WriteVarUInt32(uint v) => WriteVarUInt64(v);

    public void WriteVarUInt64(ulong v)
    {
        while (v > 0x7F)
        {
            _ms.WriteByte((byte)(v | 0x80));
            v >>= 7;
        }
        _ms.WriteByte((byte)v);
    }

    /// <summary>有符号 32 位，zigzag + varint（与 reader ReadSInt32 互逆）。</summary>
    public void WriteSInt32(int v)
    {
        var zz = (uint)((v << 1) ^ (v >> 31));
        WriteVarUInt32(zz);
    }

    /// <summary>有符号 64 位，zigzag + varint（与 reader ReadSInt64 互逆）。</summary>
    public void WriteSInt64(long v)
    {
        var zz = (ulong)((v << 1) ^ (v >> 63));
        WriteVarUInt64(zz);
    }

    /// <summary>有符号 32 位 varint（非 zigzag；用于 record 计数等）。</summary>
    public void WriteInt32(long v)
    {
        if (v < 0) v += 1L << 64;
        WriteVarUInt64((ulong)v);
    }

    public void WriteFixed32(uint v)
    {
        Span<byte> buf = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32LittleEndian(buf, v);
        _ms.Write(buf);
    }

    public void WriteFixed64(ulong v)
    {
        Span<byte> buf = stackalloc byte[8];
        BinaryPrimitives.WriteUInt64LittleEndian(buf, v);
        _ms.Write(buf);
    }

    public void WriteBool(bool v) => WriteVarUInt32(v ? 1u : 0u);

    /// <summary>length-delimited：varint(长度) + 字节。</summary>
    public void WriteBytes(byte[] bytes)
    {
        WriteVarUInt32((uint)bytes.Length);
        _ms.Write(bytes, 0, bytes.Length);
    }

    public void WriteString(string s) => WriteBytes(Encoding.UTF8.GetBytes(s));

    /// <summary>(field_number &lt;&lt; 3 | wire_type) 作为 varint 写入。</summary>
    public void WriteTag(int fieldNumber, int wireType) => WriteVarUInt32((uint)(((ulong)fieldNumber << 3) | (uint)wireType));
}

using System.Buffers.Binary;

namespace Azrng.NMaxCompute.Tunnel.Wire;

/// <summary>
/// Tunnel 协议的 protobuf wire 解码器。
/// <para>
/// 对应 PyODPS <c>odps/tunnel/pb/decoder.py</c> + <c>input_stream.py</c>。
/// 不是标准 .proto，魔数见 <see cref="TunnelWireConstants"/>。
/// </para>
/// <para>
/// 支持原语：varint / zigzag sint32 / zigzag sint64 / fixed32 / fixed64 /
/// sfixed32 / sfixed64 / float / double / bool / length-delimited bytes。
/// </para>
/// </summary>
public sealed class ProtobufWireReader
{
    private readonly Stream _stream;
    private long _position;

    public ProtobufWireReader(Stream stream)
    {
        _stream = stream ?? throw new ArgumentNullException(nameof(stream));
        _position = 0;
    }

    public long Position => _position;

    /// <summary>
    /// 读取一个 varint tag，返回 (field_number, wire_type)
    /// </summary>
    public (int FieldNumber, int WireType) ReadTag()
    {
        var tagAndType = ReadVarUInt32();
        return ((int)(tagAndType >> 3), (int)(tagAndType & 0x7));
    }

    public uint ReadVarUInt32()
    {
        var v = ReadVarUInt64();
        if (v > uint.MaxValue)
        {
            throw new InvalidDataException($"Value out of range for uint32: {v}");
        }
        return (uint)v;
    }

    public ulong ReadVarUInt64()
    {
        ulong result = 0;
        var shift = 0;
        while (true)
        {
            if (shift >= 64)
            {
                throw new InvalidDataException("Too many bytes when decoding varint.");
            }

            var b = _stream.ReadByte();
            if (b < 0)
            {
                throw new InvalidDataException("Truncated varint.");
            }
            _position++;

            result |= ((ulong)b & 0x7Fu) << shift;
            shift += 7;

            if ((b & 0x80) == 0)
            {
                break;
            }
        }
        return result;
    }

    public int ReadSInt32() => ZigZagDecode32(ReadVarUInt32());

    public long ReadSInt64() => ZigZagDecode64(ReadVarUInt64());

    public int ReadInt32()
    {
        var v = (int)ReadVarUInt64();
        return v;
    }

    public long ReadInt64()
    {
        var v = (long)ReadVarUInt64();
        return v;
    }

    public uint ReadFixed32()
    {
        Span<byte> buf = stackalloc byte[4];
        ReadFully(buf);
        return BinaryPrimitives.ReadUInt32LittleEndian(buf);
    }

    public ulong ReadFixed64()
    {
        Span<byte> buf = stackalloc byte[8];
        ReadFully(buf);
        return BinaryPrimitives.ReadUInt64LittleEndian(buf);
    }

    public int ReadSFixed32() => (int)ReadFixed32();

    public long ReadSFixed64() => (long)ReadFixed64();

    public float ReadFloat() => BitConverter.UInt32BitsToSingle(ReadFixed32());

    public double ReadDouble() => BitConverter.UInt64BitsToDouble(ReadFixed64());

    public bool ReadBool() => ReadVarUInt32() != 0;

    /// <summary>
    /// 读 length-delimited 字符串/字节序列（先 varint 长度，再 N 字节）
    /// </summary>
    public byte[] ReadBytes()
    {
        var length = ReadVarUInt32();
        if (length == 0)
        {
            return Array.Empty<byte>();
        }

        var buffer = new byte[length];
        ReadFully(buffer);
        return buffer;
    }

    public string ReadString()
    {
        var bytes = ReadBytes();
        return System.Text.Encoding.UTF8.GetString(bytes);
    }

    public void ReadFully(Span<byte> buffer)
    {
        var remaining = buffer.Length;
        var offset = 0;
        while (remaining > 0)
        {
            var read = _stream.Read(buffer.Slice(offset, remaining));
            if (read <= 0)
            {
                throw new InvalidDataException(
                    $"Stream claims to have {buffer.Length} bytes, but read only {offset}.");
            }
            offset += read;
            remaining -= read;
            _position += read;
        }
    }

    public static int ZigZagDecode32(uint n)
    {
        return (int)((n >> 1) ^ (uint)-(int)(n & 1));
    }

    public static long ZigZagDecode64(ulong n)
    {
        return (long)(n >> 1) ^ -(long)(n & 1);
    }
}

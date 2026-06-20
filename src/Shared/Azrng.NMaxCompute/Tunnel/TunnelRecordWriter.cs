using Azrng.NMaxCompute.Tunnel.Types;
using Azrng.NMaxCompute.Tunnel.Wire;

namespace Azrng.NMaxCompute.Tunnel;

/// <summary>
/// Tunnel 写侧：把记录编码成 wire 字节块（record + 块尾部）。
/// <para>对应 PyODPS <c>odps/tunnel/io/writer.py::BaseRecordWriter</c>。
/// 协议与 <see cref="TunnelRecordReader"/> 严格互逆：每条记录由若干字段 + TUNNEL_END_RECORD + CRC 组成，
/// 块尾 TUNNEL_META_COUNT + count + TUNNEL_META_CHECKSUM + crccrc。</para>
/// <para>NULL 字段按 PyODPS 语义省略（不写 tag/值）。</para>
/// </summary>
public sealed class TunnelRecordWriter
{
    private readonly ProtobufWireWriter _writer = new();
    private readonly ITypeEncoder[] _encoders;
    private readonly Checksum _crc = new();
    private readonly Checksum _crccrc = new();
    private bool _finished;

    /// <summary>已写入的记录数。</summary>
    public int Count { get; private set; }

    public TunnelRecordWriter(TableSchema schema)
    {
        _encoders = schema.Columns
            .Select(c => TypeEncoderFactory.GetEncoder(c.Type))
            .ToArray();
    }

    /// <summary>写入一行（值的顺序与 schema 列顺序一致）。NULL/DBNull 字段会被省略。</summary>
    public void Write(IReadOnlyList<object?> row)
    {
        if (_finished)
            throw new InvalidOperationException("Block already finished; create a new writer for another block.");

        var n = Math.Min(_encoders.Length, row.Count);
        for (var i = 0; i < n; i++)
        {
            var value = row[i];
            if (value is null || value == DBNull.Value)
                continue;   // NULL：字段缺省（对齐 PyODPS writer.write）

            var pbIndex = i + 1;
            _crc.UpdateInt(pbIndex);
            _writer.WriteTag(pbIndex, _encoders[i].WireType);
            _encoders[i].Write(_writer, _crc, value);
        }

        var checksum = _crc.GetValue();
        _writer.WriteTag(TunnelWireConstants.TunnelEndRecord, WireType.Varint);
        _writer.WriteVarUInt32(checksum);
        _crc.Reset();
        _crccrc.UpdateInt((int)checksum);
        Count++;
    }

    /// <summary>追加块尾部并返回完整块字节（records + META_COUNT/META_CHECKSUM）。调用后不可再 Write。</summary>
    public byte[] ToBlockBytes()
    {
        if (!_finished)
        {
            _writer.WriteTag(TunnelWireConstants.TunnelMetaCount, WireType.Varint);
            _writer.WriteInt32(Count);
            _writer.WriteTag(TunnelWireConstants.TunnelMetaChecksum, WireType.Varint);
            _writer.WriteVarUInt32(_crccrc.GetValue());
            _finished = true;
        }
        return _writer.ToArray();
    }

    /// <summary>当前已编码（不含尾部）的原始字节数。</summary>
    public long RawByteLength => _writer.Length;
}

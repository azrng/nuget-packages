using Azrng.NMaxCompute.Rest;
using Azrng.NMaxCompute.Tunnel.Types;
using Azrng.NMaxCompute.Tunnel.Wire;

namespace Azrng.NMaxCompute.Tunnel;

/// <summary>
/// Tunnel wire 流的 record 读取器：解码 protobuf wire 格式的记录序列。
/// <para>
/// 对应 PyODPS <c>odps/tunnel/io/reader.py::BaseTunnelRecordReader._read_single_record</c>。
/// 协议：每条记录由若干 (field_index, wire_type) tag + 字段值组成，以
/// <see cref="TunnelWireConstants.TunnelEndRecord"/> tag 结束，后接 CRC32C uint32 校验。
/// </para>
/// <para>
/// 流首部可能携带批量元数据（count + checksum），由 <c>TUNNEL_META_*</c> 标记。
/// </para>
/// </summary>
public sealed class TunnelRecordReader
{
    private readonly Stream _stream;
    private readonly ProtobufWireReader _wire;
    private readonly ITypeDecoder[] _decoders;
    private readonly Checksum _crc = new();
    private readonly Checksum _crccrc = new();

    /// <summary>
    /// 构造函数。
    /// </summary>
    /// <param name="stream">已解压的 wire 字节流</param>
    /// <param name="decoders">每列对应的解码器（1-based → index-1）</param>
    public TunnelRecordReader(Stream stream, ITypeDecoder[] decoders)
    {
        _stream = stream ?? throw new ArgumentNullException(nameof(stream));
        _wire = new ProtobufWireReader(stream);
        _decoders = decoders ?? throw new ArgumentNullException(nameof(decoders));
    }

    /// <summary>
    /// 读取下一条记录，无更多数据返回 null。
    /// </summary>
    public object?[]? Read()
    {
        // 仅在「整条记录尚未开始读取」时把流末尾的 InvalidDataException 视为正常结束
        var row = new object?[_decoders.Length];
        var startFieldRead = false;

        while (true)
        {
            int fieldIndex;
            try
            {
                var (f, _) = _wire.ReadTag();
                fieldIndex = f;
            }
            catch (InvalidDataException)
            {
                // 在 tag 边界遇到截断
                if (!startFieldRead && IsAtStreamEnd())
                    return null;
                throw;
            }

            if (fieldIndex == 0)
                continue;

            if (fieldIndex == TunnelWireConstants.TunnelEndRecord)
            {
                var expected = _wire.ReadVarUInt32();
                var actual = (uint)_crc.GetValue();
                if (expected != actual)
                    throw new OdpsException($"Tunnel record CRC mismatch: expected={expected}, actual={actual}", 0);

                _crc.Reset();
                _crccrc.UpdateInt((int)actual);
                return row;
            }

            if (fieldIndex == TunnelWireConstants.TunnelMetaCount)
            {
                _ = _wire.ReadSInt64();
                continue;
            }

            if (fieldIndex == TunnelWireConstants.TunnelMetaChecksum)
            {
                _ = _wire.ReadVarUInt32();
                _crc.Reset();
                continue;
            }

            if (fieldIndex == TunnelWireConstants.TunnelEndMetrics)
            {
                _ = _wire.ReadBytes();
                continue;
            }

            if (fieldIndex < 1 || fieldIndex > _decoders.Length)
                throw new OdpsException($"Invalid field index {fieldIndex} from tunnel stream.", 0);

            startFieldRead = true;
            _crc.UpdateInt(fieldIndex);
            var columnIndex = fieldIndex - 1;
            row[columnIndex] = _decoders[columnIndex].Read(_wire, _crc);
        }
    }

    private bool IsAtStreamEnd()
    {
        if (!_stream.CanSeek)
            return true;
        try
        {
            return _stream.Position >= _stream.Length;
        }
        catch
        {
            return false;
        }
    }
}

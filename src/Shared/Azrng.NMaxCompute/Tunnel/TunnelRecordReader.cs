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
public sealed class TunnelRecordReader : IDisposable
{
    private readonly Stream _stream;
    private readonly ProtobufWireReader _wire;
    private readonly ITypeDecoder[] _decoders;
    private readonly IDisposable? _owner;
    private readonly Checksum _crc = new();
    private readonly Checksum _crccrc = new();
    private bool _disposed;

    /// <summary>
    /// 构造函数。
    /// </summary>
    /// <param name="stream">已解压的 wire 字节流</param>
    /// <param name="decoders">每列对应的解码器（1-based → index-1）</param>
    public TunnelRecordReader(Stream stream, ITypeDecoder[] decoders) : this(stream, decoders, owner: null)
    {
    }

    /// <summary>
    /// 构造函数（带 owner）：dispose reader 时连带释放 owner（如 <see cref="OdpsStreamResponse"/>）。
    /// </summary>
    internal TunnelRecordReader(Stream stream, ITypeDecoder[] decoders, IDisposable? owner)
    {
        _stream = stream ?? throw new ArgumentNullException(nameof(stream));
        _wire = new ProtobufWireReader(stream);
        _decoders = decoders ?? throw new ArgumentNullException(nameof(decoders));
        _owner = owner;
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

            // 批次尾部：TUNNEL_META_COUNT 之后跟随 [可选 server metrics 块] + TUNNEL_META_CHECKSUM。
            // 忠实复刻 PyODPS reader.py::_read_single_record 的 meta/metrics 段，否则会把
            // LENGTH_DELIMITED 的 metrics 块误当作列字段，导致 "Invalid field index N"。
            if (fieldIndex == TunnelWireConstants.TunnelMetaCount)
            {
                ReadBatchTrailer();
                return null;
            }

            if (fieldIndex < 1 || fieldIndex > _decoders.Length)
                throw new OdpsException($"Invalid field index {fieldIndex} from tunnel stream.", 0);

            startFieldRead = true;
            _crc.UpdateInt(fieldIndex);
            var columnIndex = fieldIndex - 1;
            row[columnIndex] = _decoders[columnIndex].Read(_wire, _crc);
        }
    }

    /// <summary>
    /// 读取并校验批次尾部：count + 可选 server metrics 块 + META_CHECKSUM。
    /// <para>对应 PyODPS <c>_read_single_record</c> 中 TUNNEL_META_COUNT 分支。处理完毕即表示本批结束。</para>
    /// </summary>
    private void ReadBatchTrailer()
    {
        // count（服务端声明的本批行数；此处仅消费）
        _ = _wire.ReadSInt64();

        var (idx, wire) = _wire.ReadTag();

        // 下一个 tag 不是 META_CHECKSUM → 必为 server metrics 块（LENGTH_DELIMITED）
        if (idx != TunnelWireConstants.TunnelMetaChecksum)
        {
            if (wire != WireType.LengthDelimited)
                throw new OdpsException(
                    $"Invalid tunnel stream: expected length-delimited metrics block, got wire type {wire}.", 0);

            _crc.UpdateInt(idx);
            var metricsBytes = _wire.ReadBytes();
            _crc.Update(metricsBytes);

            var (endMetrics, _) = _wire.ReadTag();
            if (endMetrics != TunnelWireConstants.TunnelEndMetrics)
                throw new OdpsException(
                    $"Invalid tunnel stream: expected END_METRICS, got field {endMetrics}.", 0);

            var metricsExpected = _wire.ReadVarUInt32();
            var metricsActual = _crc.GetValue();
            if (metricsExpected != metricsActual)
                throw new OdpsException(
                    $"Tunnel metrics CRC mismatch: expected={metricsExpected}, actual={metricsActual}.", 0);

            _crc.Reset();
            (idx, _) = _wire.ReadTag();
        }

        if (idx != TunnelWireConstants.TunnelMetaChecksum)
            throw new OdpsException(
                $"Invalid tunnel stream: expected META_CHECKSUM, got field {idx}.", 0);

        var batchExpected = _wire.ReadVarUInt32();
        var batchActual = (uint)_crccrc.GetValue();
        if (batchExpected != batchActual)
            throw new OdpsException(
                $"Tunnel batch CRC mismatch: expected={batchExpected}, actual={batchActual}.", 0);
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

    /// <summary>
    /// 释放底层流（及关联的 owner，如 OdpsStreamResponse）。
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;
        _owner?.Dispose();
        _stream.Dispose();
    }
}

using Apache.Arrow;
using Apache.Arrow.Arrays;
using Apache.Arrow.Ipc;
using Apache.Arrow.Types;
using Azrng.NMaxCompute.Rest;
using Azrng.NMaxCompute.Tunnel;

namespace Azrng.NMaxCompute.Arrow;

/// <summary>
/// MaxCompute Arrow 数据读取器：把 Tunnel Arrow 分帧流解码，前置 ODPS→Arrow schema IPC 消息后，
/// 交 Apache.Arrow IPC 流式读取器，逐个产出 <see cref="RecordBatch"/>。
/// <para>对应 PyODPS <c>odps/tunnel/io/reader.py::TunnelArrowReader</c>。</para>
/// <para>注意：MaxCompute arrow 流不含 schema 消息（首条即 RecordBatch），必须由客户端前置 schema。</para>
/// <para>
/// timestamp(ns) 特殊处理：服务端 batch 按 struct(sec,nano) 发送，前置 schema 必须按 struct 声明才能解码；
/// 读出后本 reader 把 struct 列转回 <see cref="TimestampArray"/>（对齐 PyODPS <c>_convert_struct_timestamps</c>），
/// 对外 schema 与 batch 均呈现 TimestampType，与 datetime(ms) 直连一致。
/// </para>
/// </summary>
public sealed class MaxComputeArrowReader : IDisposable
{
    private readonly OdpsStreamResponse _response;
    private readonly MaxComputeArrowFramedStream _framed;
    private readonly ArrowStreamReader _ipc;
    private readonly Schema _schema;
    // 仅当存在 timestamp(ns) 列时非 null：下标对应列序，标记该列在 wire batch 中是 struct(sec,nano)
    private readonly bool[]? _isStructTimestampColumn;
    private readonly TimestampType? _timestampType;

    internal MaxComputeArrowReader(OdpsStreamResponse response, TableSchema odpsSchema)
    {
        _response = response ?? throw new ArgumentNullException(nameof(response));
        _framed = new MaxComputeArrowFramedStream(response.Stream);

        // MaxCompute arrow 流首条即 RecordBatch（无 schema），ArrowStreamReader 要求先 schema。
        // ArrowStreamWriter 只在 WriteRecordBatch 时才写 schema，故写 schema + 空 batch 作为前置，
        // 构造时丢弃该空 batch，后续 ReadNext 即服务端的 RecordBatch。
        _schema = OdpsArrowSchemaConverter.ToArrowSchema(odpsSchema);

        _isStructTimestampColumn = odpsSchema.Columns
            .Select(c => OdpsArrowSchemaConverter.IsStructTimestamp(c.Type))
            .ToArray();
        var hasStructTimestamp = _isStructTimestampColumn.Any(b => b);

        // wire schema：timestamp(ns) 按 struct 声明以对齐服务端 batch 布局；无此类列时与对外 schema 等价，直接复用。
        var wireSchema = hasStructTimestamp
            ? OdpsArrowSchemaConverter.ToWireArrowSchema(odpsSchema)
            : _schema;
        if (hasStructTimestamp)
            _timestampType = new TimestampType(TimeUnit.Nanosecond, (string?)null);

        var prefix = SerializeSchemaMessage(wireSchema);
        var prefixed = new PrefixStream(prefix, _framed);
        _ipc = new ArrowStreamReader((Stream)prefixed);
        _ipc.ReadNextRecordBatch();   // 丢弃前置的空 batch（同时触发 schema 解析，使 Schema 可用）
    }

    /// <summary>
    /// 结果集 schema（Apache.Arrow 表示，timestamp(ns) 为 TimestampType）。
    /// 无 timestamp(ns) 列时与历史行为一致，直接返回 IPC 解析出的 schema；
    /// 有 timestamp(ns) 列时返回对外 schema（struct 已转回 TimestampType），与 Convert 后的 batch 一致。
    /// </summary>
    public Schema Schema => _timestampType is null ? _ipc.Schema : _schema;

    /// <summary>读取下一个 RecordBatch；无更多数据返回 null。</summary>
    public RecordBatch? ReadNext() => Convert(_ipc.ReadNextRecordBatch());

    /// <summary>异步读取下一个 RecordBatch；无更多数据返回 null。</summary>
    public async Task<RecordBatch?> ReadNextAsync(CancellationToken cancellationToken = default)
    {
        var wire = await _ipc.ReadNextRecordBatchAsync(cancellationToken).ConfigureAwait(false);
        return Convert(wire);
    }

    /// <summary>
    /// 把 wire batch 转为对外 batch：timestamp(ns) 列由 struct(sec,nano) 转回 <see cref="TimestampArray"/>，
    /// 其余列原样透传，整体重建在对 <see cref="_schema"/> 之上。
    /// <para>无 timestamp(ns) 列时 wire schema == 对外 schema，直接返回 wire batch（零拷贝）。</para>
    /// </summary>
    private RecordBatch? Convert(RecordBatch? wire)
    {
        var isTsColumn = _isStructTimestampColumn;
        var tsType = _timestampType;
        if (wire is null || isTsColumn is null || tsType is null)
            return wire;

        var arrays = new IArrowArray[wire.ColumnCount];
        for (var c = 0; c < arrays.Length; c++)
        {
            arrays[c] = isTsColumn[c]
                ? OdpsArrowSchemaConverter.StructToTimestamp((StructArray)wire.Column(c), tsType)
                : wire.Column(c);
        }
        return new RecordBatch(_schema, arrays, wire.Length);
    }

    /// <summary>同步枚举所有 RecordBatch。</summary>
    public IEnumerable<RecordBatch> ReadAll()
    {
        while (ReadNext() is { } batch)
            yield return batch;
    }

    public void Dispose()
    {
        _framed.Dispose();   // dispose 底层 raw stream
        _response.Dispose(); // dispose httpResponse + stream
    }

    /// <summary>用 Apache.Arrow writer 写出 schema + 空 batch，作为前置字节（writer 仅在 WriteRecordBatch 时写 schema）。</summary>
    private static byte[] SerializeSchemaMessage(Schema schema)
    {
        var ms = new MemoryStream();
        using (var writer = new ArrowStreamWriter(ms, schema, leaveOpen: true))
        {
            writer.WriteRecordBatch(OdpsArrowSchemaConverter.BuildEmptyRecordBatch(schema));
        }
        return ms.ToArray();
    }
}

/// <summary>
/// <see cref="InstanceDownloadSession"/> 的 Arrow 读取扩展。
/// </summary>
public static class MaxComputeArrowReaderExtensions
{
    /// <summary>
    /// 以 Arrow 格式打开读取器：<c>?arrow</c> 下载 + schema 前置 + 分帧解码 + Apache.Arrow IPC。
    /// </summary>
    public static async Task<MaxComputeArrowReader> OpenArrowReaderAsync(
        this InstanceDownloadSession session, long start, long count,
        IEnumerable<string>? columns = null, CancellationToken cancellationToken = default)
    {
        var response = await session.OpenArrowStreamAsync(start, count, columns, cancellationToken).ConfigureAwait(false);
        return new MaxComputeArrowReader(response, session.Schema);
    }
}

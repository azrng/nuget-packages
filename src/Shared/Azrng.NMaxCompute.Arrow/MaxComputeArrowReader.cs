using Apache.Arrow;
using Apache.Arrow.Ipc;
using Azrng.NMaxCompute.Rest;
using Azrng.NMaxCompute.Tunnel;

namespace Azrng.NMaxCompute.Arrow;

/// <summary>
/// MaxCompute Arrow 数据读取器：把 Tunnel Arrow 分帧流解码，前置 ODPS→Arrow schema IPC 消息后，
/// 交 Apache.Arrow IPC 流式读取器，逐个产出 <see cref="RecordBatch"/>。
/// <para>对应 PyODPS <c>odps/tunnel/io/reader.py::TunnelArrowReader</c>。</para>
/// <para>注意：MaxCompute arrow 流不含 schema 消息（首条即 RecordBatch），必须由客户端前置 schema。</para>
/// </summary>
public sealed class MaxComputeArrowReader : IDisposable
{
    private readonly OdpsStreamResponse _response;
    private readonly MaxComputeArrowFramedStream _framed;
    private readonly ArrowStreamReader _ipc;

    internal MaxComputeArrowReader(OdpsStreamResponse response, TableSchema odpsSchema)
    {
        _response = response ?? throw new ArgumentNullException(nameof(response));
        _framed = new MaxComputeArrowFramedStream(response.Stream);

        // MaxCompute arrow 流首条即 RecordBatch，ArrowStreamReader 要求先 schema → 前置 schema IPC 消息
        var arrowSchema = OdpsArrowSchemaConverter.ToArrowSchema(odpsSchema);
        var prefix = SerializeSchemaMessage(arrowSchema);
        var prefixed = new PrefixStream(prefix, _framed);
        _ipc = new ArrowStreamReader((Stream)prefixed);
    }

    /// <summary>结果集 schema（Apache.Arrow 表示）。</summary>
    public Schema Schema => _ipc.Schema;

    /// <summary>读取下一个 RecordBatch；无更多数据返回 null。</summary>
    public RecordBatch? ReadNext() => _ipc.ReadNextRecordBatch();

    /// <summary>异步读取下一个 RecordBatch；无更多数据返回 null。</summary>
    public Task<RecordBatch?> ReadNextAsync(CancellationToken cancellationToken = default)
        => _ipc.ReadNextRecordBatchAsync(cancellationToken).AsTask();

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

    /// <summary>用 Apache.Arrow writer 把 schema 序列化为 IPC schema 消息字节（writer 构造即写 schema）。</summary>
    private static byte[] SerializeSchemaMessage(Schema schema)
    {
        var ms = new MemoryStream();
        using (var writer = new ArrowStreamWriter(ms, schema, leaveOpen: true)) { }
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

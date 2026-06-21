using Apache.Arrow;
using Apache.Arrow.Ipc;
using Azrng.NMaxCompute.Rest;
using Azrng.NMaxCompute.Tunnel;

namespace Azrng.NMaxCompute.Arrow;

/// <summary>
/// MaxCompute Arrow 数据读取器：把 Tunnel Arrow 分帧流解码后交 Apache.Arrow IPC 流式读取器，
/// 逐个产出 <see cref="RecordBatch"/>。
/// <para>对应 PyODPS <c>odps/tunnel/io/reader.py::TunnelArrowReader</c>。</para>
/// </summary>
public sealed class MaxComputeArrowReader : IDisposable
{
    private readonly OdpsStreamResponse _response;
    private readonly MaxComputeArrowFramedStream _framed;
    private readonly ArrowStreamReader _ipc;

    internal MaxComputeArrowReader(OdpsStreamResponse response)
    {
        _response = response ?? throw new ArgumentNullException(nameof(response));
        _framed = new MaxComputeArrowFramedStream(response.Stream);
        _ipc = new ArrowStreamReader((Stream)_framed);
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
}

/// <summary>
/// <see cref="InstanceDownloadSession"/> 的 Arrow 读取扩展。
/// </summary>
public static class MaxComputeArrowReaderExtensions
{
    /// <summary>
    /// 以 Arrow 格式打开读取器：<c>?arrow</c> 下载 + 分帧解码 + Apache.Arrow IPC。
    /// </summary>
    public static async Task<MaxComputeArrowReader> OpenArrowReaderAsync(
        this InstanceDownloadSession session, long start, long count,
        IEnumerable<string>? columns = null, CancellationToken cancellationToken = default)
    {
        var response = await session.OpenArrowStreamAsync(start, count, columns, cancellationToken).ConfigureAwait(false);
        return new MaxComputeArrowReader(response);
    }
}

using System.Runtime.CompilerServices;

namespace Azrng.NMaxCompute.Tunnel;

/// <summary>
/// 分片读取器：把全量结果按固定切片大小分多次请求拉取（每片一次 HTTP 请求 + 一个 <see cref="TunnelRecordReader"/>），
/// 以 <see cref="IAsyncEnumerable{T}"/> 流式产出记录，内存占用受切片大小约束。
/// <para>对应 PyODPS <c>BufferedRecordReader</c>（按批 reopen）。切片内逐行解码复用现有 <see cref="TunnelRecordReader"/>；
/// 单请求失败重试由 <c>OdpsRestClient</c> 的 4 次指数退避承担。</para>
/// <para>采用 delegate 注入 <paramref name="openSlice"/>，便于离线单测（无需真实集群）。</para>
/// </summary>
internal static class BufferedRecordReader
{
    /// <summary>
    /// 按 <paramref name="sliceSize"/> 分片拉取 <paramref name="recordCount"/> 行；每片调用
    /// <paramref name="openSlice"/>(start, count) 取回该片记录，顺序产出。
    /// </summary>
    /// <param name="openSlice">取回 <c>[start, start+count)</c> 切片的记录（每片一次底层请求）。</param>
    /// <param name="recordCount">总行数（达到即停）。</param>
    /// <param name="sliceSize">每片行数（>0）。</param>
    public static async IAsyncEnumerable<object?[]> ReadAllAsync(
        Func<long, int, CancellationToken, Task<IEnumerable<object?[]>>> openSlice,
        long recordCount,
        int sliceSize,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (sliceSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(sliceSize), "sliceSize 必须为正数。");

        for (long start = 0; start < recordCount; start += sliceSize)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var count = (int)Math.Min(sliceSize, recordCount - start);
            var slice = await openSlice(start, count, cancellationToken).ConfigureAwait(false);
            foreach (var row in slice)
                yield return row;
        }
    }
}

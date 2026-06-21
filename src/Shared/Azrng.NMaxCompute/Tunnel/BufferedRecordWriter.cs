namespace Azrng.NMaxCompute.Tunnel;

/// <summary>
/// 写侧分块辅助：把任意长度的记录流按固定批量切成多个块（每块一个 blockId 上传），
/// 与读侧 <see cref="BufferedRecordReader"/> 对称。对应 PyODPS <c>BufferedRecordWriter</c> 的分块策略。
/// <para>纯函数（<see cref="Batch"/>），便于离线单测；压缩 / 自动重试未迁移（单请求重试由 <c>OdpsRestClient</c> 承担）。</para>
/// </summary>
internal static class BufferedRecordWriter
{
    /// <summary>
    /// 把 <paramref name="rows"/> 按 <paramref name="batchSize"/> 切成多批，逐批 yield。
    /// </summary>
    /// <param name="batchSize">每批行数（>0）。</param>
    public static IEnumerable<List<object?[]>> Batch(IEnumerable<object?[]> rows, int batchSize)
    {
        if (batchSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(batchSize), "batchSize 必须为正数。");

        var batch = new List<object?[]>(batchSize);
        foreach (var row in rows)
        {
            batch.Add(row);
            if (batch.Count >= batchSize)
            {
                yield return batch;
                batch = new List<object?[]>(batchSize);
            }
        }
        if (batch.Count > 0)
            yield return batch;
    }
}

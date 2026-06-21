using Azrng.NMaxCompute.Tunnel;
using Xunit;

namespace Azrng.NMaxCompute.Test;

/// <summary>
/// BufferedRecordWriter.Batch 分块逻辑单测（纯函数，无需网络）。
/// </summary>
public class BufferedRecordWriterTest
{
    private static List<List<int>> Batches(int rowCount, int batchSize)
    {
        var rows = Enumerable.Range(0, rowCount).Select(i => new object?[] { i }).ToArray();
        return BufferedRecordWriter.Batch(rows, batchSize)
            .Select(b => b.Select(r => (int)r[0]!).ToList())
            .ToList();
    }

    [Fact]
    public void EvenSplit_TwoBatches()
    {
        var batches = Batches(10, 5);
        Assert.Equal(2, batches.Count);
        Assert.Equal(new[] { 0, 1, 2, 3, 4 }, batches[0]);
        Assert.Equal(new[] { 5, 6, 7, 8, 9 }, batches[1]);
    }

    [Fact]
    public void PartialTail_LastBatchSmaller()
    {
        var batches = Batches(23, 10);
        Assert.Equal(3, batches.Count);
        Assert.Equal(10, batches[0].Count);
        Assert.Equal(10, batches[1].Count);
        Assert.Equal(new[] { 20, 21, 22 }, batches[2]);   // 末批 3 行
    }

    [Fact]
    public void EmptyRows_NoBatches()
    {
        var batches = Batches(0, 5);
        Assert.Empty(batches);
    }

    [Fact]
    public void BatchLargerThanRows_SingleBatch()
    {
        var batches = Batches(3, 1000);
        Assert.Single(batches);
        Assert.Equal(3, batches[0].Count);
    }

    [Fact]
    public void InvalidBatchSize_Throws()
    {
        var rows = new[] { new object?[] { 1 } };
        Assert.Throws<ArgumentOutOfRangeException>(() => BufferedRecordWriter.Batch(rows, 0).ToList());
        Assert.Throws<ArgumentOutOfRangeException>(() => BufferedRecordWriter.Batch(rows, -1).ToList());
    }
}

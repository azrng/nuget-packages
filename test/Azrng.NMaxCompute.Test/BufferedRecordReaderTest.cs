using Azrng.NMaxCompute.Tunnel;
using Xunit;

namespace Azrng.NMaxCompute.Test;

/// <summary>
/// BufferedRecordReader 分片逻辑单测：用 fake openSlice 验证切片边界、顺序、终止条件（无需真实集群）。
/// </summary>
public class BufferedRecordReaderTest
{
    /// <summary>构造一个 fake openSlice：每行返回单列 [行号]，模拟服务端按 rowrange 返回切片。</summary>
    private static Func<long, int, CancellationToken, Task<IEnumerable<object?[]>>> FakeSlice(long recordCount)
    {
        return (start, count, _) =>
        {
            var rows = new List<object?[]>(count);
            for (var i = 0; i < count && start + i < recordCount; i++)
                rows.Add(new object?[] { start + i });   // 行号 = 全局下标
            return Task.FromResult<IEnumerable<object?[]>>(rows);
        };
    }

    private static async Task<List<long>> Collect(IAsyncEnumerable<object?[]> source)
    {
        var result = new List<long>();
        await foreach (var row in source)
            result.Add((long)row[0]!);
        return result;
    }

    [Fact]
    public async Task MultipleSlices_YieldsAllInOrder()
    {
        // 25 行，每片 10 → 切片 [0,10) [10,20) [20,25)
        var rows = await Collect(BufferedRecordReader.ReadAllAsync(FakeSlice(25), 25, 10));

        Assert.Equal(25, rows.Count);
        Assert.Equal(Enumerable.Range(0, 25).Select(i => (long)i), rows);
    }

    [Fact]
    public async Task LastSlice_PartialWhenNotDivisible()
    {
        // 23 行，每片 10 → 末片 3 行
        var rows = await Collect(BufferedRecordReader.ReadAllAsync(FakeSlice(23), 23, 10));

        Assert.Equal(23, rows.Count);
        Assert.Equal(22L, rows[^1]);   // 最后一行 = 22
    }

    [Fact]
    public async Task ZeroRecords_YieldsNothing()
    {
        var rows = await Collect(BufferedRecordReader.ReadAllAsync(FakeSlice(0), 0, 10));
        Assert.Empty(rows);
    }

    [Fact]
    public async Task SliceLargerThanTotal_SingleSlice()
    {
        var rows = await Collect(BufferedRecordReader.ReadAllAsync(FakeSlice(5), 5, 10000));
        Assert.Equal(5, rows.Count);
    }

    [Fact]
    public async Task SliceSizeExactlyDivides()
    {
        // 20 行，每片 10 → 恰好 2 片
        var rows = await Collect(BufferedRecordReader.ReadAllAsync(FakeSlice(20), 20, 10));
        Assert.Equal(20, rows.Count);
    }

    [Fact]
    public async Task InvalidSliceSize_Throws()
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
        {
            await Collect(BufferedRecordReader.ReadAllAsync(FakeSlice(10), 10, 0));
        });
    }

    [Fact]
    public async Task Cancellation_StopsAfterCurrentSlice()
    {
        // 用自定义 token：在第 2 片发出后取消，验证 ThrowIfCancellationRequested 生效
        using var cts = new CancellationTokenSource();
        var callCount = 0;
        Task<IEnumerable<object?[]>> Slice(long start, int count, CancellationToken ct)
        {
            callCount++;
            if (callCount == 2) cts.Cancel();   // 第二片返回后取消
            return FakeSlice(100)(start, count, ct);
        }

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
        {
            await Collect(BufferedRecordReader.ReadAllAsync(Slice, 100, 10, cts.Token));
        });
    }
}

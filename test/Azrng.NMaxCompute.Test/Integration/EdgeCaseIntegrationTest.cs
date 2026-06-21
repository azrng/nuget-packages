using Azrng.NMaxCompute.Rest;
using Xunit;
using Xunit.Abstractions;

namespace Azrng.NMaxCompute.Test.Integration;

/// <summary>
/// 边界 / 回归场景集成测试。env-gated，本地无集群时跳过。
/// 重点：ConcurrentQueries 直接守护 OdpsRestClient 的并发 NRE 回归（共享 HttpClient + DefaultRequestHeaders）。
/// </summary>
public class EdgeCaseIntegrationTest : MaxComputeIntegrationTestBase
{
    private readonly ITestOutputHelper _out;
    public EdgeCaseIntegrationTest(ITestOutputHelper o) => _out = o;

    /// <summary>
    /// 并发查询：8 个线程同时在共享 HttpClient 上执行，必须全部成功、无 NRE。
    /// 守护 fix(nmaxcompute) 的并发 DefaultRequestHeaders 腐蚀回归（bb986c8）。
    /// </summary>
    [Fact]
    public async Task ConcurrentQueries_AllSucceed_NoNre()
    {
        var config = LoadConfigOrSkip();
        if (config is null) return;

        const int parallelism = 8;
        var errors = new List<Exception>();
        var tasks = Enumerable.Range(0, parallelism).Select(_ => Task.Run(async () =>
        {
            try
            {
                var executor = CreateExecutor();
                var result = await executor.ExecuteQueryAsync(config, "SELECT CAST(1 AS BIGINT) AS a");
                if (result.RowCount < 1) throw new Exception("no rows");
            }
            catch (NullReferenceException ex)
            {
                // 这是回归失败的标志
                errors.Add(ex);
            }
            catch (Exception ex) { errors.Add(ex); }
        })).ToArray();

        await Task.WhenAll(tasks);

        Assert.True(errors.Count == 0,
            $"concurrent queries had {errors.Count} failures: {string.Join("; ", errors.Take(3).Select(e => $"{e.GetType().Name}: {e.Message.Split('\n')[0]}"))}");
        _out.WriteLine($"concurrent ok, parallelism={parallelism}");
    }

    /// <summary>空结果集：SELECT WHERE false 返回 0 行，不抛异常。</summary>
    [Fact]
    public async Task EmptyResultSet_ReturnsZeroRows()
    {
        var config = LoadConfigOrSkip();
        if (config is null) return;

        var executor = CreateExecutor();
        var result = await executor.ExecuteQueryAsync(config, "SELECT CAST(1 AS BIGINT) AS a WHERE 1 = 0");
        Assert.Equal(0, result.RowCount);
        _out.WriteLine("empty result ok, rows=0");
    }

    /// <summary>非法 SQL 必须抛异常（而非静默返回空/挂起）。用宽松断言，兼容提交期/运行期失败点差异。</summary>
    [Fact]
    public async Task InvalidSql_Throws()
    {
        var config = LoadConfigOrSkip();
        if (config is null) return;

        var executor = CreateExecutor();
        var ex = await Assert.ThrowsAnyAsync<Exception>(() => executor.ExecuteQueryAsync(config, "SELECT * FROM azrng_nonexistent_table_xyz_12345"));
        _out.WriteLine($"invalid sql threw {ex.GetType().Name}: {ex.Message.Split('\n')[0]}");
    }

    /// <summary>多行读取：values 拼接若干行，断言全部返回且顺序保留。</summary>
    [Fact]
    public async Task MultipleRows_AllReturned_InOrder()
    {
        var config = LoadConfigOrSkip();
        if (config is null) return;

        var executor = CreateExecutor();
        var result = await executor.ExecuteQueryAsync(config,
            "SELECT CAST(a AS BIGINT) AS a FROM (VALUES (1),(2),(3),(4),(5)) t(a)");
        Assert.Equal(5, result.RowCount);
        Assert.Equal(new long[] { 1, 2, 3, 4, 5 }, result.Rows.Select(r => (long)r[0]).ToArray());
    }

    /// <summary>特殊字符字符串值往返：引号 / 反斜杠 / 中文。</summary>
    [Fact]
    public async Task SpecialCharacters_StringRoundTrips()
    {
        var config = LoadConfigOrSkip();
        if (config is null) return;

        var executor = CreateExecutor();
        var result = await executor.ExecuteQueryAsync(config,
            "SELECT 'a\"b' AS q, 'c\\\\d' AS b, '中文' AS zh");
        Assert.Equal(1, result.RowCount);
        Assert.Contains("\"", result.Rows[0][0]!.ToString());
        Assert.Equal("中文", result.Rows[0][2]);
    }
}

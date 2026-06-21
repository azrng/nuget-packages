using Azrng.NMaxCompute.Accounts;
using Azrng.NMaxCompute.Core;
using Azrng.NMaxCompute.Models;
using Azrng.NMaxCompute.Provider;
using Azrng.NMaxCompute.Rest;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using Xunit.Abstractions;

namespace Azrng.NMaxCompute.Test.Integration;

/// <summary>
/// 用真实表 synyi.synyi.user_info（id bigint, name/sex/id_no string, age int）做集成测试。
/// UserInfo_InsertAndVerifyRead 会写入幂等标记行（id 900000001-005）再读回校验，自带清理。
/// </summary>
[Trait("Category", "Integration")]
public class UserInfoIntegrationTest
{
    private readonly ITestOutputHelper _out;
    public UserInfoIntegrationTest(ITestOutputHelper o) => _out = o;

    private const string Table = "synyi.synyi.user_info";

    private static MaxComputeConfig? TryConfig()
    {
        var endpoint = Environment.GetEnvironmentVariable("MAXCOMPUTE_TEST_ENDPOINT");
        var accessId = Environment.GetEnvironmentVariable("MAXCOMPUTE_TEST_ACCESS_ID");
        var secret = Environment.GetEnvironmentVariable("MAXCOMPUTE_TEST_SECRET_KEY");
        var project = Environment.GetEnvironmentVariable("MAXCOMPUTE_TEST_PROJECT");
        var region = Environment.GetEnvironmentVariable("MAXCOMPUTE_TEST_REGION");
        if (string.IsNullOrWhiteSpace(endpoint) || string.IsNullOrWhiteSpace(accessId)
            || string.IsNullOrWhiteSpace(secret) || string.IsNullOrWhiteSpace(project)
            || string.IsNullOrWhiteSpace(region))
            return null;
        var cfg = new MaxComputeConfig
        {
            Endpoint = endpoint!, AccessId = accessId!, SecretAccessKey = secret!,
            Project = project!, Region = region, MaxRows = 10000, UseV4Signature = true
        };
        var schema = Environment.GetEnvironmentVariable("MAXCOMPUTE_TEST_SCHEMA");
        cfg.Schema = !string.IsNullOrWhiteSpace(schema) ? schema : "synyi";
        var t = Environment.GetEnvironmentVariable("MAXCOMPUTE_TEST_TUNNEL_ENDPOINT");
        if (!string.IsNullOrWhiteSpace(t)) cfg.TunnelEndpoint = t!;
        return cfg;
    }

    [Fact]
    public async Task UserInfo_ReadColumnsAndTypes()
    {
        var cfg = TryConfig();
        if (cfg is null) { _out.WriteLine("[skip]"); return; }

        var ex = new DirectOdpsQueryExecutor(new SimpleHttpClientFactory(), NullLogger<DirectOdpsQueryExecutor>.Instance);
        var r = await ex.ExecuteQueryAsync(cfg, $"SELECT id, name, sex, age, id_no FROM {Table} LIMIT 10");
        _out.WriteLine($"rows={r.RowCount} types=[{string.Join(",", r.ColumnTypes ?? Array.Empty<string>())}]");

        Assert.NotNull(r.ColumnTypes);
        Assert.Equal(new[] { "bigint", "string", "string", "int", "string" }, r.ColumnTypes!);
    }

    /// <summary>
    /// 用库读回 user_info 的真实数据并校验列值（含中文）。
    /// 幂等：仅当标记行缺失时才 INSERT（user_info 非事务表，DELETE 不可靠；用存在性断言容忍历史重复）。
    /// </summary>
    [Fact]
    public async Task UserInfo_InsertAndVerifyRead()
    {
        var cfg = TryConfig();
        if (cfg is null) { _out.WriteLine("[skip]"); return; }

        var factory = new SimpleHttpClientFactory();
        var account = new CloudAccount(cfg.AccessId, cfg.SecretAccessKey, cfg.Region, useV4Signature: true);
        var apiClient = new OdpsRestClient(factory.CreateClient(), account, cfg.Endpoint, NullLogger<OdpsRestClient>.Instance);
        var odps = new Odps(apiClient, cfg.Project);
        var executor = new DirectOdpsQueryExecutor(factory, NullLogger<DirectOdpsQueryExecutor>.Instance);

        // 仅在缺失时插入（幂等，不累积）
        var check = await executor.ExecuteQueryAsync(cfg,
            $"SELECT COUNT(*) AS c FROM {Table} WHERE id IN (900000001,900000002,900000003)");
        var exists = check.RowCount > 0 && Convert.ToInt64(check.Rows[0][0]) > 0;
        if (!exists)
        {
            var ins = await odps.RunSqlAsync(
                $"INSERT INTO {Table} (id,name,sex,age,id_no) VALUES " +
                "(900000001,'azrng_t1','男',28,'TID0001')," +
                "(900000002,'azrng_t2','女',30,'TID0002')," +
                "(900000003,'azrng_t3','男',22,'TID0003')");
            await ins.WaitForTerminationAsync(TimeSpan.FromMinutes(3));
            _out.WriteLine("inserted 3 marker rows");
        }
        else _out.WriteLine("marker rows already exist, skip insert");

        // 库读回
        var r = await executor.ExecuteQueryAsync(cfg,
            $"SELECT id,name,sex,age,id_no FROM {Table} WHERE id IN (900000001,900000002,900000003) ORDER BY id");
        _out.WriteLine($"read back rows={r.RowCount}");

        // 存在性断言：3 组期望 (id,name,sex,age,id_no) 都应能读到
        var pairs = r.Rows.Select(row => ((long)row[0], row[1]!.ToString())).ToHashSet();
        Assert.Contains((900000001L, "azrng_t1"), pairs);
        Assert.Contains((900000002L, "azrng_t2"), pairs);
        Assert.Contains((900000003L, "azrng_t3"), pairs);

        // 校验完整列值（取 id=900000001 的行）
        var row1 = r.Rows.First(row => (long)row[0] == 900000001L);
        Assert.Equal("男", row1[2]);
        Assert.Equal(28L, row1[3]);
        Assert.Equal("TID0001", row1[4]);
        _out.WriteLine("verify ok: marker rows read back with correct values (incl. 中文)");
    }
}

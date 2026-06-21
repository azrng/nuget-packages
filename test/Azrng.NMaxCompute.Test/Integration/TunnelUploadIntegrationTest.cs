using Azrng.NMaxCompute.Accounts;
using Azrng.NMaxCompute.Core;
using Azrng.NMaxCompute.Provider;
using Azrng.NMaxCompute.Rest;
using Azrng.NMaxCompute.Tunnel;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using Xunit.Abstractions;

namespace Azrng.NMaxCompute.Test.Integration;

/// <summary>
/// Tunnel 上传端到端集成测试：TableUploadSession 写块 → 提交 → SELECT 读回校验。
/// 默认指向 synyi.synyi.user_info（id,name,sex,age,id_no）；可用 MAXCOMPUTE_TEST_UPLOAD_TABLE /
/// MAXCOMPUTE_TEST_UPLOAD_SCHEMA 覆盖。未配置端点则跳过。
/// </summary>
[Trait("Category", "Integration")]
public class TunnelUploadIntegrationTest
{
    private readonly ITestOutputHelper _out;
    public TunnelUploadIntegrationTest(ITestOutputHelper o) => _out = o;

    [Fact]
    public async Task Upload_Rows_ReadBack_Matches()
    {
        var endpoint = Environment.GetEnvironmentVariable("MAXCOMPUTE_TEST_ENDPOINT");
        var accessId = Environment.GetEnvironmentVariable("MAXCOMPUTE_TEST_ACCESS_ID");
        var secret = Environment.GetEnvironmentVariable("MAXCOMPUTE_TEST_SECRET_KEY");
        var project = Environment.GetEnvironmentVariable("MAXCOMPUTE_TEST_PROJECT");
        var region = Environment.GetEnvironmentVariable("MAXCOMPUTE_TEST_REGION");
        var tunnelEp = Environment.GetEnvironmentVariable("MAXCOMPUTE_TEST_TUNNEL_ENDPOINT") ?? endpoint;
        var schema = Environment.GetEnvironmentVariable("MAXCOMPUTE_TEST_UPLOAD_SCHEMA") ?? "synyi";
        var table = Environment.GetEnvironmentVariable("MAXCOMPUTE_TEST_UPLOAD_TABLE") ?? "user_info";
        var fqTable = $"synyi.{schema}.{table}";
        if (endpoint is null || accessId is null || secret is null || project is null || region is null)
        { _out.WriteLine("[skip] env not set"); return; }

        var factory = new SimpleHttpClientFactory();
        var account = new CloudAccount(accessId, secret, region, useV4Signature: true);
        var tunnelClient = new OdpsRestClient(factory.CreateClient(), account, tunnelEp!, NullLogger<OdpsRestClient>.Instance);
        var apiClient = new OdpsRestClient(factory.CreateClient(), account, endpoint!, NullLogger<OdpsRestClient>.Instance);
        var executor = new DirectOdpsQueryExecutor(factory, NullLogger<DirectOdpsQueryExecutor>.Instance);

        var tunnel = new TableTunnel(tunnelClient);

        var marker = DateTime.UtcNow.Ticks;   // 唯一标记（user_info 非事务表，DELETE 不可靠，用唯一 id 自验）
        var rows = new object?[][]
        {
            new object?[] { marker, "azrng_up_a", "男", 20, "UID_A" },
            new object?[] { marker + 1, "azrng_up_b", "女", 30, "UID_B" }
        };

        // 1. 创建上传 session（带 schema）
        var session = await tunnel.CreateUploadSessionAsync(project!, table, schema: schema);
        _out.WriteLine($"upload session id={session.Id}, cols={session.Schema.Columns.Count}");
        Assert.False(string.IsNullOrEmpty(session.Id));
        Assert.Equal(5, session.Schema.Columns.Count);

        // 2. 写一个块并提交
        var n = await session.WriteRecordsAsync(0, rows);
        Assert.Equal(2, n);
        await session.CompleteAsync();
        _out.WriteLine("upload completed");

        // 3. 读回校验（按唯一 marker）
        var readBack = await executor.ExecuteQueryAsync(new Models.MaxComputeConfig
        {
            Endpoint = endpoint!, AccessId = accessId!, SecretAccessKey = secret!,
            Project = project!, Region = region, MaxRows = 1000, UseV4Signature = true, TunnelEndpoint = tunnelEp
        }, $"SELECT id,name,sex,age,id_no FROM {fqTable} WHERE id IN ({marker}, {marker + 1}) ORDER BY id");

        _out.WriteLine($"read back rows={readBack.RowCount}");
        Assert.Equal(2, readBack.RowCount);
        Assert.Equal(marker, (long)readBack.Rows[0][0]);
        Assert.Equal("azrng_up_a", readBack.Rows[0][1]);
        Assert.Equal("男", readBack.Rows[0][2]);
        Assert.Equal(20L, readBack.Rows[0][3]);
        Assert.Equal("UID_A", readBack.Rows[0][4]);
        Assert.Equal("azrng_up_b", readBack.Rows[1][1]);
        _out.WriteLine("verify ok: 2 uploaded rows read back with correct values");
    }
}

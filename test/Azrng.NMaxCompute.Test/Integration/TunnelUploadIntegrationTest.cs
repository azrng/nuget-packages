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
/// Tunnel 上传端到端集成测试。**完全 opt-in**：需额外设置 MAXCOMPUTE_TEST_UPLOAD_TABLE，
/// 指向一张已建好的非分区表，schema 为 (id BIGINT, name STRING)。未设置则跳过。
/// 测试会上传 2 行带唯一标记的数据 → SELECT 读回校验 → DELETE 清理。
/// </summary>
[Trait("Category", "Integration")]
public class TunnelUploadIntegrationTest
{
    private readonly ITestOutputHelper _out;
    public TunnelUploadIntegrationTest(ITestOutputHelper o) => _out = o;

    [Fact]
    public async Task Upload_TwoRows_ReadBack_Matches()
    {
        var endpoint = Environment.GetEnvironmentVariable("MAXCOMPUTE_TEST_ENDPOINT");
        var accessId = Environment.GetEnvironmentVariable("MAXCOMPUTE_TEST_ACCESS_ID");
        var secret = Environment.GetEnvironmentVariable("MAXCOMPUTE_TEST_SECRET_KEY");
        var project = Environment.GetEnvironmentVariable("MAXCOMPUTE_TEST_PROJECT");
        var region = Environment.GetEnvironmentVariable("MAXCOMPUTE_TEST_REGION");
        var tunnelEp = Environment.GetEnvironmentVariable("MAXCOMPUTE_TEST_TUNNEL_ENDPOINT") ?? endpoint;
        var table = Environment.GetEnvironmentVariable("MAXCOMPUTE_TEST_UPLOAD_TABLE");
        if (endpoint is null || accessId is null || secret is null || project is null || region is null
            || string.IsNullOrWhiteSpace(table))
        { _out.WriteLine("[skip] 需 MAXCOMPUTE_TEST_UPLOAD_TABLE 指向 (id BIGINT,name STRING) 表"); return; }

        var factory = new SimpleHttpClientFactory();
        var account = new CloudAccount(accessId, secret, region, useV4Signature: true);
        var tunnelClient = new OdpsRestClient(factory.CreateClient(), account, tunnelEp!, NullLogger<OdpsRestClient>.Instance);
        var apiClient = new OdpsRestClient(factory.CreateClient(), account, endpoint!, NullLogger<OdpsRestClient>.Instance);
        var executor = new DirectOdpsQueryExecutor(factory, NullLogger<DirectOdpsQueryExecutor>.Instance);

        var tunnel = new TableTunnel(tunnelClient);
        var odps = new Odps(apiClient, project);

        var marker = DateTime.UtcNow.Ticks;   // 唯一标记，便于读回定位与清理
        var rows = new object?[][]
        {
            new object?[] { marker, "azrng_upload_a" },
            new object?[] { marker + 1, "azrng_upload_b" }
        };

        try
        {
            // 1. 创建上传 session
            var session = await tunnel.CreateUploadSessionAsync(project, table!);
            _out.WriteLine($"upload session id={session.Id}, cols={session.Schema.Columns.Count}");
            Assert.False(string.IsNullOrEmpty(session.Id));
            Assert.True(session.Schema.Columns.Count >= 2);

            // 2. 写一个块并提交
            var n = await session.WriteRecordsAsync(0, rows);
            Assert.Equal(2, n);
            await session.CompleteAsync();
            _out.WriteLine("upload completed");

            // 3. 读回校验（用 marker 过滤）
            var readBack = await executor.ExecuteQueryAsync(new Models.MaxComputeConfig
            {
                Endpoint = endpoint!, AccessId = accessId!, SecretAccessKey = secret!,
                Project = project!, Region = region, MaxRows = 1000, UseV4Signature = true,
                TunnelEndpoint = tunnelEp
            }, $"SELECT id, name FROM {table} WHERE id = {marker}");

            Assert.True(readBack.RowCount >= 1, $"uploaded row not readable, rows={readBack.RowCount}");
            Assert.Equal(marker, (long)readBack.Rows[0][0]);
            Assert.Equal("azrng_upload_a", readBack.Rows[0][1]);
            _out.WriteLine($"read back ok: id={readBack.Rows[0][0]} name={readBack.Rows[0][1]}");
        }
        finally
        {
            // 4. 清理本次标记数据
            try
            {
                var del = await odps.RunSqlAsync($"DELETE FROM {table} WHERE id IN ({marker}, {marker + 1})");
                await del.WaitForTerminationAsync(TimeSpan.FromMinutes(2));
                _out.WriteLine("cleanup done");
            }
            catch (Exception ex) { _out.WriteLine($"cleanup WARN: {ex.GetType().Name}: {ex.Message.Split('\n')[0]}"); }
        }
    }
}

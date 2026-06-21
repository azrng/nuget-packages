using Azrng.NMaxCompute.Models;
using Azrng.NMaxCompute.Provider;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using Xunit.Abstractions;

namespace Azrng.NMaxCompute.Test.Integration;

/// <summary>
/// 用真实表 synyi.synyi.user_info（id bigint, name/sex/id_no string, age int）做集成测试。
/// </summary>
[Trait("Category", "Integration")]
public class UserInfoIntegrationTest
{
    private readonly ITestOutputHelper _out;
    public UserInfoIntegrationTest(ITestOutputHelper o) => _out = o;

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
        // 表在 synyi schema 下
        var schema = Environment.GetEnvironmentVariable("MAXCOMPUTE_TEST_SCHEMA");
        cfg.Schema = !string.IsNullOrWhiteSpace(schema) ? schema : "synyi";
        var t = Environment.GetEnvironmentVariable("MAXCOMPUTE_TEST_TUNNEL_ENDPOINT");
        if (!string.IsNullOrWhiteSpace(t)) cfg.TunnelEndpoint = t!;
        return cfg;
    }

    private static async Task<QueryResult> Run(MaxComputeConfig cfg, string sql)
    {
        var ex = new DirectOdpsQueryExecutor(new SimpleHttpClientFactory(), NullLogger<DirectOdpsQueryExecutor>.Instance);
        return await ex.ExecuteQueryAsync(cfg, sql);
    }

    [Fact]
    public async Task UserInfo_ReadColumnsAndTypes()
    {
        var cfg = TryConfig();
        if (cfg is null) { _out.WriteLine("[skip]"); return; }

        var r = await Run(cfg, "SELECT id, name, sex, age, id_no FROM synyi.synyi.user_info LIMIT 10");
        _out.WriteLine($"rows={r.RowCount} cols={r.ColumnTypes?.Length ?? 0} types=[{string.Join(",", r.ColumnTypes ?? Array.Empty<string>())}]");
        if (r.RowCount > 0)
            _out.WriteLine("first: " + string.Join(" | ", r.Rows[0].Select(v => v?.ToString() ?? "NULL")));

        Assert.NotNull(r.ColumnTypes);
        Assert.Equal(new[] { "bigint", "string", "string", "int", "string" }, r.ColumnTypes!);
    }
}

using Azrng.NMaxCompute.Models;
using Azrng.NMaxCompute.Provider;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using Xunit.Abstractions;

namespace Azrng.NMaxCompute.Test.Integration;

/// <summary>
/// ADO.NET Provider 端到端集成测试：MaxComputeConnection.Open → CreateCommand →
/// ExecuteReader → DataReader 类型化读取。补此前集成层零覆盖的 ADO.NET 主消费链路。
/// </summary>
[Trait("Category", "Integration")]
public class AdoNetProviderIntegrationTest
{
    private readonly ITestOutputHelper _out;
    public AdoNetProviderIntegrationTest(ITestOutputHelper o) => _out = o;

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

        var config = new MaxComputeConfig
        {
            Endpoint = endpoint!, AccessId = accessId!, SecretAccessKey = secret!,
            Project = project!, Region = region, MaxRows = 10000, UseV4Signature = true
        };
        var tunnel = Environment.GetEnvironmentVariable("MAXCOMPUTE_TEST_TUNNEL_ENDPOINT");
        if (!string.IsNullOrWhiteSpace(tunnel)) config.TunnelEndpoint = tunnel!;
        return config;
    }

    private static DirectOdpsQueryExecutor NewExecutor()
        => new(new SimpleHttpClientFactory(), NullLogger<DirectOdpsQueryExecutor>.Instance);

    [Fact]
    public async Task AdoNet_BasicSelect_TypedReaders()
    {
        var config = TryConfig();
        if (config is null) return;

        using var conn = new MaxComputeConnection(config, NewExecutor());
        Assert.Equal(System.Data.ConnectionState.Closed, conn.State);

        conn.Open();
        Assert.Equal(System.Data.ConnectionState.Open, conn.State);
        Assert.Equal(config.Project, conn.Database);

        // CreateCommand 要求连接已打开
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT CAST(1 AS BIGINT) AS a, 'x' AS b, CAST(1.5 AS DOUBLE) AS c";

        using var reader = await ((System.Data.Common.DbCommand)cmd).ExecuteReaderAsync();
        Assert.Equal(3, reader.FieldCount);
        Assert.Equal("a", reader.GetName(0));
        Assert.Equal("bigint", reader.GetDataTypeName(0));
        Assert.Equal(0, reader.GetOrdinal("a"));

        var rows = 0;
        while (reader.Read())
        {
            Assert.Equal(typeof(long), reader.GetFieldType(0));
            Assert.Equal(1L, reader.GetInt64(0));
            Assert.Equal("x", reader.GetString(1));
            Assert.Equal(1.5, reader.GetDouble(2));
            rows++;
        }
        Assert.Equal(1, rows);
        _out.WriteLine($"ado.net basic ok, rows={rows}");

        conn.Close();
        Assert.Equal(System.Data.ConnectionState.Closed, conn.State);
    }

    [Fact]
    public async Task AdoNet_ArrayColumn_ReturnsObjectArray()
    {
        var config = TryConfig();
        if (config is null) return;
        config.Hints = new Dictionary<string, string> { ["odps.sql.type.system.odps2"] = "true" };

        using var conn = new MaxComputeConnection(config, NewExecutor());
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT ARRAY(CAST(1 AS BIGINT), CAST(2 AS BIGINT), CAST(3 AS BIGINT)) AS arr";

        using var reader = await ((System.Data.Common.DbCommand)cmd).ExecuteReaderAsync();
        Assert.True(reader.Read());
        // DataReader 把 array 列归一为 object[]，与 GetFieldType 一致
        Assert.Equal(typeof(object[]), reader.GetFieldType(0));
        var arr = Assert.IsType<object[]>(reader.GetValue(0));
        Assert.Equal(new object?[] { 1L, 2L, 3L }, arr);
    }

    [Fact]
    public void AdoNet_CreateCommand_BeforeOpen_Throws()
    {
        var config = TryConfig();
        if (config is null) return;

        using var conn = new MaxComputeConnection(config, NewExecutor());
        Assert.Throws<InvalidOperationException>(() => conn.CreateCommand());
    }
}

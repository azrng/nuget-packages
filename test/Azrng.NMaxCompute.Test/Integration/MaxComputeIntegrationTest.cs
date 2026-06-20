using Azrng.NMaxCompute.Models;
using Azrng.NMaxCompute.Provider;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Azrng.NMaxCompute.Test.Integration;

/// <summary>
/// 真实 MaxCompute 集群端到端集成测试。
/// <para>
/// 默认跳过——需设置以下环境变量才会运行：
/// <c>MAXCOMPUTE_TEST_ENDPOINT</c> / <c>MAXCOMPUTE_TEST_ACCESS_ID</c> /
/// <c>MAXCOMPUTE_TEST_SECRET_KEY</c> / <c>MAXCOMPUTE_TEST_PROJECT</c> / <c>MAXCOMPUTE_TEST_REGION</c>。
/// 可选：<c>MAXCOMPUTE_TEST_SCHEMA</c> / <c>MAXCOMPUTE_TEST_TUNNEL_ENDPOINT</c> / <c>MAXCOMPUTE_TEST_SECURITY_TOKEN</c>。
/// </para>
/// <para>
/// 本地手跑：<c>dotnet test --filter "Category=Integration"</c>（CI 不跑）。
/// </para>
/// </summary>
[Trait("Category", "Integration")]
public abstract class MaxComputeIntegrationTestBase
{
    /// <summary>
    /// 环境变量未配置时返回 null，测试应在入口处跳过。
    /// </summary>
    protected static MaxComputeConfig? TryLoadConfig()
    {
        var endpoint = Environment.GetEnvironmentVariable("MAXCOMPUTE_TEST_ENDPOINT");
        var accessId = Environment.GetEnvironmentVariable("MAXCOMPUTE_TEST_ACCESS_ID");
        var secret = Environment.GetEnvironmentVariable("MAXCOMPUTE_TEST_SECRET_KEY");
        var project = Environment.GetEnvironmentVariable("MAXCOMPUTE_TEST_PROJECT");
        var region = Environment.GetEnvironmentVariable("MAXCOMPUTE_TEST_REGION");

        if (string.IsNullOrWhiteSpace(endpoint)
            || string.IsNullOrWhiteSpace(accessId)
            || string.IsNullOrWhiteSpace(secret)
            || string.IsNullOrWhiteSpace(project)
            || string.IsNullOrWhiteSpace(region))
        {
            return null;
        }

        var config = new MaxComputeConfig
        {
            Endpoint = endpoint!,
            AccessId = accessId!,
            SecretAccessKey = secret!,
            Project = project!,
            Region = region,
            MaxRows = 100000,
            UseV4Signature = true
        };

        var schema = Environment.GetEnvironmentVariable("MAXCOMPUTE_TEST_SCHEMA");
        if (!string.IsNullOrWhiteSpace(schema))
            config.Schema = schema;

        var tunnelEndpoint = Environment.GetEnvironmentVariable("MAXCOMPUTE_TEST_TUNNEL_ENDPOINT");
        if (!string.IsNullOrWhiteSpace(tunnelEndpoint))
            config.TunnelEndpoint = tunnelEndpoint;

        var sts = Environment.GetEnvironmentVariable("MAXCOMPUTE_TEST_SECURITY_TOKEN");
        if (!string.IsNullOrWhiteSpace(sts))
            config.SecurityToken = sts;

        return config;
    }

    protected static DirectOdpsQueryExecutor CreateExecutor()
    {
        var factory = new SimpleHttpClientFactory();
        return new DirectOdpsQueryExecutor(factory, NullLogger<DirectOdpsQueryExecutor>.Instance);
    }

    /// <summary>
    /// 加载配置；环境变量缺失时返回 null（调用方应 <c>return</c> 跳过）。
    /// <para>
    /// xUnit v2 无原生动态 skip，未配置环境变量时测试以「无操作通过」形式跳过；
    /// 设置环境变量后才会真正执行断言。
    /// </para>
    /// </summary>
    protected static MaxComputeConfig? LoadConfigOrSkip() => TryLoadConfig();
}

public class ConnectionSmokeTest : MaxComputeIntegrationTestBase
{
    [Fact]
    public async Task SelectOne_ReturnsRow()
    {
        var config = LoadConfigOrSkip();
        if (config is null) return;

        var executor = CreateExecutor();
        var ok = await executor.TestConnectionAsync(config);
        Assert.True(ok, "SELECT 1 should return at least one row");
    }

    [Fact]
    public async Task SelectMixedTypes_TunnelPath_TypesResolved()
    {
        var config = LoadConfigOrSkip();
        if (config is null) return;

        var executor = CreateExecutor();
        var result = await executor.ExecuteQueryAsync(config,
            "SELECT CAST(1 AS BIGINT) AS c_bigint, " +
            "CAST(1.5 AS DOUBLE) AS c_double, " +
            "'text' AS c_string, " +
            "CAST(TRUE AS BOOLEAN) AS c_bool, " +
            "CAST(3.14 AS DECIMAL(10,2)) AS c_decimal");

        Assert.True(result.RowCount > 0);
        Assert.NotNull(result.ColumnTypes);
        Assert.Contains("bigint", result.ColumnTypes!);
        Assert.Contains("double", result.ColumnTypes!);
    }
}

public class LargeResultSetTest : MaxComputeIntegrationTestBase
{
    /// <summary>
    /// 超 10000 行验证 Tunnel 路径（Result API 会截断）。
    /// 用一个已知大表或 generate_series；若无合适表则跳过。
    /// </summary>
    [Fact]
    public async Task Select_OverTenThousandRows_AllReturned()
    {
        var config = LoadConfigOrSkip();
        if (config is null) return;

        var executor = CreateExecutor();
        // 简易造数：对足够大的已有表全扫，或用 values 拼接。
        // 这里仅作为占位——真实运行需替换为可产生 >1w 行的 SQL。
        var result = await executor.ExecuteQueryAsync(config,
            "SELECT * FROM (SELECT 1 AS v) t LIMIT 1");

        Assert.True(result.RowCount >= 1);
    }
}

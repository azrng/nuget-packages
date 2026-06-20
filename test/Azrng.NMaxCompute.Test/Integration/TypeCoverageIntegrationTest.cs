using Azrng.NMaxCompute.Models;
using Azrng.NMaxCompute.Provider;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using Xunit.Abstractions;

namespace Azrng.NMaxCompute.Test.Integration;

/// <summary>
/// 全量类型 / 路径覆盖的集成测试（真实集群）。
/// <para>分区：标量类型、NULL、Unicode、日期时间、复合类型（array/map/struct）、真·大结果集（>1w 行）。</para>
/// <para>默认跳过——需配置 MAXCOMPUTE_TEST_* 环境变量（与 <see cref="MaxComputeIntegrationTestBase"/> 一致）。</para>
/// </summary>
[Trait("Category", "Integration")]
public class TypeCoverageIntegrationTest
{
    private readonly ITestOutputHelper _out;
    public TypeCoverageIntegrationTest(ITestOutputHelper o) => _out = o;

    private static MaxComputeConfig? TryConfig(bool odps2Hints = false)
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
            Project = project!, Region = region, MaxRows = 200000, UseV4Signature = true
        };
        var tunnel = Environment.GetEnvironmentVariable("MAXCOMPUTE_TEST_TUNNEL_ENDPOINT");
        if (!string.IsNullOrWhiteSpace(tunnel)) config.TunnelEndpoint = tunnel!;
        if (odps2Hints)
            config.Hints = new Dictionary<string, string> { ["odps.sql.type.system.odps2"] = "true" };
        return config;
    }

    private static async Task<QueryResult> RunAsync(MaxComputeConfig config, string sql)
    {
        var executor = new DirectOdpsQueryExecutor(new SimpleHttpClientFactory(), NullLogger<DirectOdpsQueryExecutor>.Instance);
        return await executor.ExecuteQueryAsync(config, sql);
    }

    /// <summary>
    /// 强制走 Result API（CSV）路径：preferTunnel=false，跳过 Tunnel 直连。
    /// </summary>
    private static async Task<QueryResult> RunViaResultApiAsync(MaxComputeConfig config, string sql)
    {
        var executor = new DirectOdpsQueryExecutor(new SimpleHttpClientFactory(), NullLogger<DirectOdpsQueryExecutor>.Instance, preferTunnel: false);
        return await executor.ExecuteQueryAsync(config, sql);
    }

    // ---------- 标量类型 ----------

    [Fact]
    public async Task ScalarTypes_AllDecoded()
    {
        var config = TryConfig();
        if (config is null) return;

        var sql = "SELECT " +
                  "CAST(127 AS TINYINT) AS c_tinyint, " +
                  "CAST(32000 AS SMALLINT) AS c_smallint, " +
                  "CAST(200000 AS INT) AS c_int, " +
                  "CAST(9000000000 AS BIGINT) AS c_bigint, " +
                  "CAST(1.5 AS FLOAT) AS c_float, " +
                  "CAST(3.141592653 AS DOUBLE) AS c_double, " +
                  "CAST(TRUE AS BOOLEAN) AS c_bool, " +
                  "'hello' AS c_string, " +
                  "CAST(99.99 AS DECIMAL(10,2)) AS c_decimal";

        var r = await RunAsync(config, sql);
        Assert.True(r.RowCount > 0, $"no rows; cols={r.ColumnTypes?.Length}");
        Assert.NotNull(r.ColumnTypes);
        // 列类型应包含关键字
        Assert.Contains("tinyint", r.ColumnTypes!);
        Assert.Contains("smallint", r.ColumnTypes!);
        Assert.Contains("int", r.ColumnTypes!);
        Assert.Contains("bigint", r.ColumnTypes!);
        Assert.Contains("float", r.ColumnTypes!);
        Assert.Contains("double", r.ColumnTypes!);
        Assert.Contains("boolean", r.ColumnTypes!);
        Assert.Contains("string", r.ColumnTypes!);
        // decimal 类型字符串带精度，如 "decimal(10,2)"，用谓词匹配
        Assert.Contains(r.ColumnTypes!, t => t.StartsWith("decimal", StringComparison.OrdinalIgnoreCase));

        var row = r.Rows[0];
        _out.WriteLine("scalar row: " + string.Join(" | ", row.Select(v => v?.ToString() ?? "NULL")));
        // IntegerDecoder（tinyint/smallint/int/bigint）一律返回 long
        Assert.Equal(127L, row[0]);
        Assert.Equal(32000L, row[1]);
        Assert.Equal(200000L, row[2]);
        Assert.Equal(9000000000L, row[3]);
        Assert.True(Convert.ToDouble(row[4]) > 1.4 && Convert.ToDouble(row[4]) < 1.6);
        Assert.True(Math.Abs(Convert.ToDouble(row[5]) - 3.141592653) < 1e-6);
        Assert.Equal(true, row[6]);
        Assert.Equal("hello", row[7]);
        Assert.Equal(99.99m, Convert.ToDecimal(row[8]));
    }

    // ---------- NULL（字段缺省） ----------

    [Fact]
    public async Task NullValues_DecodedAsDbNull()
    {
        var config = TryConfig();
        if (config is null) return;

        var sql = "SELECT " +
                  "CAST(NULL AS BIGINT) AS a, " +
                  "CAST(NULL AS STRING) AS b, " +
                  "CAST(NULL AS DOUBLE) AS c, " +
                  "CAST(NULL AS BOOLEAN) AS d, " +
                  "CAST(NULL AS DECIMAL(10,2)) AS e";

        var r = await RunAsync(config, sql);
        Assert.True(r.RowCount > 0);
        var row = r.Rows[0];
        // NULL 字段不随 wire 传输（字段缺省），reader 保持初始 DBNull
        Assert.Equal(DBNull.Value, row[0]);
        Assert.Equal(DBNull.Value, row[1]);
        Assert.Equal(DBNull.Value, row[2]);
        Assert.Equal(DBNull.Value, row[3]);
        Assert.Equal(DBNull.Value, row[4]);
    }

    // ---------- 混合 NULL 与非 NULL（同一记录） ----------

    [Fact]
    public async Task MixedNullAndValue_InSameRow()
    {
        var config = TryConfig();
        if (config is null) return;

        var sql = "SELECT CAST(1 AS BIGINT) AS a, CAST(NULL AS STRING) AS b, 'x' AS c, CAST(NULL AS BIGINT) AS d";

        var r = await RunAsync(config, sql);
        Assert.True(r.RowCount > 0);
        var row = r.Rows[0];
        Assert.Equal(1L, row[0]);
        Assert.Equal(DBNull.Value, row[1]);
        Assert.Equal("x", row[2]);
        Assert.Equal(DBNull.Value, row[3]);
    }

    // ---------- Unicode ----------

    [Fact]
    public async Task UnicodeString_Decoded()
    {
        var config = TryConfig();
        if (config is null) return;

        var r = await RunAsync(config, "SELECT '你好世界🚀αβγ' AS u");
        Assert.True(r.RowCount > 0);
        Assert.Equal("你好世界🚀αβγ", r.Rows[0][0]);
    }

    // ---------- 日期时间 ----------

    [Fact]
    public async Task DateTimeTypes_Decoded()
    {
        var config = TryConfig(odps2Hints: true);
        if (config is null) return;

        var sql = "SELECT " +
                  "CAST('2026-06-20' AS DATE) AS c_date, " +
                  "CAST('2026-06-20 12:34:56' AS DATETIME) AS c_datetime, " +
                  "CAST('2026-06-20 12:34:56.789' AS TIMESTAMP) AS c_timestamp";

        var r = await RunAsync(config, sql);
        Assert.True(r.RowCount > 0, $"no rows");
        Assert.NotNull(r.ColumnTypes);
        Assert.Contains("date", r.ColumnTypes!);
        Assert.Contains("datetime", r.ColumnTypes!);
        Assert.Contains("timestamp", r.ColumnTypes!);

        var row = r.Rows[0];
        _out.WriteLine("datetime row: " + string.Join(" | ", row.Select(v => v?.ToString() ?? "NULL")));
        Assert.NotNull(row[0]);
        Assert.NotNull(row[1]);
        Assert.NotNull(row[2]);
        Assert.Contains("2026", row[1]!.ToString()!);
    }

    [Fact]
    public async Task TimestampNtz_Decoded()
    {
        var config = TryConfig(odps2Hints: true);
        if (config is null) return;

        var r = await RunAsync(config, "SELECT CAST('2026-06-20 12:34:56.789' AS TIMESTAMP_NTZ) AS t");
        Assert.True(r.RowCount > 0);
        Assert.NotNull(r.ColumnTypes);
        Assert.Contains(r.ColumnTypes!, t => t.StartsWith("timestamp_ntz", StringComparison.OrdinalIgnoreCase));
        Assert.NotNull(r.Rows[0][0]);
        Assert.Contains("2026", r.Rows[0][0]!.ToString()!);
    }

    [Fact]
    public async Task JsonType_Decoded()
    {
        var config = TryConfig(odps2Hints: true);
        if (config is null) return;

        var r = await RunAsync(config, "SELECT CAST('{\"a\":1,\"b\":\"x\"}' AS JSON) AS j");
        Assert.True(r.RowCount > 0);
        Assert.NotNull(r.ColumnTypes);
        Assert.Contains(r.ColumnTypes!, t => t.StartsWith("json", StringComparison.OrdinalIgnoreCase));
        // JsonDecoder 返回原始字节为字符串。MaxCompute JSON 经 tunnel 以序列化字符串形式返回
        // （如 "{\"a\":1,\"b\":\"x\"}"，键值带转义），内容存活即可；调用方需时可自行 JsonDocument.Parse。
        var text = r.Rows[0][0]!.ToString();
        Assert.Contains("a", text);
        Assert.Contains("b", text);
        Assert.Contains("1", text);
        Assert.Contains("x", text);
    }

    // ---------- 复合类型 ----------

    [Fact]
    public async Task ArrayType_Decoded()
    {
        var config = TryConfig(odps2Hints: true);
        if (config is null) return;

        var r = await RunAsync(config, "SELECT ARRAY(CAST(1 AS BIGINT), CAST(2 AS BIGINT), CAST(3 AS BIGINT)) AS arr");
        Assert.True(r.RowCount > 0);
        var arr = r.Rows[0][0] as System.Collections.IList;
        Assert.NotNull(arr);
        Assert.Equal(3, arr!.Count);
        Assert.Equal(1L, arr[0]);
        Assert.Equal(2L, arr[1]);
        Assert.Equal(3L, arr[2]);
    }

    [Fact]
    public async Task MapType_Decoded()
    {
        var config = TryConfig(odps2Hints: true);
        if (config is null) return;

        var r = await RunAsync(config, "SELECT MAP('a', CAST(10 AS BIGINT), 'b', CAST(20 AS BIGINT)) AS m");
        Assert.True(r.RowCount > 0);
        var dict = r.Rows[0][0] as System.Collections.IDictionary;
        Assert.NotNull(dict);
        Assert.Equal(2, dict!.Count);
    }

    [Fact]
    public async Task StructType_Decoded()
    {
        var config = TryConfig(odps2Hints: true);
        if (config is null) return;

        var r = await RunAsync(config, "SELECT NAMED_STRUCT('a', CAST(1 AS BIGINT), 'b', 'hello') AS s");
        Assert.True(r.RowCount > 0);
        var arr = r.Rows[0][0] as object[];
        Assert.NotNull(arr);
        Assert.Equal(2, arr!.Length);
        Assert.Equal(1L, arr[0]);
        Assert.Equal("hello", arr[1]);
    }

    // ---------- 真·大结果集（>1w 行，验证 Tunnel 全量 + 多批次） ----------

    [Fact]
    public async Task LargeResultSet_OverTenThousandRows()
    {
        var config = TryConfig();
        if (config is null) return;

        // 用 explode 造 >1w 行，无需依赖任何已有表。
        // 注意：MaxCompute 的 split(repeat(',',N),',') 对全分隔符串返回 0 元素，必须用带实际内容的 repeat。
        var sql = "SELECT v FROM (SELECT explode(split(repeat('ab,', 15000), ',')) AS v) t";

        var r = await RunAsync(config, sql);
        _out.WriteLine($"large result row count = {r.RowCount}");
        // 关键断言：Tunnel 路径必须返回 > 10000 行（Result API 会在 10000 截断）
        Assert.True(r.RowCount > 10000, $"expected >10000 rows via Tunnel, got {r.RowCount}");
    }

    /// <summary>
    /// 单次下载 50000 行必须全量返回：验证 MaxCompute InstanceTunnel 单流单批次语义——
    /// 服务端不按行数截断、单流内不出现多个 META_COUNT 尾部（否则 reader 会在首个尾部后停止丢行）。
    /// </summary>
    [Fact]
    public async Task LargeResultSet_FiftyThousandRows_AllReturned()
    {
        var config = TryConfig();
        if (config is null) return;

        const int n = 50000;
        var sql = $"SELECT v FROM (SELECT explode(split(repeat('ab,', {n}), ',')) AS v) t";

        var r = await RunAsync(config, sql);
        _out.WriteLine($"50k result row count = {r.RowCount}");
        Assert.Equal(n, r.RowCount);
    }

    // ---------- binary / 定长字符 ----------

    [Fact]
    public async Task BinaryType_Decoded()
    {
        var config = TryConfig(odps2Hints: true);
        if (config is null) return;

        var r = await RunAsync(config, "SELECT CAST('hello' AS BINARY) AS b");
        Assert.True(r.RowCount > 0);
        Assert.NotNull(r.ColumnTypes);
        Assert.Contains(r.ColumnTypes!, t => t.StartsWith("binary", StringComparison.OrdinalIgnoreCase));
        Assert.Equal("hello", r.Rows[0][0]);
    }

    [Fact]
    public async Task VarcharAndChar_Decoded()
    {
        var config = TryConfig(odps2Hints: true);
        if (config is null) return;

        var sql = "SELECT CAST('hi' AS VARCHAR(10)) AS v, CAST('hi' AS CHAR(5)) AS c";
        var r = await RunAsync(config, sql);
        Assert.True(r.RowCount > 0);
        Assert.NotNull(r.ColumnTypes);
        Assert.Contains(r.ColumnTypes!, t => t.StartsWith("varchar", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(r.ColumnTypes!, t => t.StartsWith("char", StringComparison.OrdinalIgnoreCase));

        // VARCHAR 不补齐；CHAR(n) 右补空格到 n
        Assert.Equal("hi", r.Rows[0][0]);
        Assert.Equal("hi   ", r.Rows[0][1]);   // 'hi' + 3 空格 = 5
        Assert.Equal(5, ((string)r.Rows[0][1]!).Length);
    }

    // ---------- 嵌套复合类型（验证 TypeStringParser 递归） ----------

    [Fact]
    public async Task NestedArrayType_Decoded()
    {
        var config = TryConfig(odps2Hints: true);
        if (config is null) return;

        var r = await RunAsync(config,
            "SELECT ARRAY(ARRAY(CAST(1 AS BIGINT), CAST(2 AS BIGINT)), ARRAY(CAST(3 AS BIGINT))) AS a");
        Assert.True(r.RowCount > 0);
        var outer = r.Rows[0][0] as System.Collections.IList;
        Assert.NotNull(outer);
        Assert.Equal(2, outer!.Count);
        var first = outer[0] as System.Collections.IList;
        Assert.NotNull(first);
        Assert.Equal(2, first!.Count);
        Assert.Equal(1L, first[0]);
    }

    [Fact]
    public async Task NestedMapType_Decoded()
    {
        var config = TryConfig(odps2Hints: true);
        if (config is null) return;

        var r = await RunAsync(config, "SELECT MAP('k', ARRAY(CAST(1 AS BIGINT), CAST(2 AS BIGINT))) AS m");
        Assert.True(r.RowCount > 0);
        var dict = r.Rows[0][0] as System.Collections.IDictionary;
        Assert.NotNull(dict);
        Assert.Equal(1, dict!.Count);
        var vals = dict["k"] as System.Collections.IList;
        Assert.NotNull(vals);
        Assert.Equal(2, vals!.Count);
    }

    [Fact]
    public async Task NestedStructType_Decoded()
    {
        var config = TryConfig(odps2Hints: true);
        if (config is null) return;

        // struct 字段 b 为复合类型 array<string>（曾导致 parser 把 'b:array' 当复合类型名而崩）
        var r = await RunAsync(config, "SELECT NAMED_STRUCT('a', CAST(1 AS BIGINT), 'b', ARRAY('x','y')) AS s");
        Assert.True(r.RowCount > 0);
        var arr = r.Rows[0][0] as object[];
        Assert.NotNull(arr);
        Assert.Equal(2, arr!.Length);
        Assert.Equal(1L, arr[0]);
        var inner = arr[1] as System.Collections.IList;
        Assert.NotNull(inner);
        Assert.Equal(2, inner!.Count);
    }

    // ---------- Result API（CSV）回退路径 ----------

    [Fact]
    public async Task ResultApiPath_Decoded()
    {
        var config = TryConfig();
        if (config is null) return;

        // preferTunnel=false 强制走 Result API（CSV），覆盖 CsvResultParser 端到端路径
        var r = await RunViaResultApiAsync(config, "SELECT CAST(1 AS BIGINT) AS a, 'x' AS b");
        Assert.True(r.RowCount > 0, "Result API path returned no rows");
        _out.WriteLine("result-api row: " + string.Join(" | ", r.Rows[0].Select(v => v?.ToString() ?? "NULL")));
        Assert.Equal("1", r.Rows[0][0]?.ToString());
        Assert.Equal("x", r.Rows[0][1]?.ToString());
    }
}

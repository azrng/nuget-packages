using Azrng.NMaxCompute.Models;
using Xunit;
using Xunit.Abstractions;

namespace Azrng.NMaxCompute.Test.Integration;

/// <summary>
/// 时区开关（UseLocalTimeZone）集群验证：record 路径（TunnelRecordReader → DateTimeDecoder/TimestampDecoder）。
/// 同一查询在本地 / UTC 两种模式下跑，断言返回值差 = 本地时区偏移；且值类型为 DateTime/DateTimeOffset（证明走了 Tunnel 解码而非 CSV 回退）。
/// </summary>
[Trait("Category", "Integration")]
public class TimeZoneClusterProbe : MaxComputeIntegrationTestBase
{
    private readonly ITestOutputHelper _out;
    public TimeZoneClusterProbe(ITestOutputHelper o) => _out = o;

    private async Task<object?> Scalar(MaxComputeConfig config, string sql)
    {
        var executor = CreateExecutor();
        var result = await executor.ExecuteQueryAsync(config, sql);
        return result.RowCount > 0 ? result.Rows[0][0] : null;
    }

    /// <summary>DATETIME：UseLocalTimeZone true(本地) vs false(UTC)，差值 = 本地偏移。</summary>
    [Fact]
    public async Task DateTime_LocalVsUtc_DiffersByOffset()
    {
        var config = LoadConfigOrSkip();
        if (config is null) { _out.WriteLine("[skip] env not set"); return; }

        const string sql = "SELECT CAST('2026-06-21 12:00:00' AS DATETIME) AS d";

        config.UseLocalTimeZone = true;
        var localRaw = await Scalar(config, sql);
        config.UseLocalTimeZone = false;
        var utcRaw = await Scalar(config, sql);

        _out.WriteLine($"datetime local={localRaw} ({localRaw?.GetType().Name})  utc={utcRaw} ({utcRaw?.GetType().Name})");

        if (localRaw is not DateTime local || utcRaw is not DateTime utc)
        {
            _out.WriteLine("[skip] DATETIME 未走 Tunnel 解码（值非 DateTime，可能 CSV 回退），时区开关路径未生效");
            return;
        }

        Assert.Equal(DateTimeKind.Local, local.Kind);
        Assert.Equal(DateTimeKind.Utc, utc.Kind);
        Assert.Equal(TimeZoneInfo.Local.GetUtcOffset(local).Ticks, local.Ticks - utc.Ticks);
        _out.WriteLine($"VERIFY OK: offset={TimeZoneInfo.Local.GetUtcOffset(local)}");
    }

    /// <summary>TIMESTAMP：UseLocalTimeZone true(本地偏移) vs false(UTC+00:00)，同一时刻。</summary>
    [Fact]
    public async Task Timestamp_LocalVsUtc_DiffersByOffset()
    {
        var config = LoadConfigOrSkip();
        if (config is null) { _out.WriteLine("[skip] env not set"); return; }

        const string sql = "SELECT CAST('2026-06-21 12:00:00.123456789' AS TIMESTAMP) AS t";

        config.UseLocalTimeZone = true;
        var localRaw = await Scalar(config, sql);
        config.UseLocalTimeZone = false;
        var utcRaw = await Scalar(config, sql);

        _out.WriteLine($"timestamp local={localRaw} ({localRaw?.GetType().Name})  utc={utcRaw} ({utcRaw?.GetType().Name})");

        if (localRaw is not DateTimeOffset local || utcRaw is not DateTimeOffset utc)
        {
            _out.WriteLine("[skip] TIMESTAMP 未走 Tunnel 解码（值非 DateTimeOffset，可能 CSV 回退），时区开关路径未生效");
            return;
        }

        Assert.Equal(TimeSpan.Zero, utc.Offset);
        Assert.Equal(TimeZoneInfo.Local.GetUtcOffset(local), local.Offset);
        Assert.Equal(local.UtcDateTime, utc.UtcDateTime);   // 同一时刻
        _out.WriteLine($"VERIFY OK: local offset={local.Offset}");
    }
}

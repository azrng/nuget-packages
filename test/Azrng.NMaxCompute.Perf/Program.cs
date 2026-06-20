using System.Diagnostics;
using Azrng.NMaxCompute.Models;
using Azrng.NMaxCompute.Provider;
using Azrng.NMaxCompute.Tunnel;
using Azrng.NMaxCompute.Tunnel.Types;
using Azrng.NMaxCompute.Tunnel.Wire;
using Microsoft.Extensions.Logging.Abstractions;

namespace Azrng.NMaxCompute.Perf;

/// <summary>
/// Azrng.NMaxCompute 端到端性能基准（直连真实 MaxCompute 集群）。
/// <para>
/// 运行：配置 MAXCOMPUTE_TEST_* 环境变量后
/// <c>dotnet run --project test/Azrng.NMaxCompute.Perf -c Release</c>
/// </para>
/// <para>不是单元测试项目（IsTestProject=false），不会被 <c>dotnet test</c> 收纳。</para>
/// </summary>
internal static class Program
{
    private const int DefaultRows = 50000;
    private const int WarmupRepeats = 1;

    private static async Task<int> Main(string[] args)
    {
        var config = TryLoadConfig();
        if (config is null)
        {
            Console.Error.WriteLine("缺少 MAXCOMPUTE_TEST_* 环境变量，跳过性能测试。");
            Console.Error.WriteLine("必需：ENDPOINT / ACCESS_ID / SECRET_KEY / PROJECT / REGION");
            Console.Error.WriteLine("可选：TUNNEL_ENDPOINT、MAXCOMPUTE_PERF_ROWS（默认 50000）");
            return 2;
        }

        var rows = ParseRows();
        var executor = new DirectOdpsQueryExecutor(
            new SingleHttpClientFactory(), NullLogger<DirectOdpsQueryExecutor>.Instance);

        Console.WriteLine($"Azrng.NMaxCompute 性能基准  project={config.Project} region={config.Region} rows={rows}");
        Console.WriteLine(new string('-', 76));

        // [A] 冷启动延迟：SELECT 1（含实例提交 + 等待 + Tunnel 拉取）
        var cold = await Measure(() => executor.ExecuteQueryAsync(config, "SELECT 1"));
        Console.WriteLine($"{"[A] 冷启动 SELECT 1",-46}{cold,8:N0} ms");

        // [B] 大结果吞吐：explode 造 rows 行，单次拉取
        var bigSql = $"SELECT v FROM (SELECT explode(split(repeat('ab,', {rows}), ',')) AS v) t";
        var (bigMs, bigRows) = await MeasureRows(() => executor.ExecuteQueryAsync(config, bigSql));
        var rps = bigMs > 0 ? bigRows * 1000.0 / bigMs : 0;
        var usPerRow = bigRows > 0 ? (double)bigMs * 1000 / bigRows : 0;
        Console.WriteLine($"{"[B] 大结果拉取",-46}{bigMs,8:N0} ms   rows={bigRows:N0}");
        Console.WriteLine($"{"    吞吐",-46}{rps,8:N0} rows/s");
        Console.WriteLine($"{"    每行耗时",-46}{usPerRow,8:N2} µs/row");

        // [C] 混合类型解码：5 列单行（bigint/double/string/bool/decimal）
        var mixedSql = "SELECT CAST(1 AS BIGINT) a, CAST(1.5 AS DOUBLE) b, 'text' c, " +
                       "CAST(TRUE AS BOOLEAN) d, CAST(3.14 AS DECIMAL(10,2)) e";
        var mixed = await Measure(() => executor.ExecuteQueryAsync(config, mixedSql));
        Console.WriteLine($"{"[C] 混合类型（5 列）",-46}{mixed,8:N0} ms");

        // [D] 连续小查询平均延迟（排除首次冷启动）
        const int smallN = 3;
        var smallSum = 0L;
        for (var i = 0; i < WarmupRepeats; i++)
            await Measure(() => executor.ExecuteQueryAsync(config, "SELECT 1"));
        for (var i = 0; i < smallN; i++)
            smallSum += await Measure(() => executor.ExecuteQueryAsync(config, "SELECT 1"));
        Console.WriteLine($"{"[D] 小查询平均（" + smallN + " 次）",-46}{(double)smallSum / smallN,8:N0} ms");

        // [E] 离线 wire 解码吞吐：内存构造 100w 行 bigint wire 流，纯本地解码（隔离网络/服务端）
        const int decodeRows = 1_000_000;
        var (decodeMs, decoded) = OfflineDecode(decodeRows);
        var decodeRps = decodeMs > 0 ? decoded * 1000.0 / decodeMs : 0;
        Console.WriteLine($"{"[E] 离线 wire 解码（" + decodeRows + " 行）",-46}{decodeMs,8:N0} ms");
        Console.WriteLine($"{"    纯解码吞吐",-46}{decodeRps,8:N0} rows/s");

        Console.WriteLine(new string('-', 76));
        Console.WriteLine("说明：每次查询含实例提交+等待+Tunnel 拉取；[B] 吞吐受网络与服务端影响，仅供参考。");
        return 0;
    }

    private static async Task<long> Measure(Func<Task<QueryResult>> action)
    {
        var sw = Stopwatch.StartNew();
        await action().ConfigureAwait(false);
        return sw.ElapsedMilliseconds;
    }

    private static async Task<(long Ms, int Rows)> MeasureRows(Func<Task<QueryResult>> action)
    {
        var sw = Stopwatch.StartNew();
        var r = await action().ConfigureAwait(false);
        return (sw.ElapsedMilliseconds, r.RowCount);
    }

    private static int ParseRows()
    {
        var v = Environment.GetEnvironmentVariable("MAXCOMPUTE_PERF_ROWS");
        return int.TryParse(v, out var n) && n > 0 ? n : DefaultRows;
    }

    private static MaxComputeConfig? TryLoadConfig()
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
            Endpoint = endpoint!,
            AccessId = accessId!,
            SecretAccessKey = secret!,
            Project = project!,
            Region = region,
            MaxRows = 500000,
            UseV4Signature = true
        };
        var tunnel = Environment.GetEnvironmentVariable("MAXCOMPUTE_TEST_TUNNEL_ENDPOINT");
        if (!string.IsNullOrWhiteSpace(tunnel))
            config.TunnelEndpoint = tunnel!;
        return config;
    }

    /// <summary>
    /// 极简 IHttpClientFactory：复用单个 HttpClient（与集成测试一致的共享语义）。
    /// </summary>
    private sealed class SingleHttpClientFactory : IHttpClientFactory
    {
        private static readonly HttpClient Shared = new();
        public HttpClient CreateClient(string name) => Shared;
    }

    /// <summary>
    /// 内存构造 N 行 bigint 的 Tunnel wire 流并解码，返回 (耗时 ms, 解码行数)。
    /// 单列 bigint，结构与真实服务端一致（每行 field1+sint64+END_RECORD+CRC，尾部 META_COUNT+META_CHECKSUM）。
    /// </summary>
    private static (long Ms, int Rows) OfflineDecode(int rows)
    {
        var ms = new MemoryStream();
        var crccrc = new Checksum();

        for (long i = 0; i < rows; i++)
        {
            var value = i;
            var crc = new Checksum();
            crc.UpdateInt(1);            // field index
            crc.UpdateLong(value);       // bigint 值
            var crcVal = crc.GetValue();
            crccrc.UpdateInt((int)crcVal);

            WriteTag(ms, 1, 0);                       // field 1, varint
            WriteSInt64(ms, value);
            WriteTag(ms, TunnelWireConstants.TunnelEndRecord, 0);
            WriteVarUInt(ms, crcVal);
        }

        WriteTag(ms, TunnelWireConstants.TunnelMetaCount, 0);
        WriteSInt64(ms, rows);
        WriteTag(ms, TunnelWireConstants.TunnelMetaChecksum, 0);
        WriteVarUInt(ms, crccrc.GetValue());

        ms.Position = 0;
        var decoders = new[] { TypeDecoderFactory.GetDecoder("bigint") };
        using var reader = new TunnelRecordReader(ms, decoders);

        var sw = Stopwatch.StartNew();
        var count = 0;
        while (reader.Read() != null)
            count++;
        sw.Stop();
        return (sw.ElapsedMilliseconds, count);
    }

    private static void WriteVarUInt(MemoryStream ms, uint v)
    {
        while (v > 0x7F) { ms.WriteByte((byte)(v | 0x80)); v >>= 7; }
        ms.WriteByte((byte)v);
    }

    private static void WriteSInt64(MemoryStream ms, long v)
    {
        var zz = (ulong)((v << 1) ^ (v >> 63));
        while (zz > 0x7F) { ms.WriteByte((byte)(zz | 0x80)); zz >>= 7; }
        ms.WriteByte((byte)zz);
    }

    private static void WriteTag(MemoryStream ms, int field, int wireType)
        => WriteVarUInt(ms, (uint)(((ulong)field << 3) | (uint)wireType));
}

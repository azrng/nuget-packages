using Apache.Arrow;
using Apache.Arrow.Arrays;
using Apache.Arrow.Types;
using Azrng.NMaxCompute.Accounts;
using Azrng.NMaxCompute.Core;
using Azrng.NMaxCompute.Rest;
using Azrng.NMaxCompute.Tunnel;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using Xunit.Abstractions;

namespace Azrng.NMaxCompute.Arrow.Test;

/// <summary>探针：查 timestamp/decimal 列时，Arrow 读是否需要 struct wire schema（PyODPS timestamp-as-struct）。</summary>
[Trait("Category", "Integration")]
public class ArrowTsProbe
{
    private sealed class F : IHttpClientFactory { private static readonly HttpClient S = new(); public HttpClient CreateClient(string n) => S; }
    private readonly ITestOutputHelper _out;
    public ArrowTsProbe(ITestOutputHelper o) => _out = o;

    private async Task Probe(string label, string sql, Action<Schema, RecordBatch?>? verify = null)
    {
        var endpoint = Environment.GetEnvironmentVariable("MAXCOMPUTE_TEST_ENDPOINT");
        var accessId = Environment.GetEnvironmentVariable("MAXCOMPUTE_TEST_ACCESS_ID");
        var secret = Environment.GetEnvironmentVariable("MAXCOMPUTE_TEST_SECRET_KEY");
        var project = Environment.GetEnvironmentVariable("MAXCOMPUTE_TEST_PROJECT");
        var region = Environment.GetEnvironmentVariable("MAXCOMPUTE_TEST_REGION");
        var tunnelEp = Environment.GetEnvironmentVariable("MAXCOMPUTE_TEST_TUNNEL_ENDPOINT") ?? endpoint;
        if (endpoint is null || accessId is null || secret is null || project is null || region is null)
        { _out.WriteLine("[skip]"); return; }

        var f = new F();
        var account = new CloudAccount(accessId, secret, region, useV4Signature: true);
        var api = new OdpsRestClient(f.CreateClient(), account, endpoint!, NullLogger<OdpsRestClient>.Instance);
        var tunnel = new OdpsRestClient(f.CreateClient(), account, tunnelEp!, NullLogger<OdpsRestClient>.Instance);
        var odps = new Odps(api, project!);

        try
        {
            var inst = await odps.RunSqlAsync(sql);
            await inst.WaitForTerminationAsync(TimeSpan.FromMinutes(10));
            var session = await InstanceDownloadSession.CreateAsync(tunnel, project!, inst.Id);
            _out.WriteLine($"[{label}] odps cols: {string.Join(",", session.Schema.Columns.Select(c => $"{c.Name}:{c.Type}"))}");
            using var arrow = await session.OpenArrowReaderAsync(0, Math.Max(1, session.RecordCount));
            _out.WriteLine($"[{label}] arrow schema: {string.Join(",", arrow.Schema.FieldsList.Select(x => $"{x.Name}:{x.DataType.Name}"))}");
            var batch = arrow.ReadNext();
            _out.WriteLine($"[{label}] OK batch len={batch?.Length}");
            verify?.Invoke(arrow.Schema, batch);
            _out.WriteLine($"[{label}] VERIFY OK");
        }
        catch (Exception ex) { _out.WriteLine($"[{label}] ERR {ex.GetType().Name}: {ex.Message.Split('\n')[0]}"); }
    }

    [Fact]
    public async Task Run()
    {
        await Probe("datetime", "SELECT CAST('2026-06-21 12:00:00' AS DATETIME) AS d");
        await Probe("decimal", "SELECT CAST(3.14 AS DECIMAL(10,2)) AS c");
        // timestamp(ns)：服务端按 struct(sec,nano) 发送，reader 应转回 TimestampArray；
        // 公共 schema 呈现 timestamp，列值小数部分纳秒 = 123456789（与时区无关，验证精度保留）。
        await Probe("timestamp", "SELECT CAST('2026-06-21 12:00:00.123456789' AS TIMESTAMP) AS t", (schema, batch) =>
        {
            Assert.True(schema.FieldsList[0].DataType is TimestampType tsType && tsType.Unit == TimeUnit.Nanosecond,
                $"public schema 应为 timestamp(ns)，实际 {schema.FieldsList[0].DataType.Name}");
            var col = Assert.IsType<TimestampArray>(batch!.Column(0));
            Assert.Equal(123_456_789L, col.Values[0] % 1_000_000_000L);
        });
    }
}

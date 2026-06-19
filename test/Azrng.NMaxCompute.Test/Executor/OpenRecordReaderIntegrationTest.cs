using System.Net;
using System.Net.Http.Headers;
using Azrng.NMaxCompute.Accounts;
using Azrng.NMaxCompute.Rest;
using Azrng.NMaxCompute.Tunnel;
using Azrng.NMaxCompute.Tunnel.Wire;
using Xunit;

namespace Azrng.NMaxCompute.Test.Executor;

/// <summary>
/// 用本地 HttpMessageHandler 假冒 Tunnel 服务端，验证 OpenRecordReaderAsync 端到端：
/// 请求拼接（?data&downloadid&rowrange&x-odps-tunnel-version）+ 流式读取 + wire 解码 + CRC 校验。
/// </summary>
public class OpenRecordReaderIntegrationTest
{
    [Fact]
    public async Task OpenRecordReader_ParsesWireStream()
    {
        // 构造一条 tunnel wire 流：1 列 bigint，2 行 [10, 20]
        var wire = BuildTwoRowsOfBigInt(10L, 20L);

        var handler = new StubHandler(
            statusCode: 200,
            contentType: "application/octet-stream",
            body: wire);

        var httpClient = new HttpClient(handler);
        var account = new CloudAccount("test-id", "test-secret", region: null, useV4Signature: false);
        var client = new OdpsRestClient(httpClient, account, "http://tunnel.test/api", logger: null);

        // 用 FromResponseJsonForTest 预填 schema/recordCount/id，绕过真实的 session 创建请求
        var session = InstanceDownloadSession.FromResponseJsonForTest(
            "proj", "inst",
            @"{""DownloadID"":""dl-1"",""Status"":""NORMAL"",""RecordCount"":2," +
            @"""Schema"":{""columns"":[{""name"":""v"",""type"":""bigint""}]}}");

        // 让 session 使用上面这个 client。session 的 _client 是 readonly，通过反射注入（仅测试）
        InjectClient(session, client);

        using var reader = await session.OpenRecordReaderAsync(0, 2);

        var r1 = reader.Read();
        var r2 = reader.Read();
        var r3 = reader.Read();

        Assert.NotNull(r1);
        Assert.Equal(10L, r1![0]);
        Assert.NotNull(r2);
        Assert.Equal(20L, r2![0]);
        Assert.Null(r3);

        // 验证请求 URL 包含必要的 query（rowrange 的括号会被 URL 编码为 %28 %29）
        Assert.Contains("downloadid=dl-1", handler.LastRequestUri);
        Assert.Contains("rowrange=", handler.LastRequestUri);
        Assert.Contains("0%2C2", handler.LastRequestUri);   // "0,2" 编码后
        Assert.Contains("data", handler.LastRequestUri);
        Assert.Contains("x-odps-tunnel-version", handler.LastRequestHeaders ?? "");
    }

    private static byte[] BuildTwoRowsOfBigInt(long a, long b)
    {
        var ms = new MemoryStream();
        WriteRow(ms, a);
        WriteRow(ms, b);
        return ms.ToArray();
    }

    private static void WriteRow(MemoryStream ms, long value)
    {
        WriteTag(ms, 1, 0);                  // field 1, varint
        WriteSInt64(ms, value);

        var crc = new Checksum();
        crc.UpdateInt(1);
        crc.UpdateLong(value);
        WriteTag(ms, TunnelWireConstants.TunnelEndRecord, 0);
        WriteVarUInt(ms, crc.GetValue());
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

    private static void WriteTag(MemoryStream ms, int field, int wire)
    {
        var tag = ((ulong)field << 3) | (uint)wire;
        while (tag > 0x7F) { ms.WriteByte((byte)(tag | 0x80)); tag >>= 7; }
        ms.WriteByte((byte)tag);
    }

    private static void InjectClient(InstanceDownloadSession session, OdpsRestClient client)
    {
        var field = typeof(InstanceDownloadSession).GetField("_client",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.NotNull(field);
        field!.SetValue(session, client);
    }

    /// <summary>
    /// 极简 HttpMessageHandler：所有请求返回固定响应，并记录最后一个 URI。
    /// </summary>
    private sealed class StubHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode;
        private readonly string _contentType;
        private readonly byte[] _body;

        public StubHandler(int statusCode, string contentType, byte[] body)
        {
            _statusCode = (HttpStatusCode)statusCode;
            _contentType = contentType;
            _body = body;
        }

        public string? LastRequestUri { get; private set; }

        public string? LastRequestHeaders { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequestUri = request.RequestUri?.AbsoluteUri ?? string.Empty;
            LastRequestHeaders = string.Join(",", request.Headers.Select(h => $"{h.Key}={string.Join(";", h.Value)}"));
            var response = new HttpResponseMessage(_statusCode)
            {
                Content = new ByteArrayContent(_body)
            };
            response.Content.Headers.ContentType = new MediaTypeHeaderValue(_contentType);
            return Task.FromResult(response);
        }
    }
}

using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Common.HttpClients;

namespace Common.HttpClients.Next.Test.Integration;

/// <summary>
/// 非命名客户端（default）模式集成测试。
/// 与 <see cref="ApifoxEchoIntegrationTests"/>（命名模式，经 IHttpHelperFactory.CreateClient(name)）互补，
/// 这里直接注入 <see cref="IHttpHelper"/>，覆盖 Startup 中 <c>AddHttpClientService(options)</c>
/// 注册的 default 客户端路径。重点回归：删除多余 TryAddTransient&lt;LoggingHandler&gt; 注册后，
/// 非命名注入与日志处理器工厂创建链路仍可正常工作。
/// 这些测试会发起真实网络请求，需在联网环境执行；
/// 离线环境可通过 <c>--filter Category!=Integration</c> 跳过。
/// </summary>
[Trait("Category", "Integration")]
public class ApifoxEchoDefaultClientIntegrationTests
{
    private readonly IHttpHelper _http;

    // 直接注入 IHttpHelper（非命名模式），而非 IHttpHelperFactory
    public ApifoxEchoDefaultClientIntegrationTests(IHttpHelper http)
    {
        _http = http;
    }

    // ========== HTTP 方法与回显 ==========

    [Fact]
    public async Task DefaultClient_GetAsync_WithQuery_ShouldEchoArgs()
    {
        var result = await _http.GetAsync<EchoResponse>("get", new { foo = "bar", num = 1 });

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Data.Should().NotBeNull();
        result.Data!.Args.Should().ContainKey("foo").WhoseValue.Should().Be("bar");
        result.Data.Args.Should().ContainKey("num").WhoseValue.Should().Be("1");
    }

    [Fact]
    public async Task DefaultClient_GetAsync_AsString_ShouldReturnEchoBody()
    {
        var result = await _http.GetAsync("get", new { mark = "azrng-default" });

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Data.Should().NotBeNullOrEmpty();
        result.Data.Should().Contain("azrng-default");
    }

    [Fact]
    public async Task DefaultClient_PostAsync_WithJsonBody_ShouldEchoJson()
    {
        var result = await _http.PostAsync<EchoResponse>("post", new { name = "azrng", age = 18 });

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Json.Should().NotBeNull();
        result.Data.Json!.Value.GetProperty("name").GetString().Should().Be("azrng");
        result.Data.Json.Value.GetProperty("age").GetInt32().Should().Be(18);
    }

    [Fact]
    public async Task DefaultClient_PutAsync_WithBodyAndQuery_ShouldEcho()
    {
        var result = await _http.PutAsync<EchoResponse>("put", new { k = "v" }, new { id = 99 });

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Args.Should().ContainKey("id").WhoseValue.Should().Be("99");
        result.Data.Json.Should().NotBeNull();
        result.Data.Json!.Value.GetProperty("k").GetString().Should().Be("v");
    }

    [Fact]
    public async Task DefaultClient_DeleteAsync_WithQuery_ShouldEchoArgs()
    {
        var result = await _http.DeleteAsync<EchoResponse>("delete", new { x = "y" });

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Args.Should().ContainKey("x").WhoseValue.Should().Be("y");
    }

    // ========== 表单 ==========

    [Fact]
    public async Task DefaultClient_PostFormDataAsync_KeyValue_ShouldEchoForm()
    {
        var form = new List<KeyValuePair<string, string>>
        {
            new("field1", "value1"),
            new("field2", "value2")
        };

        var result = await _http.PostFormDataAsync<EchoResponse>("post", form);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Form.Should().ContainKey("field1").WhoseValue.Should().Be("value1");
        result.Data.Form.Should().ContainKey("field2").WhoseValue.Should().Be("value2");
    }

    // ========== 自定义请求头 ==========

    [Fact]
    public async Task DefaultClient_GetAsync_WithCustomHeader_ShouldEchoHeader()
    {
        var headers = new Dictionary<string, string> { { "X-Default-Header", "hello-default" } };

        var result = await _http.GetAsync<EchoResponse>("get", null, headers);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Headers.Should().NotBeEmpty();
        result.Data.Headers!.Should().Contain(kvp =>
            string.Equals(kvp.Key, "X-Default-Header", StringComparison.OrdinalIgnoreCase)
            && kvp.Value == "hello-default");
    }

    // ========== SendAsync（枚举 / 原始）==========

    [Fact]
    public async Task DefaultClient_SendAsync_WithEnum_ShouldEcho()
    {
        using var content = new StringContent("{\"a\":1}", Encoding.UTF8, "application/json");

        var result = await _http.SendAsync(HttpRequestEnum.Post, "post", content);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNullOrEmpty();
        result.Data.Should().Contain("\"a\"");
    }

    [Fact]
    public async Task DefaultClient_SendAsync_WithRawRequestMessage_ShouldReturnRawResponse()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "get");

        var response = await _http.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("headers");
    }

    // ========== 流式读取 ==========

    [Fact]
    public async Task DefaultClient_GetStreamAsync_ShouldReadStream()
    {
        const int bytes = 100;

        var result = await _http.GetStreamAsync($"range/{bytes}");

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Data.Should().NotBeNull();

        await using var stream = result.Data!;
        using var buffer = new MemoryStream();
        await stream.CopyToAsync(buffer);
        buffer.Length.Should().Be(bytes);
    }

    // ========== 文件下载 ==========

    [Fact]
    public async Task DefaultClient_DownloadFileAsync_ShouldSavePngToFile()
    {
        var filePath = Path.Combine(Path.GetTempPath(), $"echo-default-{Guid.NewGuid():N}.png");
        try
        {
            var result = await _http.DownloadFileAsync("image/png", filePath);

            result.IsSuccess.Should().BeTrue();
            result.StatusCode.Should().Be(HttpStatusCode.OK);
            result.Data.Should().NotBeNull();
            result.Data!.FilePath.Should().Be(filePath);
            result.Data.FileSize.Should().BeGreaterThan(0);

            File.Exists(filePath).Should().BeTrue();
            var bytes = await File.ReadAllBytesAsync(filePath);
            bytes.Should().HaveCount((int)result.Data.FileSize);
            // PNG 文件以 89 50 4E 47（‰PNG）魔数开头
            bytes[0].Should().Be(0x89);
            bytes[1].Should().Be(0x50);
            bytes[2].Should().Be(0x4E);
            bytes[3].Should().Be(0x47);
        }
        finally
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }

    // ========== 错误处理（FailThrowException = false，返回结构化失败结果）==========

    [Fact]
    public async Task DefaultClient_StatusNotFound_WhenFailThrowDisabled_ShouldReturnFailResult()
    {
        var result = await _http.GetAsync("status/404");

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        result.Data.Should().BeNull();
    }

    // ========== 结构化结果（IHttpResult 元信息）==========

    [Fact]
    public async Task DefaultClient_GetAsync_ShouldContainRawBodyAndStatusCode()
    {
        var result = await _http.GetAsync("get");

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        // RawBody 应为非空 JSON 文本，包含 echo 的 url 字段
        result.RawBody.Should().NotBeNullOrEmpty();
        result.RawBody.Should().Contain("\"url\"");
    }

    /// <summary>
    /// Apifox Echo 响应结构（args/data/files/form/json/headers/url 均为小写键）。
    /// 与 <see cref="ApifoxEchoIntegrationTests.EchoResponse"/> 保持一致。
    /// </summary>
    private sealed class EchoResponse
    {
        public Dictionary<string, string>? Args { get; set; }

        public string? Data { get; set; }

        public Dictionary<string, string>? Files { get; set; }

        public Dictionary<string, string>? Form { get; set; }

        public JsonElement? Json { get; set; }

        public Dictionary<string, string>? Headers { get; set; }

        public string? Url { get; set; }
    }
}

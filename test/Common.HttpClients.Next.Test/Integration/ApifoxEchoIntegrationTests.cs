using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Common.HttpClients;

namespace Common.HttpClients.Next.Test.Integration;

/// <summary>
/// IHttpHelper 针对 Apifox Echo（https://echo.apifox.com）的集成测试。
/// 覆盖 IHttpHelper 全部成员：5 种 HTTP 方法、Query/JSON/Form/文件上传/Soap、
/// 自定义 Header、GetStreamAsync 流式读取、SendAsync（原始请求）、DownloadFileAsync 下载、
/// 以及 /delay 触发的超时（由 Fallback 兜底为 503）。
/// 这些测试会发起真实网络请求，需在联网环境执行；
/// 离线环境可通过 <c>--filter Category!=Integration</c> 跳过。
/// </summary>
[Trait("Category", "Integration")]
public class ApifoxEchoIntegrationTests
{
    private readonly IHttpHelper _http;
    private readonly IHttpHelper _httpTimeout;

    public ApifoxEchoIntegrationTests(IHttpHelperFactory factory)
    {
        _http = factory.CreateClient("apifox");
        _httpTimeout = factory.CreateClient("apifox-timeout");
    }

    // ========== HTTP 方法与回显 ==========

    [Fact]
    public async Task GetAsync_WithQuery_ShouldEchoArgs()
    {
        var result = await _http.GetAsync<EchoResponse>("get",
            new HttpSendOptions { Query = new { foo = "bar", num = 1 } });

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Data.Should().NotBeNull();
        result.Data!.Args.Should().ContainKey("foo").WhoseValue.Should().Be("bar");
        result.Data.Args.Should().ContainKey("num").WhoseValue.Should().Be("1");
    }

    [Fact]
    public async Task GetAsync_AsString_ShouldReturnEchoBody()
    {
        var result = await _http.GetAsync<string>("get",
            new HttpSendOptions { Query = new { mark = "azrng" } });

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Data.Should().NotBeNullOrEmpty();
        result.Data.Should().Contain("azrng");
    }

    [Fact]
    public async Task PostAsync_WithJsonBody_ShouldEchoJson()
    {
        var result = await _http.PostAsync<EchoResponse>("post", new { name = "azrng", age = 18 });

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Json.Should().NotBeNull();
        result.Data.Json!.Value.GetProperty("name").GetString().Should().Be("azrng");
        result.Data.Json.Value.GetProperty("age").GetInt32().Should().Be(18);
    }

    [Fact]
    public async Task PostAsync_AsString_ShouldReturnEchoBody()
    {
        var result = await _http.PostAsync<string>("post", new { name = "azrng" });

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Data.Should().NotBeNullOrEmpty();
        result.Data.Should().Contain("azrng");
    }

    [Fact]
    public async Task PutAsync_WithBodyAndQuery_ShouldEcho()
    {
        var result = await _http.PutAsync<EchoResponse>("put", new { k = "v" },
            new HttpSendOptions { Query = new { id = 99 } });

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Args.Should().ContainKey("id").WhoseValue.Should().Be("99");
        result.Data.Json.Should().NotBeNull();
        result.Data.Json!.Value.GetProperty("k").GetString().Should().Be("v");
    }

    [Fact]
    public async Task PatchAsync_WithBody_ShouldEcho()
    {
        var result = await _http.PatchAsync<EchoResponse>("patch", new { patched = true });

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Json.Should().NotBeNull();
        result.Data.Json!.Value.GetProperty("patched").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_WithQuery_ShouldEchoArgs()
    {
        var result = await _http.DeleteAsync<EchoResponse>("delete",
            new HttpSendOptions { Query = new { x = "y" } });

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Args.Should().ContainKey("x").WhoseValue.Should().Be("y");
    }

    [Fact]
    public async Task DeleteAsync_AsString_ShouldReturnEchoBody()
    {
        var result = await _http.DeleteAsync<string>("delete",
            new HttpSendOptions { Query = new { x = "y" } });

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Data.Should().NotBeNullOrEmpty();
        result.Data.Should().Contain("\"x\"");
    }

    // ========== 表单 / 文件上传 / Soap ==========

    [Fact]
    public async Task PostFormDataAsync_KeyValue_ShouldEchoForm()
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

    [Fact]
    public async Task PostFormDataAsync_AsString_ShouldReturnEchoBody()
    {
        var form = new List<KeyValuePair<string, string>> { new("field1", "value1") };

        var result = await _http.PostFormDataAsync<string>("post", form);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNullOrEmpty();
        result.Data.Should().Contain("value1");
    }

    [Fact]
    public async Task PostFormDataAsync_Stream_ShouldEchoFile()
    {
        const string fileContent = "hello-upload-content";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(fileContent));

        var result = await _http.PostFormDataAsync<EchoResponse>("post", "myfile", stream, "note.txt");

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Files.Should().ContainKey("myfile").WhoseValue.Should().Be(fileContent);
    }

    [Fact]
    public async Task PostFormDataAsync_Multipart_ShouldEchoFieldsAndFile()
    {
        const string fileContent = "hello-upload-content";
        using var multipart = new MultipartFormDataContent();
        multipart.Add(new StringContent("value1"), "field1");
        multipart.Add(new ByteArrayContent(Encoding.UTF8.GetBytes(fileContent)), "myfile", "note.txt");

        var result = await _http.PostFormDataAsync<EchoResponse>("post", multipart);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Form.Should().ContainKey("field1").WhoseValue.Should().Be("value1");
        result.Data.Files.Should().ContainKey("myfile").WhoseValue.Should().Be(fileContent);
    }

    [Fact]
    public async Task PostSoapAsync_ShouldEchoXmlAndContentType()
    {
        const string xml = "<soap:Env>ping</soap:Env>";

        var result = await _http.PostSoapAsync<EchoResponse>("post", xml);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        // data 字段回显原始 XML
        result.Data!.Data.Should().Contain("ping");
        // Content-Type 应为 application/soap+xml
        result.Data.Headers.Should().NotBeEmpty();
        result.Data.Headers!.Should().Contain(kvp =>
            string.Equals(kvp.Key, "Content-Type", StringComparison.OrdinalIgnoreCase)
            && kvp.Value.Contains("application/soap+xml"));
    }

    // ========== 自定义请求头 ==========

    [Fact]
    public async Task GetAsync_WithCustomHeader_ShouldEchoHeader()
    {
        var headers = new HttpHeaders { ["X-Test-Header"] = "hello-echo" };

        var result = await _http.GetAsync<EchoResponse>("get", new HttpSendOptions { Headers = headers });

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Headers.Should().NotBeEmpty();
        // Echo 原样回显请求头；用大小写不敏感匹配避免协议层规范化差异
        result.Data.Headers!.Should().Contain(kvp =>
            string.Equals(kvp.Key, "X-Test-Header", StringComparison.OrdinalIgnoreCase)
            && kvp.Value == "hello-echo");
    }

    // ========== SendAsync（原始请求）==========

    [Fact]
    public async Task SendAsync_WithRawPostContent_ShouldEcho()
    {
        using var content = new StringContent("{\"a\":1}", Encoding.UTF8, "application/json");
        using var request = new HttpRequestMessage(HttpMethod.Post, "post") { Content = content };

        using var response = await _http.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("\"a\"");
    }

    [Fact]
    public async Task SendAsync_WithRawRequestMessage_ShouldReturnRawResponse()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "get");

        var response = await _http.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("headers");
    }

    // ========== 流式读取 ==========

    [Fact]
    public async Task GetStreamAsync_ShouldReadStream()
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

    // ========== 文件下载（DownloadFileAsync）==========

    [Fact]
    public async Task DownloadFileAsync_ShouldSavePngToFile()
    {
        var filePath = Path.Combine(Path.GetTempPath(), $"echo-{Guid.NewGuid():N}.png");
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

    // ========== 错误处理（IHttpResult.Fail）==========

    [Fact]
    public async Task StatusNotFound_ShouldReturnFailResult()
    {
        var result = await _http.GetAsync<string>("status/404");

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task StatusError_ShouldReturnFailResult()
    {
        // 4.0 起统一结果对象：失败状态码返回 IHttpResult(IsSuccess=false)，不再抛异常
        var result = await _http.GetAsync<string>("status/418");

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be((HttpStatusCode)418);
    }

    // ========== 超时（/delay 触发，由 Fallback 兜底为 503）==========

    [Fact]
    public async Task Timeout_ShouldReturnFallbackResult()
    {
        // /delay/10 远超 2s 超时；超时由 Fallback 兜底为 503
        var result = await _httpTimeout.GetAsync<string>("delay/10");

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        result.IsFallbackResponse.Should().BeTrue();
    }

    /// <summary>
    /// Apifox Echo 响应结构（args/data/files/form/json/headers/url 均为小写键）。
    /// 借助 IHttpHelper 内部 CamelCase 反序列化策略，PascalCase 属性可与小写 JSON 键匹配。
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

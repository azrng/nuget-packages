using System.Net;
using System.Text;
using System.Text.Json;
using Common.HttpClients;

namespace Common.HttpClients.Next.Test.Samples;

/// <summary>
/// IHttpHelper 使用示例（学习用），目标为回显服务 https://echo.apifox.com，请求会被原样返回，便于观察实际发出的内容。
/// 每个示例对应 IHttpHelper 的一个成员，方法顺序与接口声明一致；
/// 断言保持最简，重点看调用方式与 IHttpResult 的 IsSuccess / Data / StatusCode / ErrorMessage。
/// 示例会发起真实网络请求，离线环境可通过 <c>--filter Category!=Integration</c> 跳过。
/// </summary>
[Trait("Category", "Integration")]
public class ApifoxEchoSamples
{
    private const string Host = "https://echo.apifox.com";

    private readonly IHttpHelper _httpHelper;

    public ApifoxEchoSamples(IHttpHelper httpHelper)
    {
        _httpHelper = httpHelper;
    }

    /// <summary>
    /// GetStreamAsync 示例：获取响应流并读取为字符串，适合大响应或流式处理场景
    /// </summary>
    [Fact]
    public async Task GetStreamExample()
    {
        var result = await _httpHelper.GetStreamAsync(Host + "/get");

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();

        await using var stream = result.Data!;
        using var reader = new StreamReader(stream);
        var body = await reader.ReadToEndAsync();
        body.Should().Contain("headers");
    }

    /// <summary>
    /// PostStreamAsync 示例：POST JSON 并按流读取真实响应，适合大响应体场景
    /// </summary>
    [Fact]
    public async Task PostStreamExample()
    {
        var result = await _httpHelper.PostStreamAsync(Host + "/post", new { cursor = 2, size = 100 });

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();

        await using var stream = result.Data!;
        using var reader = new StreamReader(stream);
        var body = await reader.ReadToEndAsync();
        using var document = JsonDocument.Parse(body);

        var json = document.RootElement.GetProperty("json");
        json.GetProperty("cursor").GetInt32().Should().Be(2);
        json.GetProperty("size").GetInt32().Should().Be(100);
    }

    /// <summary>
    /// GetAsync 示例：通过 HttpSendOptions.Query 传查询参数（匿名对象自动拼接到 URL），并反序列化为强类型
    /// </summary>
    [Fact]
    public async Task GetExample()
    {
        var result = await _httpHelper.GetAsync<EchoResponse>(Host + "/get",
            new HttpSendOptions { Query = new { name = "azrng", page = 1 } });

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Args.Should().ContainKey("name").WhoseValue.Should().Be("azrng");
    }

    /// <summary>
    /// PostAsync 示例：匿名对象自动序列化为 JSON body，同时演示自定义请求头
    /// </summary>
    [Fact]
    public async Task PostJsonExample()
    {
        var result = await _httpHelper.PostAsync<string>(Host + "/post", new { name = "azrng", age = 18 },
            new HttpSendOptions { Headers = new HttpHeaders { ["X-Demo-Header"] = "hello-echo" } });

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Contain("azrng");
    }

    /// <summary>
    /// PostFormDataAsync 示例（文本字段）：postman 的 body => form-data
    /// </summary>
    [Fact]
    public async Task PostFormExample()
    {
        var form = new List<KeyValuePair<string, string>>
        {
            new("field1", "value1"),
            new("field2", "value2")
        };

        var result = await _httpHelper.PostFormDataAsync<string>(Host + "/post", form);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Contain("value1");
    }

    /// <summary>
    /// PostFormUrlEncodedAsync 示例：发送 application/x-www-form-urlencoded 表单，适合 OAuth token 等标准表单端点
    /// </summary>
    [Fact]
    public async Task PostFormUrlEncodedExample()
    {
        var form = new List<KeyValuePair<string, string>>
        {
            new("grant_type", "client_credentials"),
            new("scope", "orders.read")
        };

        var result = await _httpHelper.PostFormUrlEncodedAsync<string>(Host + "/post", form);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Contain("client_credentials");
        result.Data.Should().Contain("orders.read");
    }

    /// <summary>
    /// PostFormDataAsync 示例（单一文件流）：参数名 file，文件名 note.txt，适合只有一个文件参数的上传场景
    /// </summary>
    [Fact]
    public async Task PostFileStreamExample()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("hello-upload-content"));

        var result = await _httpHelper.PostFormDataAsync<string>(Host + "/post", "file", stream, "note.txt");

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Contain("hello-upload-content");
    }

    /// <summary>
    /// PostFormDataAsync 示例（MultipartFormDataContent）：文本字段与文件混合上传，适合复杂表单
    /// </summary>
    [Fact]
    public async Task PostMultipartExample()
    {
        using var multipart = new MultipartFormDataContent();
        multipart.Add(new StringContent("value1"), "field1");
        multipart.Add(new ByteArrayContent(Encoding.UTF8.GetBytes("hello-upload-content")), "file", "note.txt");

        var result = await _httpHelper.PostFormDataAsync<string>(Host + "/post", multipart);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Contain("hello-upload-content");
    }

    /// <summary>
    /// PostSoapAsync 示例：发送 XML 报文，Content-Type 自动设置为 application/soap+xml
    /// </summary>
    [Fact]
    public async Task PostSoapExample()
    {
        const string xml = "<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">" +
                           "<soap:Body><Ping>hello</Ping></soap:Body></soap:Envelope>";

        var result = await _httpHelper.PostSoapAsync<string>(Host + "/post", xml);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Contain("hello");
    }

    /// <summary>
    /// PutAsync 示例：JSON body 更新资源
    /// </summary>
    [Fact]
    public async Task PutExample()
    {
        var result = await _httpHelper.PutAsync<string>(Host + "/put", new { id = 1, name = "azrng" });

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Contain("azrng");
    }

    /// <summary>
    /// DeleteAsync 示例：删除资源（T 为 string 时返回原始响应体）
    /// </summary>
    [Fact]
    public async Task DeleteExample()
    {
        var result = await _httpHelper.DeleteAsync<string>(Host + "/delete",
            new HttpSendOptions { Query = new { id = 1 } });

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Contain("\"id\"");
    }

    /// <summary>
    /// DeleteAsync 示例（携带请求体）：部分接口要求删除时附带 body（如批量删除、注明删除原因）
    /// </summary>
    [Fact]
    public async Task DeleteWithBodyExample()
    {
        var result = await _httpHelper.DeleteAsync<string>(Host + "/delete",
            new { ids = new[] { 1, 2 }, reason = "cleanup" });

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Contain("cleanup");
    }

    /// <summary>
    /// PatchAsync 示例：JSON body 局部更新
    /// </summary>
    [Fact]
    public async Task PatchExample()
    {
        var result = await _httpHelper.PatchAsync<string>(Host + "/patch", new { name = "new-name" });

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Contain("new-name");
    }

    /// <summary>
    /// SendAsync 示例：底层逃生舱口，直接发送原始 HttpRequestMessage，返回原始 HttpResponseMessage（调用方自行处理响应与状态码）
    /// </summary>
    [Fact]
    public async Task SendExample()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, Host + "/get");

        using var response = await _httpHelper.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// DownloadFileAsync 示例：下载图片到本地临时文件，返回下载信息（文件路径、大小）
    /// </summary>
    [Fact]
    public async Task DownloadExample()
    {
        var filePath = Path.Combine(Path.GetTempPath(), $"echo-sample-{Guid.NewGuid():N}.png");
        try
        {
            var result = await _httpHelper.DownloadFileAsync(Host + "/image/png", filePath);

            result.IsSuccess.Should().BeTrue();
            result.Data!.FileSize.Should().BeGreaterThan(0);
        }
        finally
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }

    /// <summary>
    /// 失败处理示例：404 等错误状态码不会抛异常，返回 IsSuccess=false 的 IHttpResult，通过 ErrorMessage / StatusCode 判断失败原因
    /// </summary>
    [Fact]
    public async Task GetNotFoundExample()
    {
        var result = await _httpHelper.GetAsync<string>(Host + "/status/404");

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Echo 服务回显结构（仅本示例使用，演示强类型反序列化；借助默认 CamelCase 命名策略匹配小写 JSON 键）
    /// </summary>
    private sealed class EchoResponse
    {
        public Dictionary<string, string>? Args { get; set; }

        public string? Url { get; set; }
    }
}

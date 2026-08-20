using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Common.HttpClients.Samples
{
    /// <summary>
    /// IHttpHelper 使用示例，目标为回显服务 https://echo.apifox.com（请求会被原样返回，便于观察实际发出的内容）。
    /// 使用前需注册客户端：services.AddHttpClientService()，再注入 IHttpHelper 构造本类；
    /// 每个示例方法对应 IHttpHelper 的一个成员，方法顺序与接口声明一致，返回 IHttpResult 便于观察 IsSuccess / Data / StatusCode / ErrorMessage。
    /// </summary>
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
        public async Task<string> GetStreamExample()
        {
            var result = await _httpHelper.GetStreamAsync(Host + "/get");
            if (!result.IsSuccess || result.Data is null)
            {
                return result.ErrorMessage ?? "请求失败";
            }

            await using var stream = result.Data;
            using var reader = new StreamReader(stream);
            return await reader.ReadToEndAsync();
        }

        /// <summary>
        /// GetAsync 示例：通过 HttpSendOptions.Query 传查询参数（匿名对象自动拼接到 URL），并反序列化为强类型
        /// </summary>
        public async Task<IHttpResult<EchoResponse>> GetExample()
        {
            return await _httpHelper.GetAsync<EchoResponse>(Host + "/get",
                new HttpSendOptions { Query = new { name = "azrng", page = 1 } });
        }

        /// <summary>
        /// PostAsync 示例：匿名对象自动序列化为 JSON body，同时演示自定义请求头
        /// </summary>
        public async Task<IHttpResult<string>> PostJsonExample()
        {
            return await _httpHelper.PostAsync<string>(Host + "/post", new { name = "azrng", age = 18 },
                new HttpSendOptions { Headers = new HttpHeaders { ["X-Demo-Header"] = "hello-echo" } });
        }

        /// <summary>
        /// PostFormDataAsync 示例（文本字段）：postman 的 body => form-data
        /// </summary>
        public async Task<IHttpResult<string>> PostFormExample()
        {
            var form = new List<KeyValuePair<string, string>>
            {
                new("field1", "value1"),
                new("field2", "value2")
            };

            return await _httpHelper.PostFormDataAsync<string>(Host + "/post", form);
        }

        /// <summary>
        /// PostFormDataAsync 示例（单一文件流）：参数名 file，文件名 note.txt，适合只有一个文件参数的上传场景
        /// </summary>
        public async Task<IHttpResult<string>> PostFileStreamExample()
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes("hello-upload-content"));

            return await _httpHelper.PostFormDataAsync<string>(Host + "/post", "file", stream, "note.txt");
        }

        /// <summary>
        /// PostFormDataAsync 示例（MultipartFormDataContent）：文本字段与文件混合上传，适合复杂表单
        /// </summary>
        public async Task<IHttpResult<string>> PostMultipartExample()
        {
            using var multipart = new MultipartFormDataContent();
            multipart.Add(new StringContent("value1"), "field1");
            multipart.Add(new ByteArrayContent(Encoding.UTF8.GetBytes("hello-upload-content")), "file", "note.txt");

            return await _httpHelper.PostFormDataAsync<string>(Host + "/post", multipart);
        }

        /// <summary>
        /// PostSoapAsync 示例：发送 XML 报文，Content-Type 自动设置为 application/soap+xml
        /// </summary>
        public async Task<IHttpResult<string>> PostSoapExample()
        {
            const string xml = "<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">" +
                               "<soap:Body><Ping>hello</Ping></soap:Body></soap:Envelope>";

            return await _httpHelper.PostSoapAsync<string>(Host + "/post", xml);
        }

        /// <summary>
        /// PutAsync 示例：JSON body 更新资源
        /// </summary>
        public async Task<IHttpResult<string>> PutExample()
        {
            return await _httpHelper.PutAsync<string>(Host + "/put", new { id = 1, name = "azrng" });
        }

        /// <summary>
        /// DeleteAsync 示例：删除资源（T 为 string 时返回原始响应体）
        /// </summary>
        public async Task<IHttpResult<string>> DeleteExample()
        {
            return await _httpHelper.DeleteAsync<string>(Host + "/delete",
                new HttpSendOptions { Query = new { id = 1 } });
        }

        /// <summary>
        /// PatchAsync 示例：JSON body 局部更新
        /// </summary>
        public async Task<IHttpResult<string>> PatchExample()
        {
            return await _httpHelper.PatchAsync<string>(Host + "/patch", new { name = "new-name" });
        }

        /// <summary>
        /// SendAsync 示例：底层逃生舱口，直接发送原始 HttpRequestMessage，返回原始 HttpResponseMessage（调用方自行处理响应与状态码）
        /// </summary>
        public async Task<HttpResponseMessage> SendExample()
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, Host + "/get");

            return await _httpHelper.SendAsync(request);
        }

        /// <summary>
        /// DownloadFileAsync 示例：下载图片到本地临时文件，返回下载信息（文件路径、大小）
        /// </summary>
        public async Task<IHttpResult<DownloadResult>> DownloadExample()
        {
            var filePath = Path.Combine(Path.GetTempPath(), $"echo-sample-{Guid.NewGuid():N}.png");

            return await _httpHelper.DownloadFileAsync(Host + "/image/png", filePath);
        }

        /// <summary>
        /// 失败处理示例：404 等错误状态码不会抛异常，返回 IsSuccess=false 的 IHttpResult，通过 ErrorMessage / StatusCode 判断失败原因
        /// </summary>
        public async Task<IHttpResult<string>> GetNotFoundExample()
        {
            return await _httpHelper.GetAsync<string>(Host + "/status/404");
        }

        /// <summary>
        /// Echo 服务回显结构（仅本示例使用，演示强类型反序列化；借助默认 CamelCase 命名策略匹配小写 JSON 键）
        /// </summary>
        public sealed class EchoResponse
        {
            public Dictionary<string, string>? Args { get; set; }

            public string? Url { get; set; }
        }
    }
}

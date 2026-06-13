#nullable enable
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Common.HttpClients
{
    /// <summary>
    /// http请求基类，所有方法返回 <see cref="IHttpResult{T}"/> 包装结果
    /// </summary>
    public interface IHttpHelper
    {
        /// <summary>
        /// Get请求获取文件流
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="queryParameters">查询参数（支持匿名对象、IDictionary&lt;string, string&gt;、NameValueCollection）</param>
        /// <param name="headers">请求头</param>
        /// <param name="cancellation"></param>
        /// <returns>包含流的HttpResult</returns>
        Task<IHttpResult<Stream>> GetStreamAsync(string url, object? queryParameters = null, IDictionary<string, string>? headers = null, CancellationToken cancellation = default);

        /// <summary>
        /// GET请求返回字符串
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="queryParameters">查询参数（支持匿名对象、IDictionary&lt;string, string&gt;、NameValueCollection）</param>
        /// <param name="headers">请求头</param>
        /// <param name="cancellation"></param>
        /// <returns>包含字符串的HttpResult</returns>
        Task<IHttpResult<string>> GetAsync(string url, object? queryParameters = null, IDictionary<string, string>? headers = null, CancellationToken cancellation = default);

        /// <summary>
        /// GET请求返回自定义内容
        /// </summary>
        /// <typeparam name="T">响应内容</typeparam>
        /// <param name="url">请求地址</param>
        /// <param name="queryParameters">查询参数（支持匿名对象、IDictionary&lt;string, string&gt;、NameValueCollection）</param>
        /// <param name="headers">请求头</param>
        /// <param name="cancellation"></param>
        /// <returns>包含反序列化对象的HttpResult</returns>
        Task<IHttpResult<T>> GetAsync<T>(string url, object? queryParameters = null, IDictionary<string, string>? headers = null, CancellationToken cancellation = default);

        /// <summary>
        /// Post请求返回字符串
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="data">请求的数据（字符串、对象）</param>
        /// <param name="queryParameters">查询参数（支持匿名对象、IDictionary&lt;string, string&gt;、NameValueCollection）</param>
        /// <param name="headers">请求头</param>
        /// <param name="cancellation"></param>
        /// <returns>包含字符串的HttpResult</returns>
        Task<IHttpResult<string>> PostAsync(string url, object data, object? queryParameters = null, IDictionary<string, string>? headers = null, CancellationToken cancellation = default);

        /// <summary>
        /// POST请求返回自定义内容
        /// </summary>
        /// <typeparam name="T">返回的结果</typeparam>
        /// <param name="url">请求地址</param>
        /// <param name="data">请求的数据（字符串、对象）</param>
        /// <param name="queryParameters">查询参数（支持匿名对象、IDictionary&lt;string, string&gt;、NameValueCollection）</param>
        /// <param name="headers">请求头</param>
        /// <param name="cancellation"></param>
        /// <returns>包含反序列化对象的HttpResult</returns>
        Task<IHttpResult<T>> PostAsync<T>(string url, object data, object? queryParameters = null, IDictionary<string, string>? headers = null, CancellationToken cancellation = default);

        /// <summary>
        /// post传递form-data文本参数
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="data">请求的数据</param>
        /// <param name="queryParameters">查询参数（支持匿名对象、IDictionary&lt;string, string&gt;、NameValueCollection）</param>
        /// <param name="headers">请求头</param>
        /// <param name="cancellation"></param>
        /// <returns>包含字符串的HttpResult</returns>
        /// <remarks>postman=>body=>form-data</remarks>
        Task<IHttpResult<string>> PostFormDataAsync(string url, IEnumerable<KeyValuePair<string, string>> data, object? queryParameters = null, IDictionary<string, string>? headers = null, CancellationToken cancellation = default);

        /// <summary>
        /// post传递form-data文本参数
        /// </summary>
        /// <typeparam name="T">返回的结果</typeparam>
        /// <param name="url">请求地址</param>
        /// <param name="data">请求的数据</param>
        /// <param name="queryParameters">查询参数（支持匿名对象、IDictionary&lt;string, string&gt;、NameValueCollection）</param>
        /// <param name="headers">请求头</param>
        /// <param name="cancellation"></param>
        /// <returns>包含反序列化对象的HttpResult</returns>
        Task<IHttpResult<T>> PostFormDataAsync<T>(string url, IEnumerable<KeyValuePair<string, string>> data, object? queryParameters = null, IDictionary<string, string>? headers = null, CancellationToken cancellation = default);

        /// <summary>
        /// post传递form-data文件(特定场景，参数只有一个文件)
        /// </summary>
        /// <typeparam name="T">返回的结果</typeparam>
        /// <param name="url">请求地址</param>
        /// <param name="parameter">参数名</param>
        /// <param name="stream">文件流</param>
        /// <param name="fileName">文件名</param>
        /// <param name="queryParameters">查询参数（支持匿名对象、IDictionary&lt;string, string&gt;、NameValueCollection）</param>
        /// <param name="headers">请求头</param>
        /// <param name="cancellation"></param>
        /// <returns>包含反序列化对象的HttpResult</returns>
        /// <remarks>postman=>body=>form-data</remarks>
        Task<IHttpResult<T>> PostFormDataAsync<T>(string url, string parameter, Stream stream, string fileName, object? queryParameters = null, IDictionary<string, string>? headers = null, CancellationToken cancellation = default);

        /// <summary>
        /// post传递form-data参数(支持上传文件)
        /// </summary>
        /// <typeparam name="T">返回的结果</typeparam>
        /// <param name="url">请求地址</param>
        /// <param name="data">请求的数据</param>
        /// <param name="queryParameters">查询参数（支持匿名对象、IDictionary&lt;string, string&gt;、NameValueCollection）</param>
        /// <param name="headers">请求头</param>
        /// <param name="cancellation"></param>
        /// <returns>包含反序列化对象的HttpResult</returns>
        /// <remarks>postman=>body=>form-data</remarks>
        Task<IHttpResult<T>> PostFormDataAsync<T>(string url, MultipartFormDataContent data, object? queryParameters = null, IDictionary<string, string>? headers = null, CancellationToken cancellation = default);

        /// <summary>
        /// post调用soap接口参数
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="xmlData">请求的数据</param>
        /// <param name="queryParameters">查询参数（支持匿名对象、IDictionary&lt;string, string&gt;、NameValueCollection）</param>
        /// <param name="headers">请求头</param>
        /// <param name="cancellation"></param>
        /// <returns>包含反序列化对象的HttpResult</returns>
        /// <remarks>postman=>body=>xml</remarks>
        Task<IHttpResult<T>> PostSoapAsync<T>(string url, string xmlData, object? queryParameters = null, IDictionary<string, string>? headers = null, CancellationToken cancellation = default);

        /// <summary>
        /// PUT请求返回自定义内容
        /// </summary>
        /// <typeparam name="T">返回的结果</typeparam>
        /// <param name="url">请求地址</param>
        /// <param name="data">json字符串</param>
        /// <param name="queryParameters">查询参数（支持匿名对象、IDictionary&lt;string, string&gt;、NameValueCollection）</param>
        /// <param name="headers">请求头</param>
        /// <param name="cancellation"></param>
        /// <returns>包含反序列化对象的HttpResult</returns>
        Task<IHttpResult<T>> PutAsync<T>(string url, object data, object? queryParameters = null, IDictionary<string, string>? headers = null, CancellationToken cancellation = default);

        /// <summary>
        /// Delete请求返回字符串
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="queryParameters">查询参数（支持匿名对象、IDictionary&lt;string, string&gt;、NameValueCollection）</param>
        /// <param name="headers">请求头</param>
        /// <param name="cancellation"></param>
        /// <returns>包含字符串的HttpResult</returns>
        Task<IHttpResult<string>> DeleteAsync(string url, object? queryParameters = null, IDictionary<string, string>? headers = null, CancellationToken cancellation = default);

        /// <summary>
        /// Delete请求返回自定义内容
        /// </summary>
        /// <typeparam name="T">响应内容</typeparam>
        /// <param name="url">请求地址</param>
        /// <param name="queryParameters">查询参数（支持匿名对象、IDictionary&lt;string, string&gt;、NameValueCollection）</param>
        /// <param name="headers">请求头</param>
        /// <param name="cancellation"></param>
        /// <returns>包含反序列化对象的HttpResult</returns>
        Task<IHttpResult<T>> DeleteAsync<T>(string url, object? queryParameters = null, IDictionary<string, string>? headers = null, CancellationToken cancellation = default);

        /// <summary>
        /// Patch请求返回自定义内容
        /// </summary>
        /// <typeparam name="T">响应内容</typeparam>
        /// <param name="url">请求地址</param>
        /// <param name="data">参数</param>
        /// <param name="queryParameters">查询参数（支持匿名对象、IDictionary&lt;string, string&gt;、NameValueCollection）</param>
        /// <param name="headers">请求头</param>
        /// <param name="cancellation"></param>
        /// <returns>包含反序列化对象的HttpResult</returns>
        Task<IHttpResult<T>> PatchAsync<T>(string url, object data, object? queryParameters = null, IDictionary<string, string>? headers = null, CancellationToken cancellation = default);

        /// <summary>
        /// send调用各种接口
        /// </summary>
        /// <param name="requestEnum">请求类型</param>
        /// <param name="url">请求地址</param>
        /// <param name="httpContent">HttpContent</param>
        /// <param name="queryParameters">查询参数（支持匿名对象、IDictionary&lt;string, string&gt;、NameValueCollection）</param>
        /// <param name="mediaTypeHeader">ContentType</param>
        /// <param name="cancellation"></param>
        /// <returns>包含字符串的HttpResult</returns>
        Task<IHttpResult<string>> SendAsync(HttpRequestEnum requestEnum, string url, HttpContent httpContent,
                                            object? queryParameters = null, MediaTypeHeaderValue? mediaTypeHeader = null, CancellationToken cancellation = default);

        /// <summary>
        /// send（底层逃生舱口，返回原始HttpResponseMessage）
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellation"></param>
        /// <returns>原始HttpResponseMessage</returns>
        Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellation = default);

        /// <summary>
        /// 下载文件到本地路径
        /// </summary>
        /// <param name="url">下载地址</param>
        /// <param name="filePath">保存的本地文件路径</param>
        /// <param name="queryParameters">查询参数（支持匿名对象、IDictionary&lt;string, string&gt;、NameValueCollection）</param>
        /// <param name="headers">请求头</param>
        /// <param name="cancellation"></param>
        /// <returns>包含下载信息的HttpResult</returns>
        Task<IHttpResult<DownloadResult>> DownloadFileAsync(string url, string filePath, object? queryParameters = null,
            IDictionary<string, string>? headers = null, CancellationToken cancellation = default);
    }
}

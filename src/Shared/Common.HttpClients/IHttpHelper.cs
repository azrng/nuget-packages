using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Common.HttpClients
{
    /// <summary>
    /// http请求基类
    /// </summary>
    public interface IHttpHelper
    {
        /// <summary>
        /// Get请求获取文件流
        /// </summary>
        /// <param name="url"></param>
        /// <param name="jwtToken"></param>
        /// <param name="headers"></param>
        /// <param name="timeout"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        Task<Stream> GetStreamAsync(string url, string jwtToken = "", IDictionary<string, string> headers = null, int? timeout = null,
                                    CancellationToken cancellation = default);

        /// <summary>
        /// GET请求返回字符串
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="jwtToken">token</param>
        /// <param name="headers">请求头</param>
        /// <param name="timeout"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        Task<string> GetAsync(string url, string jwtToken = "", IDictionary<string, string> headers = null, int? timeout = null,
                              CancellationToken cancellation = default);

        /// <summary>
        /// GET请求并传递token返回自定义内容
        /// </summary>
        /// <typeparam name="T">响应内容</typeparam>
        /// <param name="url">请求地址</param>
        /// <param name="jwtToken">token</param>
        /// <param name="headers">请求头</param>
        /// <param name="timeout"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        Task<T> GetAsync<T>(string url, string jwtToken = "", IDictionary<string, string> headers = null, int? timeout = null,
                            CancellationToken cancellation = default);

        /// <summary>
        /// Post请求返回字符串
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="data">请求的数据（字符串、对象）</param>
        /// <param name="jwtToken">token</param>
        /// <param name="headers">请求头</param>
        /// <param name="timeout"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        Task<string> PostAsync(string url, object data, string jwtToken = "",
                               IDictionary<string, string> headers = null, int? timeout = null, CancellationToken cancellation = default);

        /// <summary>
        /// POST请求返回自定义内容
        /// </summary>
        /// <typeparam name="T">返回的结果</typeparam>
        /// <param name="url">请求地址</param>
        /// <param name="data">请求的数据（字符串、对象）</param>
        /// <param name="jwtToken">token</param>
        /// <param name="headers">请求头</param>
        /// <param name="timeout"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        Task<T> PostAsync<T>(string url, object data, string jwtToken = "", IDictionary<string, string> headers = null,
                             int? timeout = null, CancellationToken cancellation = default);

        // /// <summary>
        // /// POST请求返回自定义内容
        // /// </summary>
        // /// <typeparam name="T">返回的结果</typeparam>
        // /// <param name="url">请求地址</param>
        // /// <param name="data">请求的数据（字符串、对象）</param>
        // /// <param name="jwtToken">token</param>
        // /// <param name="headers">请求头</param>
        // /// <returns></returns>
        // IAsyncEnumerable<string> PostGetStreamAsync<T>(string url, object data, string jwtToken = "",
        //                                                IDictionary<string, string> headers = null, CancellationToken cancellation = default);

        /// <summary>
        /// post传递form-data文本参数
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="data">请求的数据（字符串、对象）</param>
        /// <param name="jwtToken">token</param>
        /// <param name="headers">请求头</param>
        /// <param name="timeout"></param>
        /// <param name="cancellation"></param>
        /// <returns>字符串类型</returns>
        /// <remarks>postman=>body=>form-data</remarks>
        Task<string> PostFormDataAsync(string url, IEnumerable<KeyValuePair<string, string>> data, string jwtToken = "",
                                       IDictionary<string, string> headers = null, int? timeout = null,
                                       CancellationToken cancellation = default);

        /// <summary>
        /// post传递form-data文本参数
        /// </summary>
        /// <typeparam name="T">返回的结果</typeparam>
        /// <param name="url">请求地址</param>
        /// <param name="data">请求的数据</param>
        /// <param name="jwtToken">token</param>
        /// <param name="headers">请求头</param>
        /// <param name="timeout"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        Task<T> PostFormDataAsync<T>(string url, IEnumerable<KeyValuePair<string, string>> data, string jwtToken = "",
                                     IDictionary<string, string> headers = null, int? timeout = null,
                                     CancellationToken cancellation = default);

        /// <summary>
        /// post传递form-data文件(特定场景，参数只有一个文件)
        /// </summary>
        /// <typeparam name="T">返回的结果</typeparam>
        /// <param name="url">请求地址</param>
        /// <param name="parameter">参数名</param>
        /// <param name="stream">文件流</param>
        /// <param name="fileName">文件名</param>
        /// <param name="jwtToken"></param>
        /// <param name="headers">请求头</param>
        /// <param name="timeout"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        /// <remarks>postman=>body=>form-data</remarks>
        Task<T> PostFormDataAsync<T>(string url, string parameter, Stream stream, string fileName, string jwtToken = "",
                                     IDictionary<string, string> headers = null, int? timeout = null,
                                     CancellationToken cancellation = default);

        /// <summary>
        /// post传递form-data参数(支持上传文件)
        /// </summary>
        /// <typeparam name="T">返回的结果</typeparam>
        /// <param name="url">请求地址</param>
        /// <param name="data">请求的数据</param>
        /// <param name="jwtToken"></param>
        /// <param name="headers">请求头</param>
        /// <param name="timeout"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        /// <remarks>postman=>body=>form-data</remarks>
        Task<T> PostFormDataAsync<T>(string url, MultipartFormDataContent data, string jwtToken = "",
                                     IDictionary<string, string> headers = null, int? timeout = null,
                                     CancellationToken cancellation = default);

        /// <summary>
        /// post调用soap接口参数
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="xmlData">请求的数据</param>
        /// <param name="jwtToken">token</param>
        /// <param name="headers">请求头</param>
        /// <param name="timeout"></param>
        /// <param name="cancellation"></param>
        /// <returns>字符串类型</returns>
        /// <remarks>postman=>body=>xml</remarks>
        Task<T> PostSoapAsync<T>(string url, string xmlData, string jwtToken = "",
                                 IDictionary<string, string> headers = null, int? timeout = null, CancellationToken cancellation = default);

        /// <summary>
        /// POST请求返回自定义内容
        /// </summary>
        /// <typeparam name="T">返回的结果</typeparam>
        /// <param name="url">请求地址</param>
        /// <param name="data">json字符串</param>
        /// <param name="jwtToken">token</param>
        /// <param name="headers">请求头</param>
        /// <param name="timeout"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        Task<T> PutAsync<T>(string url, object data, string jwtToken = "", IDictionary<string, string> headers = null, int? timeout = null,
                            CancellationToken cancellation = default);

        /// <summary>
        /// Delete请求并传递token返回自定义内容
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="jwtToken">token</param>
        /// <param name="headers">请求头</param>
        /// <param name="timeout"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        Task<string> DeleteAsync(string url, string jwtToken = "", IDictionary<string, string> headers = null, int? timeout = null,
                                 CancellationToken cancellation = default);

        /// <summary>
        /// Delete请求并传递token返回自定义内容
        /// </summary>
        /// <typeparam name="T">响应内容</typeparam>
        /// <param name="url">请求地址</param>
        /// <param name="jwtToken">token</param>
        /// <param name="headers">请求头</param>
        /// <param name="timeout"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        Task<T> DeleteAsync<T>(string url, string jwtToken = "", IDictionary<string, string> headers = null, int? timeout = null,
                               CancellationToken cancellation = default);

        /// <summary>
        /// Patch请求并传递token返回自定义内容
        /// </summary>
        /// <typeparam name="T">响应内容</typeparam>
        /// <param name="url">请求地址</param>
        /// <param name="data">参数</param>
        /// <param name="jwtToken">token</param>
        /// <param name="headers">请求头</param>
        /// <param name="timeout"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        Task<T> PatchAsync<T>(string url, object data, string jwtToken = "",
                              IDictionary<string, string> headers = null, int? timeout = null, CancellationToken cancellation = default);

        /// <summary>
        /// send调用各种接口
        /// </summary>
        /// <param name="requestEnum">请求类型</param>
        /// <param name="url">请求地址</param>
        /// <param name="httpContent">HttpContent</param>
        /// <param name="mediaTypeHeader">ContentType</param>
        /// <param name="timeout"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        Task<string> SendAsync(HttpRequestEnum requestEnum, string url, HttpContent httpContent,
                               MediaTypeHeaderValue mediaTypeHeader = null, int? timeout = null, CancellationToken cancellation = default);

        /// <summary>
        /// send
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellation = default);
    }
}
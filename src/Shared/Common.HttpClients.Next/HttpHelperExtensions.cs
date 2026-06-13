#nullable enable
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Common.HttpClients
{
    /// <summary>
    /// <see cref="IHttpHelper"/> 的扩展方法，提供 Bearer Token 便利重载
    /// </summary>
    public static class HttpHelperExtensions
    {
        /// <summary>
        /// 使用 Bearer Token 发起 GET 请求（字符串）
        /// </summary>
        public static Task<IHttpResult<string>> GetAsync(this IHttpHelper helper, string url,
            string bearerToken, CancellationToken cancellation = default)
        {
            return helper.GetAsync(url, headers: BearerHeaders(bearerToken), cancellation: cancellation);
        }

        /// <summary>
        /// 使用 Bearer Token 发起 GET 请求（反序列化）
        /// </summary>
        public static Task<IHttpResult<T>> GetAsync<T>(this IHttpHelper helper, string url,
            string bearerToken, CancellationToken cancellation = default)
        {
            return helper.GetAsync<T>(url, headers: BearerHeaders(bearerToken), cancellation: cancellation);
        }

        /// <summary>
        /// 使用 Bearer Token 发起 GET 请求获取流
        /// </summary>
        public static Task<IHttpResult<Stream>> GetStreamAsync(this IHttpHelper helper, string url,
            string bearerToken, CancellationToken cancellation = default)
        {
            return helper.GetStreamAsync(url, headers: BearerHeaders(bearerToken), cancellation: cancellation);
        }

        /// <summary>
        /// 使用 Bearer Token 发起 POST 请求（字符串）
        /// </summary>
        public static Task<IHttpResult<string>> PostAsync(this IHttpHelper helper, string url, object data,
            string bearerToken, CancellationToken cancellation = default)
        {
            return helper.PostAsync(url, data, headers: BearerHeaders(bearerToken), cancellation: cancellation);
        }

        /// <summary>
        /// 使用 Bearer Token 发起 POST 请求（反序列化）
        /// </summary>
        public static Task<IHttpResult<T>> PostAsync<T>(this IHttpHelper helper, string url, object data,
            string bearerToken, CancellationToken cancellation = default)
        {
            return helper.PostAsync<T>(url, data, headers: BearerHeaders(bearerToken), cancellation: cancellation);
        }

        /// <summary>
        /// 使用 Bearer Token 发起 POST form-data 请求（字符串）
        /// </summary>
        public static Task<IHttpResult<string>> PostFormDataAsync(this IHttpHelper helper, string url,
            IEnumerable<KeyValuePair<string, string>> data, string bearerToken, CancellationToken cancellation = default)
        {
            return helper.PostFormDataAsync(url, data, headers: BearerHeaders(bearerToken), cancellation: cancellation);
        }

        /// <summary>
        /// 使用 Bearer Token 发起 POST form-data 请求（反序列化）
        /// </summary>
        public static Task<IHttpResult<T>> PostFormDataAsync<T>(this IHttpHelper helper, string url,
            IEnumerable<KeyValuePair<string, string>> data, string bearerToken, CancellationToken cancellation = default)
        {
            return helper.PostFormDataAsync<T>(url, data, headers: BearerHeaders(bearerToken), cancellation: cancellation);
        }

        /// <summary>
        /// 使用 Bearer Token 发起 POST form-data 文件上传请求（单文件）
        /// </summary>
        public static Task<IHttpResult<T>> PostFormDataAsync<T>(this IHttpHelper helper, string url,
            string parameter, Stream stream, string fileName, string bearerToken, CancellationToken cancellation = default)
        {
            return helper.PostFormDataAsync<T>(url, parameter, stream, fileName, headers: BearerHeaders(bearerToken), cancellation: cancellation);
        }

        /// <summary>
        /// 使用 Bearer Token 发起 POST form-data 请求（MultipartFormDataContent）
        /// </summary>
        public static Task<IHttpResult<T>> PostFormDataAsync<T>(this IHttpHelper helper, string url,
            MultipartFormDataContent data, string bearerToken, CancellationToken cancellation = default)
        {
            return helper.PostFormDataAsync<T>(url, data, headers: BearerHeaders(bearerToken), cancellation: cancellation);
        }

        /// <summary>
        /// 使用 Bearer Token 发起 POST SOAP 请求
        /// </summary>
        public static Task<IHttpResult<T>> PostSoapAsync<T>(this IHttpHelper helper, string url,
            string xmlData, string bearerToken, CancellationToken cancellation = default)
        {
            return helper.PostSoapAsync<T>(url, xmlData, headers: BearerHeaders(bearerToken), cancellation: cancellation);
        }

        /// <summary>
        /// 使用 Bearer Token 发起 PUT 请求
        /// </summary>
        public static Task<IHttpResult<T>> PutAsync<T>(this IHttpHelper helper, string url, object data,
            string bearerToken, CancellationToken cancellation = default)
        {
            return helper.PutAsync<T>(url, data, headers: BearerHeaders(bearerToken), cancellation: cancellation);
        }

        /// <summary>
        /// 使用 Bearer Token 发起 DELETE 请求（字符串）
        /// </summary>
        public static Task<IHttpResult<string>> DeleteAsync(this IHttpHelper helper, string url,
            string bearerToken, CancellationToken cancellation = default)
        {
            return helper.DeleteAsync(url, headers: BearerHeaders(bearerToken), cancellation: cancellation);
        }

        /// <summary>
        /// 使用 Bearer Token 发起 DELETE 请求（反序列化）
        /// </summary>
        public static Task<IHttpResult<T>> DeleteAsync<T>(this IHttpHelper helper, string url,
            string bearerToken, CancellationToken cancellation = default)
        {
            return helper.DeleteAsync<T>(url, headers: BearerHeaders(bearerToken), cancellation: cancellation);
        }

        /// <summary>
        /// 使用 Bearer Token 发起 PATCH 请求
        /// </summary>
        public static Task<IHttpResult<T>> PatchAsync<T>(this IHttpHelper helper, string url, object data,
            string bearerToken, CancellationToken cancellation = default)
        {
            return helper.PatchAsync<T>(url, data, headers: BearerHeaders(bearerToken), cancellation: cancellation);
        }

        /// <summary>
        /// 使用 Bearer Token 下载文件
        /// </summary>
        public static Task<IHttpResult<DownloadResult>> DownloadFileAsync(this IHttpHelper helper, string url,
            string filePath, string bearerToken, CancellationToken cancellation = default)
        {
            return helper.DownloadFileAsync(url, filePath, headers: BearerHeaders(bearerToken), cancellation: cancellation);
        }

        private static IDictionary<string, string> BearerHeaders(string token)
        {
            var t = token.StartsWith("Bearer ", System.StringComparison.OrdinalIgnoreCase) ? token : "Bearer " + token;
            return new Dictionary<string, string> { ["Authorization"] = t };
        }
    }
}

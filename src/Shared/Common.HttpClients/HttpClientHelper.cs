using Common.HttpClients.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Common.HttpClients
{
    /// <summary>
    /// 基于HttpClient的Http请求客户端
    /// </summary>
    public class HttpClientHelper : IHttpHelper
    {
        private readonly HttpClient _client;
        private readonly HttpClientOptions _httpConfig;
        private readonly ILogger<HttpClientHelper> _logger;

        public HttpClientHelper(IHttpClientFactory httpClientFactory, IOptions<HttpClientOptions> httpConfig,
                                ILogger<HttpClientHelper> logger)
        {
            _logger = logger;
            _client = httpClientFactory.CreateClient("default");
            _httpConfig = httpConfig.Value;
        }

        public async Task<Stream> GetStreamAsync(string url, string jwtToken = "", IDictionary<string, string> headers = null,
                                                 int? timeout = null, CancellationToken cancellation = default)
        {
            VerifyParam(url, jwtToken, headers);
            return await _client.GetStreamAsync(url, cancellation);
        }

        public async Task<string> GetAsync(string url, string bearerToken = "", IDictionary<string, string> headers = null,
                                           int? timeout = null, CancellationToken cancellation = default)
        {
            VerifyParam(url, bearerToken, headers);
            return await _client.GetStringAsync(url, cancellation).ConfigureAwait(false);
        }

        public async Task<T> GetAsync<T>(string url, string bearerToken = "", IDictionary<string, string> headers = null, int? timeout = null,
                                         CancellationToken cancellation = default)
        {
            VerifyParam(url, bearerToken, headers);

            var response = await _client.GetAsync(url, cancellation).ConfigureAwait(false);
            return await ConvertResponseResult<T>(response, url).ConfigureAwait(false);
        }

        public async Task<string> PostAsync(string url, object data, string bearerToken = "",
                                            IDictionary<string, string> headers = null, int? timeout = null,
                                            CancellationToken cancellation = default)
        {
            VerifyParam(url, bearerToken, headers);
            var jsonData = data is string ? data.ToString() : JsonHelper.ToJson(data);
            using var content = new StringContent(jsonData ?? string.Empty, Encoding.UTF8, "application/json");
            var response = await _client.PostAsync(url, content, cancellation).ConfigureAwait(false);
            return await ConvertResponseResult(response, url).ConfigureAwait(false);
        }

        public async Task<T> PostAsync<T>(string url, object data, string bearerToken = "",
                                          IDictionary<string, string> headers = null, int? timeout = null,
                                          CancellationToken cancellation = default)
        {
            VerifyParam(url, bearerToken, headers);
            var jsonData = data is string ? data.ToString() : JsonHelper.ToJson(data);
            using var content = new StringContent(jsonData ?? string.Empty, Encoding.UTF8, "application/json");
            var response = await _client.PostAsync(url, content, cancellation).ConfigureAwait(false);
            return await ConvertResponseResult<T>(response, url).ConfigureAwait(false);
        }

        // public async IAsyncEnumerable<string> PostGetStreamAsync<T>(string url, object data, string bearerToken = "",
        //                                                             IDictionary<string, string> headers = null)
        // {
        //     VerifyParam(url, bearerToken, headers);
        //     var jsonData = data is string ? data.ToString() : JsonHelper.ToJson(data);
        //     using var content = new StringContent(jsonData ?? string.Empty, Encoding.UTF8, "application/json");
        //     // 添加 HttpCompletionOption.ResponseHeadersRead 选项以支持流式响应
        //     using var httpRequest = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };
        //     var response = await _client.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
        //     if (_httpConfig.FailThrowException)
        //     {
        //         //确保成功完成，不是成功就返回具体错误信息
        //         response.EnsureSuccessStatusCode();
        //     }
        //     else if (!response.IsSuccessStatusCode)
        //     {
        //         var errorContent = await response.Content.ReadAsStringAsync();
        //         _logger.LogError($"API error: {response.StatusCode} - {errorContent}");
        //         yield break;
        //     }
        //
        //     await using var stream = await response.Content.ReadAsStreamAsync();
        //     using var reader = new StreamReader(stream);
        //
        //     while (!reader.EndOfStream)
        //     {
        //         var line = await reader.ReadLineAsync();
        //         if (!string.IsNullOrEmpty(line))
        //         {
        //             yield return line;
        //         }
        //     }
        // }

        public async Task<string> PostFormDataAsync(string url, IEnumerable<KeyValuePair<string, string>> data, string bearerToken = "",
                                                    IDictionary<string, string> headers = null, int? timeout = null,
                                                    CancellationToken cancellation = default)
        {
            VerifyParam(url, bearerToken, headers);
            using var httpContent = new FormUrlEncodedContent(data);
            var response = await _client.PostAsync(url, httpContent, cancellation).ConfigureAwait(false);
            return await ConvertResponseResult(response, url);
        }

        public async Task<T> PostFormDataAsync<T>(string url, IEnumerable<KeyValuePair<string, string>> data, string bearerToken = "",
                                                  IDictionary<string, string> headers = null, int? timeout = null,
                                                  CancellationToken cancellation = default)
        {
            VerifyParam(url, bearerToken, headers);
            var httpContent = new FormUrlEncodedContent(data);
            var response = await _client.PostAsync(url, httpContent, cancellation).ConfigureAwait(false);
            return await ConvertResponseResult<T>(response, url).ConfigureAwait(false);
        }

        public async Task<T> PostFormDataAsync<T>(string url, MultipartFormDataContent data, string bearerToken = "",
                                                  IDictionary<string, string> headers = null, int? timeout = null,
                                                  CancellationToken cancellation = default)
        {
            VerifyParam(url, bearerToken, headers);
            var result = await _client.PostAsync(url, data, cancellation).ConfigureAwait(false);
            return await ConvertResponseResult<T>(result, url);
        }

        public async Task<T> PostSoapAsync<T>(string url, string xmlData, string bearerToken = "",
                                              IDictionary<string, string> headers = null, int? timeout = null,
                                              CancellationToken cancellation = default)
        {
            VerifyParam(url, bearerToken, headers);
            _client.DefaultRequestHeaders.Add("Content-Type", "application/soap+xml");

            using var content = new StringContent(xmlData, Encoding.UTF8, "application/soap+xml");

            var response = await _client.PostAsync(url, content, cancellation).ConfigureAwait(false);
            return await ConvertResponseResult<T>(response, url).ConfigureAwait(false);
        }

        public async Task<T> PostFormDataAsync<T>(string url, string parameter, Stream stream, string fileName,
                                                  string bearerToken = "",
                                                  IDictionary<string, string> headers = null, int? timeout = null,
                                                  CancellationToken cancellation = default)
        {
            VerifyParam(url, bearerToken, headers);
            var formData = new MultipartFormDataContent();
            using var byteContent = new StreamContent(stream);
            byteContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                                                     {
                                                         Name = parameter, FileName = fileName
                                                     };
            formData.Add(byteContent);

            var response = await _client.PostAsync(url, formData, cancellation);
            return await ConvertResponseResult<T>(response, url).ConfigureAwait(false);
        }

        public async Task<T> PutAsync<T>(string url, object data, string bearerToken = "",
                                         IDictionary<string, string> headers = null, int? timeout = null,
                                         CancellationToken cancellation = default)
        {
            VerifyParam(url, bearerToken, headers);
            var jsonData = data is string ? data.ToString() : JsonHelper.ToJson(data);
            using var content = new StringContent(jsonData ?? string.Empty, Encoding.UTF8, "application/json");
            var response = await _client.PutAsync(url, content, cancellation).ConfigureAwait(false);
            return await ConvertResponseResult<T>(response, url).ConfigureAwait(false);
        }

        public async Task<string> DeleteAsync(string url, string bearerToken = "",
                                              IDictionary<string, string> headers = null, int? timeout = null,
                                              CancellationToken cancellation = default)
        {
            VerifyParam(url, bearerToken, headers);
            var response = await _client.DeleteAsync(url, cancellation).ConfigureAwait(false);
            return await ConvertResponseResult(response, url).ConfigureAwait(false);
        }

        public async Task<T> DeleteAsync<T>(string url, string bearerToken = "",
                                            IDictionary<string, string> headers = null, int? timeout = null,
                                            CancellationToken cancellation = default)
        {
            VerifyParam(url, bearerToken, headers);
            var response = await _client.DeleteAsync(url, cancellation).ConfigureAwait(false);
            return await ConvertResponseResult<T>(response, url).ConfigureAwait(false);
        }

        public async Task<T> PatchAsync<T>(string url, object data, string bearerToken = "",
                                           IDictionary<string, string> headers = null, int? timeout = null,
                                           CancellationToken cancellation = default)
        {
            VerifyParam(url, bearerToken, headers);
            var jsonData = data is string ? data.ToString() : JsonHelper.ToJson(data);
            using var content = new StringContent(jsonData ?? string.Empty, Encoding.UTF8, "application/json");
            var response = await _client.PatchAsync(url, content, cancellation).ConfigureAwait(false);
            return await ConvertResponseResult<T>(response, url).ConfigureAwait(false);
        }

        public async Task<string> SendAsync(HttpRequestEnum requestEnum, string url, HttpContent httpContent,
                                            MediaTypeHeaderValue mediaTypeHeader = null, int? timeout = null,
                                            CancellationToken cancellation = default)
        {
            var method = "GET";
            switch (requestEnum)
            {
                case HttpRequestEnum.Get:
                    method = "GET";
                    break;

                case HttpRequestEnum.Put:
                    method = "PUT";
                    break;

                case HttpRequestEnum.Post:
                    method = "POST";
                    break;

                case HttpRequestEnum.Delete:
                    method = "DELETE";
                    break;
            }

            var request = new HttpRequestMessage { Method = new HttpMethod(method), RequestUri = new Uri(url), Content = httpContent };
            request.Content.Headers.ContentType = mediaTypeHeader ?? new MediaTypeHeaderValue("application/json");

            var response = await _client.SendAsync(request, cancellation).ConfigureAwait(false);
            return await ConvertResponseResult(response, url);
        }

        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellation = default)
        {
            return _client.SendAsync(request, cancellation);
        }

        #region 私有方法

        /// <summary>
        /// 转换返回的结果
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="response"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        private async Task<T> ConvertResponseResult<T>(HttpResponseMessage response, string url)
        {
            if (_httpConfig.FailThrowException)
            {
                //确保成功完成，不是成功就返回具体错误信息
                response.EnsureSuccessStatusCode();
            }
            else if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"API:{url} error: {(int)response.StatusCode} - {errorContent}");
                return default;
            }

            var resStr = await response.Content.ReadAsStringAsync();

            // 如果响应内容为空(例如从 Fallback 策略返回的空响应),返回 default
            if (string.IsNullOrEmpty(resStr))
            {
                return default;
            }

            if (typeof(T) == typeof(string))
                return (T)Convert.ChangeType(resStr, typeof(string));

            return JsonHelper.ToObject<T>(resStr);
        }

        /// <summary>
        /// 转换返回的结果
        /// </summary>
        /// <param name="response"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        private async Task<string> ConvertResponseResult(HttpResponseMessage response, string url)
        {
            if (_httpConfig.FailThrowException)
            {
                //确保成功完成，不是成功就返回具体错误信息
                response.EnsureSuccessStatusCode();
            }
            else if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"API:{url} error: {(int)response.StatusCode} - {errorContent}");
            }

            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// 参数校验
        /// </summary>
        /// <param name="url"></param>
        /// <param name="bearerToken"></param>
        /// <param name="headers"></param>
        /// <exception cref="ArgumentNullException"></exception>
        private void VerifyParam(string url, string bearerToken, IDictionary<string, string> headers)
        {
            _client.DefaultRequestHeaders.Clear();
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentNullException(nameof(url), "url不能为null");
            }

            if (!string.IsNullOrWhiteSpace(bearerToken))
            {
                _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {bearerToken}");
            }

            if (!(headers?.Count > 0))
            {
                return;
            }

            foreach (var (key, value) in headers)
            {
                _client.DefaultRequestHeaders.Add(key, value);
            }
        }
    }

    #endregion 私有方法
}
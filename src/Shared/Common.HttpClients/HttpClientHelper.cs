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

        public HttpClientHelper(HttpClient client, IOptions<HttpClientOptions> httpConfig,
                                ILogger<HttpClientHelper> logger)
        {
            _logger = logger;
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _httpConfig = httpConfig.Value;
        }

        public async Task<Stream> GetStreamAsync(string url, string jwtToken = "", IDictionary<string, string> headers = null,
                                                 int? timeout = null, CancellationToken cancellation = default)
        {
            using var request = CreateRequestMessage(HttpMethod.Get, url, jwtToken, headers);
            var response = await SendCoreAsync(request, cancellation, HttpCompletionOption.ResponseHeadersRead)
                .ConfigureAwait(false);

            try
            {
                if (_httpConfig.FailThrowException)
                {
                    response.EnsureSuccessStatusCode();
                }
                else if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync(cancellation).ConfigureAwait(false);
                    _logger.LogError("API:{Url} error: {StatusCode} - {ErrorContent}", url, (int)response.StatusCode, errorContent);
                    response.Dispose();
                    return Stream.Null;
                }

                var stream = await response.Content.ReadAsStreamAsync(cancellation).ConfigureAwait(false);
                return new ResponseStream(stream, response);
            }
            catch
            {
                response.Dispose();
                throw;
            }
        }

        public async Task<string> GetAsync(string url, string bearerToken = "", IDictionary<string, string> headers = null,
                                           int? timeout = null, CancellationToken cancellation = default)
        {
            using var request = CreateRequestMessage(HttpMethod.Get, url, bearerToken, headers);
            using var response = await SendCoreAsync(request, cancellation).ConfigureAwait(false);
            return await ConvertResponseResult(response, url).ConfigureAwait(false);
        }

        public async Task<T> GetAsync<T>(string url, string bearerToken = "", IDictionary<string, string> headers = null,
                                         int? timeout = null,
                                         CancellationToken cancellation = default)
        {
            using var request = CreateRequestMessage(HttpMethod.Get, url, bearerToken, headers);
            using var response = await SendCoreAsync(request, cancellation).ConfigureAwait(false);
            return await ConvertResponseResult<T>(response, url).ConfigureAwait(false);
        }

        public async Task<string> PostAsync(string url, object data, string bearerToken = "",
                                            IDictionary<string, string> headers = null, int? timeout = null,
                                            CancellationToken cancellation = default)
        {
            var jsonData = data is string ? data.ToString() : JsonHelper.ToJson(data);
            using var content = new StringContent(jsonData ?? string.Empty, Encoding.UTF8, "application/json");
            using var request = CreateRequestMessage(HttpMethod.Post, url, bearerToken, headers, content);
            using var response = await SendCoreAsync(request, cancellation).ConfigureAwait(false);
            return await ConvertResponseResult(response, url).ConfigureAwait(false);
        }

        public async Task<T> PostAsync<T>(string url, object data, string bearerToken = "",
                                          IDictionary<string, string> headers = null, int? timeout = null,
                                          CancellationToken cancellation = default)
        {
            var jsonData = data is string ? data.ToString() : JsonHelper.ToJson(data);
            using var content = new StringContent(jsonData ?? string.Empty, Encoding.UTF8, "application/json");
            using var request = CreateRequestMessage(HttpMethod.Post, url, bearerToken, headers, content);
            using var response = await SendCoreAsync(request, cancellation).ConfigureAwait(false);
            return await ConvertResponseResult<T>(response, url).ConfigureAwait(false);
        }

        public async Task<string> PostFormDataAsync(string url, IEnumerable<KeyValuePair<string, string>> data, string bearerToken = "",
                                                    IDictionary<string, string> headers = null, int? timeout = null,
                                                    CancellationToken cancellation = default)
        {
            using var httpContent = new FormUrlEncodedContent(data);
            using var request = CreateRequestMessage(HttpMethod.Post, url, bearerToken, headers, httpContent);
            using var response = await SendCoreAsync(request, cancellation).ConfigureAwait(false);
            return await ConvertResponseResult(response, url).ConfigureAwait(false);
        }

        public async Task<T> PostFormDataAsync<T>(string url, IEnumerable<KeyValuePair<string, string>> data, string bearerToken = "",
                                                  IDictionary<string, string> headers = null, int? timeout = null,
                                                  CancellationToken cancellation = default)
        {
            using var httpContent = new FormUrlEncodedContent(data);
            using var request = CreateRequestMessage(HttpMethod.Post, url, bearerToken, headers, httpContent);
            using var response = await SendCoreAsync(request, cancellation).ConfigureAwait(false);
            return await ConvertResponseResult<T>(response, url).ConfigureAwait(false);
        }

        public async Task<T> PostFormDataAsync<T>(string url, MultipartFormDataContent data, string bearerToken = "",
                                                  IDictionary<string, string> headers = null, int? timeout = null,
                                                  CancellationToken cancellation = default)
        {
            using var request = CreateRequestMessage(HttpMethod.Post, url, bearerToken, headers, data);
            using var response = await SendCoreAsync(request, cancellation).ConfigureAwait(false);
            return await ConvertResponseResult<T>(response, url).ConfigureAwait(false);
        }

        public async Task<T> PostSoapAsync<T>(string url, string xmlData, string bearerToken = "",
                                              IDictionary<string, string> headers = null, int? timeout = null,
                                              CancellationToken cancellation = default)
        {
            using var content = new StringContent(xmlData ?? string.Empty, Encoding.UTF8, "application/soap+xml");
            using var request = CreateRequestMessage(HttpMethod.Post, url, bearerToken, headers, content);
            using var response = await SendCoreAsync(request, cancellation).ConfigureAwait(false);
            return await ConvertResponseResult<T>(response, url).ConfigureAwait(false);
        }

        public async Task<T> PostFormDataAsync<T>(string url, string parameter, Stream stream, string fileName,
                                                  string bearerToken = "",
                                                  IDictionary<string, string> headers = null, int? timeout = null,
                                                  CancellationToken cancellation = default)
        {
            using var formData = new MultipartFormDataContent();
            using var byteContent = new StreamContent(stream);
            byteContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                                                     {
                                                         Name = parameter, FileName = fileName
                                                     };
            formData.Add(byteContent);

            using var request = CreateRequestMessage(HttpMethod.Post, url, bearerToken, headers, formData);
            using var response = await SendCoreAsync(request, cancellation).ConfigureAwait(false);
            return await ConvertResponseResult<T>(response, url).ConfigureAwait(false);
        }

        public async Task<T> PutAsync<T>(string url, object data, string bearerToken = "",
                                         IDictionary<string, string> headers = null, int? timeout = null,
                                         CancellationToken cancellation = default)
        {
            var jsonData = data is string ? data.ToString() : JsonHelper.ToJson(data);
            using var content = new StringContent(jsonData ?? string.Empty, Encoding.UTF8, "application/json");
            using var request = CreateRequestMessage(HttpMethod.Put, url, bearerToken, headers, content);
            using var response = await SendCoreAsync(request, cancellation).ConfigureAwait(false);
            return await ConvertResponseResult<T>(response, url).ConfigureAwait(false);
        }

        public async Task<string> DeleteAsync(string url, string bearerToken = "",
                                              IDictionary<string, string> headers = null, int? timeout = null,
                                              CancellationToken cancellation = default)
        {
            using var request = CreateRequestMessage(HttpMethod.Delete, url, bearerToken, headers);
            using var response = await SendCoreAsync(request, cancellation).ConfigureAwait(false);
            return await ConvertResponseResult(response, url).ConfigureAwait(false);
        }

        public async Task<T> DeleteAsync<T>(string url, string bearerToken = "",
                                            IDictionary<string, string> headers = null, int? timeout = null,
                                            CancellationToken cancellation = default)
        {
            using var request = CreateRequestMessage(HttpMethod.Delete, url, bearerToken, headers);
            using var response = await SendCoreAsync(request, cancellation).ConfigureAwait(false);
            return await ConvertResponseResult<T>(response, url).ConfigureAwait(false);
        }

        public async Task<T> PatchAsync<T>(string url, object data, string bearerToken = "",
                                           IDictionary<string, string> headers = null, int? timeout = null,
                                           CancellationToken cancellation = default)
        {
            var jsonData = data is string ? data.ToString() : JsonHelper.ToJson(data);
            using var content = new StringContent(jsonData ?? string.Empty, Encoding.UTF8, "application/json");
            using var request = CreateRequestMessage(HttpMethod.Patch, url, bearerToken, headers, content);
            using var response = await SendCoreAsync(request, cancellation).ConfigureAwait(false);
            return await ConvertResponseResult<T>(response, url).ConfigureAwait(false);
        }

        public async Task<string> SendAsync(HttpRequestEnum requestEnum, string url, HttpContent httpContent,
                                            MediaTypeHeaderValue mediaTypeHeader = null, int? timeout = null,
                                            CancellationToken cancellation = default)
        {
            var method = requestEnum switch
            {
                HttpRequestEnum.Get => HttpMethod.Get,
                HttpRequestEnum.Put => HttpMethod.Put,
                HttpRequestEnum.Post => HttpMethod.Post,
                HttpRequestEnum.Delete => HttpMethod.Delete,
                _ => throw new ArgumentOutOfRangeException(nameof(requestEnum), requestEnum, "不支持的请求类型")
            };

            using var request = CreateRequestMessage(method, url, string.Empty, null, httpContent);
            if (request.Content != null && mediaTypeHeader != null)
            {
                request.Content.Headers.ContentType = mediaTypeHeader;
            }
            else if (request.Content != null && request.Content.Headers.ContentType == null)
            {
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            }

            using var response = await SendCoreAsync(request, cancellation).ConfigureAwait(false);
            return await ConvertResponseResult(response, url).ConfigureAwait(false);
        }

        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellation = default)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return await _client.SendAsync(request, cancellation).ConfigureAwait(false);
        }

        private async Task<T> ConvertResponseResult<T>(HttpResponseMessage response, string url)
        {
            if (_httpConfig.FailThrowException)
            {
                response.EnsureSuccessStatusCode();
            }
            else if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                _logger.LogError("API:{Url} error: {StatusCode} - {ErrorContent}", url, (int)response.StatusCode, errorContent);
                return default;
            }

            var resStr = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (string.IsNullOrEmpty(resStr))
            {
                return default;
            }

            if (typeof(T) == typeof(string))
            {
                return (T)Convert.ChangeType(resStr, typeof(string));
            }

            return JsonHelper.ToObject<T>(resStr);
        }

        private async Task<string> ConvertResponseResult(HttpResponseMessage response, string url)
        {
            if (_httpConfig.FailThrowException)
            {
                response.EnsureSuccessStatusCode();
            }
            else if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                _logger.LogError("API:{Url} error: {StatusCode} - {ErrorContent}", url, (int)response.StatusCode, errorContent);
            }

            return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        }

        private HttpRequestMessage CreateRequestMessage(HttpMethod method, string url, string bearerToken,
                                                        IDictionary<string, string> headers, HttpContent content = null)
        {
            ValidateUrl(url);

            var request = new HttpRequestMessage(method, url) { Content = content };

            if (!string.IsNullOrWhiteSpace(bearerToken))
            {
                var bearerTokenStr = bearerToken.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                    ? bearerToken
                    : "Bearer " + bearerToken;
                request.Headers.TryAddWithoutValidation("Authorization", bearerTokenStr);
            }

            if (headers == null || headers.Count == 0)
            {
                return request;
            }

            foreach (var (key, value) in headers)
            {
                if (string.IsNullOrWhiteSpace(key))
                {
                    continue;
                }

                if (string.Equals(key, "Content-Type", StringComparison.OrdinalIgnoreCase))
                {
                    request.Content?.Headers.TryAddWithoutValidation(key, value);
                    continue;
                }

                if (!request.Headers.TryAddWithoutValidation(key, value))
                {
                    request.Content?.Headers.TryAddWithoutValidation(key, value);
                }
            }

            return request;
        }

        private Task<HttpResponseMessage> SendCoreAsync(HttpRequestMessage request, CancellationToken cancellation,
                                                        HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead)
        {
            return _client.SendAsync(request, completionOption, cancellation);
        }

        private static void ValidateUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentNullException(nameof(url), "api url不能为空");
            }
        }
    }
}
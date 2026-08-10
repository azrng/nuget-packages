using Common.HttpClients.Utils;
using Microsoft.Extensions.Logging;
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
    /// 基于HttpClient的Http请求客户端，返回 <see cref="IHttpResult{T}"/> 结构化结果
    /// </summary>
    public class HttpClientHelper : IHttpHelper
    {
        private readonly HttpClient _client;
        private readonly ILogger<HttpClientHelper> _logger;

        public HttpClientHelper(HttpClient client, ILogger<HttpClientHelper> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public async Task<IHttpResult<Stream>> GetStreamAsync(string url, HttpSendOptions? opt = null,
                                                              CancellationToken cancellation = default)
        {
            var fullUrl = QueryStringBuilder.AppendQuery(url, opt?.Query);
            using var request = CreateRequestMessage(HttpMethod.Get, fullUrl, opt?.Headers);
            request.Options.Set(HttpClientRequestOptionKeys.SkipResponseBodyAudit, true);
            var response = await SendCoreAsync(request, cancellation, HttpCompletionOption.ResponseHeadersRead)
                .ConfigureAwait(false);

            try
            {
                var isFallback = IsFallbackResponse(response);
                var statusCode = response.StatusCode;

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync(cancellation).ConfigureAwait(false);
                    _logger.LogError("API:{Url} error: {StatusCode} - {ErrorContent}", fullUrl, (int)statusCode, errorContent);
                    response.Dispose();
                    return HttpResult<Stream>.Fail(errorContent, statusCode, errorContent, isFallback);
                }

                var stream = await response.Content.ReadAsStreamAsync(cancellation).ConfigureAwait(false);
                return HttpResult<Stream>.Success(new ResponseStream(stream, response), statusCode, null);
            }
            catch
            {
                response.Dispose();
                throw;
            }
        }

        public async Task<IHttpResult<T>> GetAsync<T>(string url, HttpSendOptions? opt = null, CancellationToken cancellation = default)
        {
            var fullUrl = QueryStringBuilder.AppendQuery(url, opt?.Query);
            using var request = CreateRequestMessage(HttpMethod.Get, fullUrl, opt?.Headers);
            using var response = await SendCoreAsync(request, cancellation).ConfigureAwait(false);
            return await ConvertResponseResult<T>(response, fullUrl).ConfigureAwait(false);
        }

        public async Task<IHttpResult<T>> PostAsync<T>(string url, object data, HttpSendOptions? opt = null, CancellationToken cancellation = default)
        {
            var fullUrl = QueryStringBuilder.AppendQuery(url, opt?.Query);
            var jsonData = data is string ? data.ToString() : JsonHelper.ToJson(data);
            using var content = new StringContent(jsonData ?? string.Empty, Encoding.UTF8, "application/json");
            using var request = CreateRequestMessage(HttpMethod.Post, fullUrl, opt?.Headers, content);
            using var response = await SendCoreAsync(request, cancellation).ConfigureAwait(false);
            return await ConvertResponseResult<T>(response, fullUrl).ConfigureAwait(false);
        }

        public async Task<IHttpResult<T>> PostFormDataAsync<T>(string url, IEnumerable<KeyValuePair<string, string>> data,
                                                               HttpSendOptions? opt = null, CancellationToken cancellation = default)
        {
            var fullUrl = QueryStringBuilder.AppendQuery(url, opt?.Query);
            using var httpContent = new FormUrlEncodedContent(data);
            using var request = CreateRequestMessage(HttpMethod.Post, fullUrl, opt?.Headers, httpContent);
            using var response = await SendCoreAsync(request, cancellation).ConfigureAwait(false);
            return await ConvertResponseResult<T>(response, fullUrl).ConfigureAwait(false);
        }

        public async Task<IHttpResult<T>> PostFormDataAsync<T>(string url, MultipartFormDataContent data, HttpSendOptions? opt = null,
                                                               CancellationToken cancellation = default)
        {
            var fullUrl = QueryStringBuilder.AppendQuery(url, opt?.Query);
            using var request = CreateRequestMessage(HttpMethod.Post, fullUrl, opt?.Headers, data);
            using var response = await SendCoreAsync(request, cancellation).ConfigureAwait(false);
            return await ConvertResponseResult<T>(response, fullUrl).ConfigureAwait(false);
        }

        public async Task<IHttpResult<T>> PostSoapAsync<T>(string url, string xmlData, HttpSendOptions? opt = null,
                                                           CancellationToken cancellation = default)
        {
            var fullUrl = QueryStringBuilder.AppendQuery(url, opt?.Query);
            using var content = new StringContent(xmlData ?? string.Empty, Encoding.UTF8, "application/soap+xml");
            using var request = CreateRequestMessage(HttpMethod.Post, fullUrl, opt?.Headers, content);
            using var response = await SendCoreAsync(request, cancellation).ConfigureAwait(false);
            return await ConvertResponseResult<T>(response, fullUrl).ConfigureAwait(false);
        }

        public async Task<IHttpResult<T>> PostFormDataAsync<T>(string url, string parameter, Stream stream, string fileName,
                                                               HttpSendOptions? opt = null, CancellationToken cancellation = default)
        {
            var fullUrl = QueryStringBuilder.AppendQuery(url, opt?.Query);
            using var formData = new MultipartFormDataContent();
            using var byteContent = new StreamContent(stream);
            byteContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                                                     {
                                                         Name = parameter, FileName = fileName
                                                     };
            formData.Add(byteContent);

            using var request = CreateRequestMessage(HttpMethod.Post, fullUrl, opt?.Headers, formData);
            using var response = await SendCoreAsync(request, cancellation).ConfigureAwait(false);
            return await ConvertResponseResult<T>(response, fullUrl).ConfigureAwait(false);
        }

        public async Task<IHttpResult<T>> PutAsync<T>(string url, object data, HttpSendOptions? opt = null, CancellationToken cancellation = default)
        {
            var fullUrl = QueryStringBuilder.AppendQuery(url, opt?.Query);
            var jsonData = data is string ? data.ToString() : JsonHelper.ToJson(data);
            using var content = new StringContent(jsonData ?? string.Empty, Encoding.UTF8, "application/json");
            using var request = CreateRequestMessage(HttpMethod.Put, fullUrl, opt?.Headers, content);
            using var response = await SendCoreAsync(request, cancellation).ConfigureAwait(false);
            return await ConvertResponseResult<T>(response, fullUrl).ConfigureAwait(false);
        }

        public async Task<IHttpResult<T>> DeleteAsync<T>(string url, HttpSendOptions? opt = null, CancellationToken cancellation = default)
        {
            var fullUrl = QueryStringBuilder.AppendQuery(url, opt?.Query);
            using var request = CreateRequestMessage(HttpMethod.Delete, fullUrl, opt?.Headers);
            using var response = await SendCoreAsync(request, cancellation).ConfigureAwait(false);
            return await ConvertResponseResult<T>(response, fullUrl).ConfigureAwait(false);
        }

        public async Task<IHttpResult<T>> PatchAsync<T>(string url, object data, HttpSendOptions? opt = null, CancellationToken cancellation = default)
        {
            var fullUrl = QueryStringBuilder.AppendQuery(url, opt?.Query);
            var jsonData = data is string ? data.ToString() : JsonHelper.ToJson(data);
            using var content = new StringContent(jsonData ?? string.Empty, Encoding.UTF8, "application/json");
            using var request = CreateRequestMessage(HttpMethod.Patch, fullUrl, opt?.Headers, content);
            using var response = await SendCoreAsync(request, cancellation).ConfigureAwait(false);
            return await ConvertResponseResult<T>(response, fullUrl).ConfigureAwait(false);
        }

        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellation = default)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return await _client.SendAsync(request, cancellation).ConfigureAwait(false);
        }

        public async Task<IHttpResult<DownloadResult>> DownloadFileAsync(string url, string filePath, HttpSendOptions? opt = null,
                                                                         CancellationToken cancellation = default)
        {
            var fullUrl = QueryStringBuilder.AppendQuery(url, opt?.Query);
            using var request = CreateRequestMessage(HttpMethod.Get, fullUrl, opt?.Headers);
            request.Options.Set(HttpClientRequestOptionKeys.SkipResponseBodyAudit, true);
            var response = await SendCoreAsync(request, cancellation, HttpCompletionOption.ResponseHeadersRead)
                .ConfigureAwait(false);

            var isFallback = IsFallbackResponse(response);
            var statusCode = response.StatusCode;

            try
            {
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync(cancellation).ConfigureAwait(false);
                    _logger.LogError("API:{Url} error: {StatusCode} - {ErrorContent}", fullUrl, (int)statusCode, errorContent);
                    response.Dispose();
                    return HttpResult<DownloadResult>.Fail(errorContent, statusCode, errorContent, isFallback);
                }

                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                long fileSize;
                await using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 81920,
                                 true))
                {
                    await using var httpStream = await response.Content.ReadAsStreamAsync(cancellation).ConfigureAwait(false);
                    await httpStream.CopyToAsync(fileStream, cancellation).ConfigureAwait(false);
                    fileSize = fileStream.Length;
                }

                response.Dispose();

                return HttpResult<DownloadResult>.Success(new DownloadResult { FilePath = filePath, FileSize = fileSize }, statusCode,
                    null);
            }
            catch
            {
                response.Dispose();

                // 清理不完整的文件
                try
                {
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                }
                catch
                {
                    // 忽略清理失败
                }

                throw;
            }
        }

        private async Task<IHttpResult<T>> ConvertResponseResult<T>(HttpResponseMessage response, string url)
        {
            var isFallback = IsFallbackResponse(response);
            var statusCode = response.StatusCode;
            var rawBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("API:{Url} error: {StatusCode} - {ErrorContent}", url, (int)statusCode, rawBody);
                return HttpResult<T>.Fail(rawBody, statusCode, rawBody, isFallback);
            }

            // T 为 string 时原样返回响应体（含空串），保持字符串语义一致；
            // 仅反序列化对象时，空响应体才返回 default(T)
            if (typeof(T) == typeof(string))
            {
                return HttpResult<T>.Success((T)(object)rawBody, statusCode, rawBody);
            }

            if (string.IsNullOrEmpty(rawBody))
            {
                return HttpResult<T>.Success(default, statusCode, rawBody);
            }

            var data = JsonHelper.ToObject<T>(rawBody);
            return HttpResult<T>.Success(data, statusCode, rawBody);
        }

        private HttpRequestMessage CreateRequestMessage(HttpMethod method, string url,
                                                        IDictionary<string, string>? headers, HttpContent? content = null)
        {
            ValidateUrl(url);

            var request = new HttpRequestMessage(method, url) { Content = content };

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

        private static bool IsFallbackResponse(HttpResponseMessage response)
        {
            return response.Headers.Contains(HttpClientHeaderNames.FallbackResponse);
        }

        private static void ValidateUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentNullException(nameof(url), "api url cannot be empty");
            }
        }
    }
}

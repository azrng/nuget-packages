using Common.HttpClients.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
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
        private readonly HttpClientOptions _httpConfig;
        private readonly ILogger<HttpClientHelper> _logger;

        public HttpClientHelper(HttpClient client, IOptions<HttpClientOptions> httpConfig,
                                ILogger<HttpClientHelper> logger)
        {
            _logger = logger;
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _httpConfig = httpConfig.Value;
        }

        public async Task<IHttpResult<Stream>> GetStreamAsync(string url, object? queryParameters = null, IDictionary<string, string>? headers = null, CancellationToken cancellation = default)
        {
            var fullUrl = QueryStringBuilder.AppendQuery(url, queryParameters);
            using var request = CreateRequestMessage(HttpMethod.Get, fullUrl, headers);
            request.Options.Set(HttpClientRequestOptionKeys.SkipResponseBodyAudit, true);
            var response = await SendCoreAsync(request, cancellation, HttpCompletionOption.ResponseHeadersRead)
                .ConfigureAwait(false);

            try
            {
                var isFallback = IsFallbackResponse(response);
                var statusCode = response.StatusCode;

                if (_httpConfig.FailThrowException)
                {
                    response.EnsureSuccessStatusCode();
                }
                else if (!response.IsSuccessStatusCode)
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

        public async Task<IHttpResult<string>> GetAsync(string url, object? queryParameters = null, IDictionary<string, string>? headers = null, CancellationToken cancellation = default)
        {
            var fullUrl = QueryStringBuilder.AppendQuery(url, queryParameters);
            using var request = CreateRequestMessage(HttpMethod.Get, fullUrl, headers);
            using var response = await SendCoreAsync(request, cancellation).ConfigureAwait(false);
            return await ConvertResponseResult(response, fullUrl).ConfigureAwait(false);
        }

        public async Task<IHttpResult<T>> GetAsync<T>(string url, object? queryParameters = null, IDictionary<string, string>? headers = null, CancellationToken cancellation = default)
        {
            var fullUrl = QueryStringBuilder.AppendQuery(url, queryParameters);
            using var request = CreateRequestMessage(HttpMethod.Get, fullUrl, headers);
            using var response = await SendCoreAsync(request, cancellation).ConfigureAwait(false);
            return await ConvertResponseResult<T>(response, fullUrl).ConfigureAwait(false);
        }

        public async Task<IHttpResult<string>> PostAsync(string url, object data, object? queryParameters = null, IDictionary<string, string>? headers = null, CancellationToken cancellation = default)
        {
            var fullUrl = QueryStringBuilder.AppendQuery(url, queryParameters);
            var jsonData = data is string ? data.ToString() : JsonHelper.ToJson(data);
            using var content = new StringContent(jsonData ?? string.Empty, Encoding.UTF8, "application/json");
            using var request = CreateRequestMessage(HttpMethod.Post, fullUrl, headers, content);
            using var response = await SendCoreAsync(request, cancellation).ConfigureAwait(false);
            return await ConvertResponseResult(response, fullUrl).ConfigureAwait(false);
        }

        public async Task<IHttpResult<T>> PostAsync<T>(string url, object data, object? queryParameters = null, IDictionary<string, string>? headers = null, CancellationToken cancellation = default)
        {
            var fullUrl = QueryStringBuilder.AppendQuery(url, queryParameters);
            var jsonData = data is string ? data.ToString() : JsonHelper.ToJson(data);
            using var content = new StringContent(jsonData ?? string.Empty, Encoding.UTF8, "application/json");
            using var request = CreateRequestMessage(HttpMethod.Post, fullUrl, headers, content);
            using var response = await SendCoreAsync(request, cancellation).ConfigureAwait(false);
            return await ConvertResponseResult<T>(response, fullUrl).ConfigureAwait(false);
        }

        public async Task<IHttpResult<string>> PostFormDataAsync(string url, IEnumerable<KeyValuePair<string, string>> data, object? queryParameters = null, IDictionary<string, string>? headers = null, CancellationToken cancellation = default)
        {
            var fullUrl = QueryStringBuilder.AppendQuery(url, queryParameters);
            using var httpContent = new FormUrlEncodedContent(data);
            using var request = CreateRequestMessage(HttpMethod.Post, fullUrl, headers, httpContent);
            using var response = await SendCoreAsync(request, cancellation).ConfigureAwait(false);
            return await ConvertResponseResult(response, fullUrl).ConfigureAwait(false);
        }

        public async Task<IHttpResult<T>> PostFormDataAsync<T>(string url, IEnumerable<KeyValuePair<string, string>> data, object? queryParameters = null, IDictionary<string, string>? headers = null, CancellationToken cancellation = default)
        {
            var fullUrl = QueryStringBuilder.AppendQuery(url, queryParameters);
            using var httpContent = new FormUrlEncodedContent(data);
            using var request = CreateRequestMessage(HttpMethod.Post, fullUrl, headers, httpContent);
            using var response = await SendCoreAsync(request, cancellation).ConfigureAwait(false);
            return await ConvertResponseResult<T>(response, fullUrl).ConfigureAwait(false);
        }

        public async Task<IHttpResult<T>> PostFormDataAsync<T>(string url, MultipartFormDataContent data, object? queryParameters = null, IDictionary<string, string>? headers = null, CancellationToken cancellation = default)
        {
            var fullUrl = QueryStringBuilder.AppendQuery(url, queryParameters);
            using var request = CreateRequestMessage(HttpMethod.Post, fullUrl, headers, data);
            using var response = await SendCoreAsync(request, cancellation).ConfigureAwait(false);
            return await ConvertResponseResult<T>(response, fullUrl).ConfigureAwait(false);
        }

        public async Task<IHttpResult<T>> PostSoapAsync<T>(string url, string xmlData, object? queryParameters = null, IDictionary<string, string>? headers = null, CancellationToken cancellation = default)
        {
            var fullUrl = QueryStringBuilder.AppendQuery(url, queryParameters);
            using var content = new StringContent(xmlData ?? string.Empty, Encoding.UTF8, "application/soap+xml");
            using var request = CreateRequestMessage(HttpMethod.Post, fullUrl, headers, content);
            using var response = await SendCoreAsync(request, cancellation).ConfigureAwait(false);
            return await ConvertResponseResult<T>(response, fullUrl).ConfigureAwait(false);
        }

        public async Task<IHttpResult<T>> PostFormDataAsync<T>(string url, string parameter, Stream stream, string fileName, object? queryParameters = null,
                                                                IDictionary<string, string>? headers = null, CancellationToken cancellation = default)
        {
            var fullUrl = QueryStringBuilder.AppendQuery(url, queryParameters);
            using var formData = new MultipartFormDataContent();
            using var byteContent = new StreamContent(stream);
            byteContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                                                     {
                                                         Name = parameter, FileName = fileName
                                                     };
            formData.Add(byteContent);

            using var request = CreateRequestMessage(HttpMethod.Post, fullUrl, headers, formData);
            using var response = await SendCoreAsync(request, cancellation).ConfigureAwait(false);
            return await ConvertResponseResult<T>(response, fullUrl).ConfigureAwait(false);
        }

        public async Task<IHttpResult<T>> PutAsync<T>(string url, object data, object? queryParameters = null, IDictionary<string, string>? headers = null, CancellationToken cancellation = default)
        {
            var fullUrl = QueryStringBuilder.AppendQuery(url, queryParameters);
            var jsonData = data is string ? data.ToString() : JsonHelper.ToJson(data);
            using var content = new StringContent(jsonData ?? string.Empty, Encoding.UTF8, "application/json");
            using var request = CreateRequestMessage(HttpMethod.Put, fullUrl, headers, content);
            using var response = await SendCoreAsync(request, cancellation).ConfigureAwait(false);
            return await ConvertResponseResult<T>(response, fullUrl).ConfigureAwait(false);
        }

        public async Task<IHttpResult<string>> DeleteAsync(string url, object? queryParameters = null, IDictionary<string, string>? headers = null, CancellationToken cancellation = default)
        {
            var fullUrl = QueryStringBuilder.AppendQuery(url, queryParameters);
            using var request = CreateRequestMessage(HttpMethod.Delete, fullUrl, headers);
            using var response = await SendCoreAsync(request, cancellation).ConfigureAwait(false);
            return await ConvertResponseResult(response, fullUrl).ConfigureAwait(false);
        }

        public async Task<IHttpResult<T>> DeleteAsync<T>(string url, object? queryParameters = null, IDictionary<string, string>? headers = null, CancellationToken cancellation = default)
        {
            var fullUrl = QueryStringBuilder.AppendQuery(url, queryParameters);
            using var request = CreateRequestMessage(HttpMethod.Delete, fullUrl, headers);
            using var response = await SendCoreAsync(request, cancellation).ConfigureAwait(false);
            return await ConvertResponseResult<T>(response, fullUrl).ConfigureAwait(false);
        }

        public async Task<IHttpResult<T>> PatchAsync<T>(string url, object data, object? queryParameters = null, IDictionary<string, string>? headers = null, CancellationToken cancellation = default)
        {
            var fullUrl = QueryStringBuilder.AppendQuery(url, queryParameters);
            var jsonData = data is string ? data.ToString() : JsonHelper.ToJson(data);
            using var content = new StringContent(jsonData ?? string.Empty, Encoding.UTF8, "application/json");
            using var request = CreateRequestMessage(HttpMethod.Patch, fullUrl, headers, content);
            using var response = await SendCoreAsync(request, cancellation).ConfigureAwait(false);
            return await ConvertResponseResult<T>(response, fullUrl).ConfigureAwait(false);
        }

        public async Task<IHttpResult<string>> SendAsync(HttpRequestEnum requestEnum, string url, HttpContent httpContent,
                                                          object? queryParameters = null, MediaTypeHeaderValue? mediaTypeHeader = null, CancellationToken cancellation = default)
        {
            var fullUrl = QueryStringBuilder.AppendQuery(url, queryParameters);
            var method = requestEnum switch
            {
                HttpRequestEnum.Get => HttpMethod.Get,
                HttpRequestEnum.Put => HttpMethod.Put,
                HttpRequestEnum.Post => HttpMethod.Post,
                HttpRequestEnum.Delete => HttpMethod.Delete,
                _ => throw new ArgumentOutOfRangeException(nameof(requestEnum), requestEnum, "不支持的请求类型")
            };

            using var request = CreateRequestMessage(method, fullUrl, null, httpContent);
            if (request.Content != null && mediaTypeHeader != null)
            {
                request.Content.Headers.ContentType = mediaTypeHeader;
            }
            else if (request.Content != null && request.Content.Headers.ContentType == null)
            {
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            }

            using var response = await SendCoreAsync(request, cancellation).ConfigureAwait(false);
            return await ConvertResponseResult(response, fullUrl).ConfigureAwait(false);
        }

        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellation = default)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return await _client.SendAsync(request, cancellation).ConfigureAwait(false);
        }

        public async Task<IHttpResult<DownloadResult>> DownloadFileAsync(string url, string filePath, object? queryParameters = null,
            IDictionary<string, string>? headers = null, CancellationToken cancellation = default)
        {
            var fullUrl = QueryStringBuilder.AppendQuery(url, queryParameters);
            using var request = CreateRequestMessage(HttpMethod.Get, fullUrl, headers);
            request.Options.Set(HttpClientRequestOptionKeys.SkipResponseBodyAudit, true);
            var response = await SendCoreAsync(request, cancellation, HttpCompletionOption.ResponseHeadersRead)
                .ConfigureAwait(false);

            var isFallback = IsFallbackResponse(response);
            var statusCode = response.StatusCode;

            try
            {
                if (_httpConfig.FailThrowException)
                {
                    response.EnsureSuccessStatusCode();
                }
                else if (!response.IsSuccessStatusCode)
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
                await using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, true))
                {
                    await using var httpStream = await response.Content.ReadAsStreamAsync(cancellation).ConfigureAwait(false);
                    await httpStream.CopyToAsync(fileStream, cancellation).ConfigureAwait(false);
                    fileSize = fileStream.Length;
                }

                response.Dispose();

                return HttpResult<DownloadResult>.Success(new DownloadResult
                {
                    FilePath = filePath,
                    FileSize = fileSize
                }, statusCode, null);
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

            if (_httpConfig.FailThrowException)
            {
                response.EnsureSuccessStatusCode();
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("API:{Url} error: {StatusCode} - {ErrorContent}", url, (int)statusCode, rawBody);
                return HttpResult<T>.Fail(rawBody, statusCode, rawBody, isFallback);
            }

            if (string.IsNullOrEmpty(rawBody))
            {
                return HttpResult<T>.Success(default, statusCode, rawBody);
            }

            if (typeof(T) == typeof(string))
            {
                return HttpResult<T>.Success((T)(object)rawBody, statusCode, rawBody);
            }

            var data = JsonHelper.ToObject<T>(rawBody);
            return HttpResult<T>.Success(data, statusCode, rawBody);
        }

        private async Task<IHttpResult<string>> ConvertResponseResult(HttpResponseMessage response, string url)
        {
            var isFallback = IsFallbackResponse(response);
            var statusCode = response.StatusCode;
            var rawBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (_httpConfig.FailThrowException)
            {
                response.EnsureSuccessStatusCode();
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("API:{Url} error: {StatusCode} - {ErrorContent}", url, (int)statusCode, rawBody);
                return HttpResult<string>.Fail(rawBody, statusCode, rawBody, isFallback);
            }

            return HttpResult<string>.Success(rawBody, statusCode, rawBody);
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

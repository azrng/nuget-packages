using Common.HttpClients.Next.Test.Helpers;
using Microsoft.Extensions.Options;

namespace Common.HttpClients.Next.Test
{
    /// <summary>
    /// HttpClientHelper 各 HTTP 方法 + IHttpResult&lt;T&gt; 返回值测试
    /// </summary>
    public class HttpClientHelperTests
    {
        [Fact]
        public async Task GetAsync_NonGeneric_ShouldReturnSuccessWithString()
        {
            using var client = NewClient(_ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("ok")
            });
            var helper = CreateHelper(client);

            var result = await helper.GetAsync("https://unit.test/health");

            Assert.True(result.IsSuccess);
            Assert.Equal("ok", result.Data);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.False(result.IsFallbackResponse);
        }

        [Fact]
        public async Task GetAsync_Generic_ShouldDeserializeJson()
        {
            using var client = NewClient(_ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"id\":7,\"name\":\"az\"}")
            });
            var helper = CreateHelper(client);

            var result = await helper.GetAsync<SampleResponse>("https://unit.test/item");

            Assert.True(result.IsSuccess);
            Assert.Equal(7, result.Data?.Id);
            Assert.Equal("az", result.Data?.Name);
        }

        [Fact]
        public async Task GetAsync_Failure_WhenFailThrowDisabled_ShouldReturnFailedResult()
        {
            using var client = NewClient(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent("server-error")
            });
            var helper = CreateHelper(client, failThrowException: false);

            var result = await helper.GetAsync<SampleResponse>("https://unit.test/error");

            Assert.False(result.IsSuccess);
            Assert.Null(result.Data);
            Assert.Equal("server-error", result.ErrorMessage);
            Assert.Equal("server-error", result.RawBody);
            Assert.Equal(HttpStatusCode.InternalServerError, result.StatusCode);
        }

        [Fact]
        public async Task GetAsync_Failure_WhenFailThrowEnabled_ShouldThrowHttpRequestException()
        {
            using var client = NewClient(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent("server-error")
            });
            var helper = CreateHelper(client, failThrowException: true);

            await Assert.ThrowsAsync<HttpRequestException>(() => helper.GetAsync<SampleResponse>("https://unit.test/error"));
        }

        [Fact]
        public async Task GetAsync_WithQueryParameters_ShouldAppendQueryString()
        {
            string? capturedUrl = null;
            using var client = NewClient(r =>
            {
                capturedUrl = r.RequestUri?.ToString();
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("ok")
                };
            });
            var helper = CreateHelper(client);

            await helper.GetAsync("https://unit.test/api", queryParameters: new { page = 1, keyword = "az rng" });

            Assert.NotNull(capturedUrl);
            Assert.Contains("page=1", capturedUrl);
            Assert.Contains("keyword=az+rng", capturedUrl);
        }

        [Fact]
        public async Task GetAsync_WithHeaders_ShouldPassThrough()
        {
            string? authHeader = null;
            using var client = NewClient(r =>
            {
                authHeader = r.Headers.GetValues("Authorization").FirstOrDefault();
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("ok")
                };
            });
            var helper = CreateHelper(client);

            await helper.GetAsync("https://unit.test/api", headers: new Dictionary<string, string>
            {
                ["Authorization"] = "Bearer abc"
            });

            Assert.Equal("Bearer abc", authHeader);
        }

        [Fact]
        public async Task PostAsync_ShouldSerializeJsonAndDeserializeResponse()
        {
            string? payload = null;
            using var client = NewClient(async r =>
            {
                payload = await r.Content!.ReadAsStringAsync();
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"id\":1,\"name\":\"created\"}")
                };
            });
            var helper = CreateHelper(client);

            var result = await helper.PostAsync<SampleResponse>("https://unit.test/post", new { id = 1, name = "az" });

            Assert.NotNull(payload);
            Assert.Contains("\"id\":1", payload);
            Assert.Contains("\"name\":\"az\"", payload);
            Assert.True(result.IsSuccess);
            Assert.Equal("created", result.Data?.Name);
        }

        [Fact]
        public async Task PostAsync_StringPayload_ShouldPassThroughRawString()
        {
            string? payload = null;
            using var client = NewClient(async r =>
            {
                payload = await r.Content!.ReadAsStringAsync();
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("ok")
                };
            });
            var helper = CreateHelper(client);

            await helper.PostAsync("https://unit.test/post", "{\"raw\":\"json\"}");

            Assert.Equal("{\"raw\":\"json\"}", payload);
        }

        [Fact]
        public async Task PostFormDataAsync_KeyValue_ShouldSendFormUrlEncoded()
        {
            string? mediaType = null;
            string? payload = null;
            using var client = NewClient(async r =>
            {
                mediaType = r.Content?.Headers.ContentType?.MediaType;
                payload = await r.Content!.ReadAsStringAsync();
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("ok")
                };
            });
            var helper = CreateHelper(client);

            await helper.PostFormDataAsync("https://unit.test/form", new Dictionary<string, string>
            {
                ["username"] = "admin",
                ["password"] = "123"
            });

            Assert.Equal("application/x-www-form-urlencoded", mediaType);
            Assert.Contains("username=admin", payload);
            Assert.Contains("password=123", payload);
        }

        [Fact]
        public async Task PostSoapAsync_ShouldUseSoapContentType()
        {
            string? mediaType = null;
            using var client = NewClient(r =>
            {
                mediaType = r.Content?.Headers.ContentType?.MediaType;
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("<resp/>")
                };
            });
            var helper = CreateHelper(client);

            await helper.PostSoapAsync<string>("https://unit.test/soap", "<xml/>");

            Assert.Equal("application/soap+xml", mediaType);
        }

        [Fact]
        public async Task PutAsync_ShouldUsePutMethod()
        {
            HttpMethod? method = null;
            using var client = NewClient(r =>
            {
                method = r.Method;
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"id\":1,\"name\":\"put\"}")
                };
            });
            var helper = CreateHelper(client);

            var result = await helper.PutAsync<SampleResponse>("https://unit.test/put", new { id = 1 });

            Assert.Equal(HttpMethod.Put, method);
            Assert.True(result.IsSuccess);
            Assert.Equal("put", result.Data?.Name);
        }

        [Fact]
        public async Task PatchAsync_ShouldUsePatchMethod()
        {
            HttpMethod? method = null;
            using var client = NewClient(r =>
            {
                method = r.Method;
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"id\":2,\"name\":\"patch\"}")
                };
            });
            var helper = CreateHelper(client);

            var result = await helper.PatchAsync<SampleResponse>("https://unit.test/patch", new { id = 2 });

            Assert.Equal(HttpMethod.Patch, method);
            Assert.True(result.IsSuccess);
            Assert.Equal("patch", result.Data?.Name);
        }

        [Fact]
        public async Task DeleteAsync_NonGeneric_ShouldReturnString()
        {
            using var client = NewClient(_ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("deleted")
            });
            var helper = CreateHelper(client);

            var result = await helper.DeleteAsync("https://unit.test/item/1");

            Assert.True(result.IsSuccess);
            Assert.Equal("deleted", result.Data);
        }

        [Fact]
        public async Task DeleteAsync_Generic_ShouldDeserializeObject()
        {
            using var client = NewClient(_ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"id\":1,\"name\":\"deleted\"}")
            });
            var helper = CreateHelper(client);

            var result = await helper.DeleteAsync<SampleResponse>("https://unit.test/item/1");

            Assert.True(result.IsSuccess);
            Assert.Equal("deleted", result.Data?.Name);
        }

        [Fact]
        public async Task SendAsync_WithEnum_ShouldUseCorrespondingMethod()
        {
            HttpMethod? method = null;
            using var client = NewClient(r =>
            {
                method = r.Method;
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("ok")
                };
            });
            var helper = CreateHelper(client);

            await helper.SendAsync(HttpRequestEnum.Post, "https://unit.test/send", new StringContent("body"));

            Assert.Equal(HttpMethod.Post, method);
        }

        [Fact]
        public async Task SendAsync_WithUnsupportedEnum_ShouldThrowArgumentOutOfRangeException()
        {
            using var client = NewClient(_ => new HttpResponseMessage(HttpStatusCode.OK));
            var helper = CreateHelper(client);

            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
                helper.SendAsync((HttpRequestEnum)999, "https://unit.test/send", new StringContent("body")));
        }

        [Fact]
        public async Task SendAsync_WithMediaTypeHeader_ShouldOverrideContentType()
        {
            string? mediaType = null;
            using var client = NewClient(r =>
            {
                mediaType = r.Content?.Headers.ContentType?.MediaType;
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("ok")
                };
            });
            var helper = CreateHelper(client);

            await helper.SendAsync(HttpRequestEnum.Post, "https://unit.test/send",
                new StringContent("raw"), mediaTypeHeader: MediaTypeHeaderValue.Parse("text/plain"));

            Assert.Equal("text/plain", mediaType);
        }

        [Fact]
        public async Task SendAsync_RawRequestMessage_ShouldPassThrough()
        {
            HttpRequestMessage? captured = null;
            using var client = NewClient(r =>
            {
                captured = r;
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("ok")
                };
            });
            var helper = CreateHelper(client);

            using var req = new HttpRequestMessage(HttpMethod.Get, "https://unit.test/raw");
            using var resp = await helper.SendAsync(req);

            Assert.Same(req, captured);
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        }

        [Fact]
        public async Task SendAsync_RawRequestMessage_Null_ShouldThrowArgumentNullException()
        {
            using var client = NewClient(_ => new HttpResponseMessage(HttpStatusCode.OK));
            var helper = CreateHelper(client);

            await Assert.ThrowsAsync<ArgumentNullException>(() => helper.SendAsync((HttpRequestMessage)null!));
        }

        [Fact]
        public async Task GetAsync_EmptyUrl_ShouldThrowArgumentNullException()
        {
            using var client = NewClient(_ => new HttpResponseMessage(HttpStatusCode.OK));
            var helper = CreateHelper(client);

            await Assert.ThrowsAsync<ArgumentNullException>(() => helper.GetAsync(""));
        }

        [Fact]
        public async Task GetAsync_WithCancellationToken_ShouldCancel()
        {
            using var client = NewClient(async (_, ct) =>
            {
                await Task.Delay(TimeSpan.FromSeconds(3), ct);
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("slow")
                };
            });
            var helper = CreateHelper(client);

            using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                helper.GetAsync("https://unit.test/slow", cancellation: cts.Token));
        }

        [Fact]
        public async Task GetAsync_FallbackResponse_ShouldBeRecognizedAsFallback()
        {
            using var client = NewClient(_ =>
            {
                var resp = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
                {
                    Content = new StringContent("fallback body")
                };
                resp.Headers.Add("X-Fallback-Response", "true");
                return resp;
            });
            var helper = CreateHelper(client, failThrowException: false);

            var result = await helper.GetAsync("https://unit.test/fallback");

            Assert.False(result.IsSuccess);
            Assert.True(result.IsFallbackResponse);
        }

        private static HttpClient NewClient(Func<HttpRequestMessage, HttpResponseMessage> factory)
        {
            return new HttpClient(new DelegateHttpMessageHandler((r, _) => Task.FromResult(factory(r))));
        }

        private static HttpClient NewClient(Func<HttpRequestMessage, Task<HttpResponseMessage>> factory)
        {
            return new HttpClient(new DelegateHttpMessageHandler((r, _) => factory(r)));
        }

        private static HttpClient NewClient(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> factory)
        {
            return new HttpClient(new DelegateHttpMessageHandler(factory));
        }

        private static HttpClientHelper CreateHelper(HttpClient client, bool failThrowException = false)
        {
            var logger = new ListLogger<HttpClientHelper>();
            var options = Options.Create(new HttpClientOptions
            {
                FailThrowException = failThrowException,
                Timeout = 100
            });
            return new HttpClientHelper(client, options, logger);
        }

        private sealed class SampleResponse
        {
            public int Id { get; set; }
            public string? Name { get; set; }
        }
    }
}

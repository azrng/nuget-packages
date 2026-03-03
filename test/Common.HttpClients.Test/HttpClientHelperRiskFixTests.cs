using Common.HttpClients.Test.Helpers;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Headers;
using Xunit;

namespace Common.HttpClients.Test
{
    public class HttpClientHelperRiskFixTests
    {
        [Fact]
        public async Task SendAsync_GetWithNullContent_ShouldReturnResponse()
        {
            using var client = new HttpClient(new DelegateHttpMessageHandler((_, _) =>
                Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                                {
                                    Content = new StringContent("ok")
                                })));

            var helper = CreateHelper(client);
            var result = await helper.SendAsync(HttpRequestEnum.Get, "https://unit.test/health", null);

            Assert.Equal("ok", result);
        }

        [Fact]
        public async Task GetAsync_Generic_ShouldDeserializeJson()
        {
            using var client = new HttpClient(new DelegateHttpMessageHandler((_, _) =>
                Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                                {
                                    Content = new StringContent("{\"id\":7,\"name\":\"az\"}")
                                })));

            var helper = CreateHelper(client);
            var result = await helper.GetAsync<SampleResponse>("https://unit.test/item");

            Assert.NotNull(result);
            Assert.Equal(7, result.Id);
            Assert.Equal("az", result.Name);
        }

        [Fact]
        public async Task GetAsync_WithBearerToken_ShouldAutoPrefixAuthorization()
        {
            string authorization = string.Empty;
            using var client = new HttpClient(new DelegateHttpMessageHandler((request, _) =>
            {
                authorization = request.Headers.GetValues("Authorization").Single();
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                                       {
                                           Content = new StringContent("ok")
                                       });
            }));

            var helper = CreateHelper(client);
            await helper.GetAsync("https://unit.test/bearer", bearerToken: "abc");

            Assert.Equal("Bearer abc", authorization);
        }

        [Fact]
        public async Task PostAsync_Object_ShouldSerializeToJson()
        {
            string? payload = null;
            using var client = new HttpClient(new DelegateHttpMessageHandler(async (request, _) =>
            {
                payload = await request.Content!.ReadAsStringAsync();
                return new HttpResponseMessage(HttpStatusCode.OK)
                       {
                           Content = new StringContent("ok")
                       };
            }));

            var helper = CreateHelper(client);
            await helper.PostAsync("https://unit.test/post", new { id = 1, name = "az" });

            Assert.NotNull(payload);
            Assert.Contains("\"id\":1", payload);
            Assert.Contains("\"name\":\"az\"", payload);
        }

        [Fact]
        public async Task PostFormDataAsync_KeyValue_ShouldSendFormUrlEncoded()
        {
            string? contentType = null;
            string? payload = null;
            using var client = new HttpClient(new DelegateHttpMessageHandler(async (request, _) =>
            {
                contentType = request.Content?.Headers.ContentType?.MediaType;
                payload = await request.Content!.ReadAsStringAsync();
                return new HttpResponseMessage(HttpStatusCode.OK)
                       {
                           Content = new StringContent("ok")
                       };
            }));

            var helper = CreateHelper(client);
            var formData = new Dictionary<string, string>
            {
                { "name", "az rng" },
                { "token", "123" }
            };

            await helper.PostFormDataAsync("https://unit.test/form", formData);

            Assert.Equal("application/x-www-form-urlencoded", contentType);
            Assert.NotNull(payload);
            Assert.Contains("name=az+rng", payload);
            Assert.Contains("token=123", payload);
        }

        [Fact]
        public async Task PostSoapAsync_ShouldUseSoapContentType()
        {
            string? mediaType = null;

            using var client = new HttpClient(new DelegateHttpMessageHandler((request, _) =>
            {
                mediaType = request.Content?.Headers.ContentType?.MediaType;
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                                       {
                                           Content = new StringContent("soap-ok")
                                       });
            }));

            var helper = CreateHelper(client);
            var result = await helper.PostSoapAsync<string>("https://unit.test/soap", "<xml/>");

            Assert.Equal("soap-ok", result);
            Assert.Equal("application/soap+xml", mediaType);
        }

        [Fact]
        public async Task PutAsync_ShouldUsePutMethod()
        {
            HttpMethod? method = null;
            using var client = new HttpClient(new DelegateHttpMessageHandler((request, _) =>
            {
                method = request.Method;
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                                       {
                                           Content = new StringContent("{\"id\":1,\"name\":\"put\"}")
                                       });
            }));

            var helper = CreateHelper(client);
            var result = await helper.PutAsync<SampleResponse>("https://unit.test/put", new { id = 1 });

            Assert.Equal(HttpMethod.Put, method);
            Assert.Equal("put", result?.Name);
        }

        [Fact]
        public async Task PatchAsync_ShouldUsePatchMethod()
        {
            HttpMethod? method = null;
            using var client = new HttpClient(new DelegateHttpMessageHandler((request, _) =>
            {
                method = request.Method;
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                                       {
                                           Content = new StringContent("{\"id\":2,\"name\":\"patch\"}")
                                       });
            }));

            var helper = CreateHelper(client);
            var result = await helper.PatchAsync<SampleResponse>("https://unit.test/patch", new { id = 2 });

            Assert.Equal(HttpMethod.Patch, method);
            Assert.Equal("patch", result?.Name);
        }

        [Fact]
        public async Task SendAsync_WithMediaTypeHeader_ShouldOverrideContentType()
        {
            string? mediaType = null;
            using var client = new HttpClient(new DelegateHttpMessageHandler((request, _) =>
            {
                mediaType = request.Content?.Headers.ContentType?.MediaType;
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                                       {
                                           Content = new StringContent("ok")
                                       });
            }));

            var helper = CreateHelper(client);
            await helper.SendAsync(HttpRequestEnum.Post, "https://unit.test/send",
                new StringContent("raw-data"), MediaTypeHeaderValue.Parse("text/plain"));

            Assert.Equal("text/plain", mediaType);
        }

        [Fact]
        public async Task GetStreamAsync_WhenStatusErrorAndFailThrowDisabled_ShouldReturnStreamNull()
        {
            using var client = new HttpClient(new DelegateHttpMessageHandler((_, _) =>
                Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError)
                                {
                                    Content = new StringContent("fail")
                                })));

            var helper = CreateHelper(client, failThrowException: false);
            var result = await helper.GetStreamAsync("https://unit.test/error");

            Assert.Same(Stream.Null, result);
        }

        [Fact]
        public async Task GetStreamAsync_WhenStatusErrorAndFailThrowEnabled_ShouldThrow()
        {
            using var client = new HttpClient(new DelegateHttpMessageHandler((_, _) =>
                Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError)
                                {
                                    Content = new StringContent("fail")
                                })));

            var helper = CreateHelper(client, failThrowException: true);

            await Assert.ThrowsAsync<HttpRequestException>(async () =>
                await helper.GetStreamAsync("https://unit.test/error"));
        }

        [Fact]
        public async Task GetAsync_WhenStatusErrorAndFailThrowDisabled_ShouldReturnErrorBody()
        {
            using var client = new HttpClient(new DelegateHttpMessageHandler((_, _) =>
                Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError)
                                {
                                    Content = new StringContent("error-body")
                                })));

            var helper = CreateHelper(client, failThrowException: false);
            var result = await helper.GetAsync("https://unit.test/error");

            Assert.Equal("error-body", result);
        }

        [Fact]
        public async Task DeleteAsync_Generic_WhenStatusErrorAndFailThrowDisabled_ShouldReturnDefault()
        {
            using var client = new HttpClient(new DelegateHttpMessageHandler((_, _) =>
                Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError)
                                {
                                    Content = new StringContent("error-body")
                                })));

            var helper = CreateHelper(client, failThrowException: false);
            var result = await helper.DeleteAsync<SampleResponse>("https://unit.test/error");

            Assert.Null(result);
        }

        [Fact]
        public async Task SendAsync_WithUnsupportedMethod_ShouldThrow()
        {
            using var client = new HttpClient(new DelegateHttpMessageHandler((_, _) =>
                Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK))));

            var helper = CreateHelper(client);
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
                await helper.SendAsync((HttpRequestEnum)999, "https://unit.test/send", null!));
        }

        [Fact]
        public async Task SendAsync_RequestMessage_ShouldPassThrough()
        {
            HttpRequestMessage? capturedRequest = null;
            using var client = new HttpClient(new DelegateHttpMessageHandler((request, _) =>
            {
                capturedRequest = request;
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                                       {
                                           Content = new StringContent("ok")
                                       });
            }));

            var helper = CreateHelper(client);
            using var request = new HttpRequestMessage(HttpMethod.Get, "https://unit.test/direct");
            using var response = await helper.SendAsync(request);

            Assert.Same(request, capturedRequest);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task SendAsync_RequestMessageNull_ShouldThrow()
        {
            using var client = new HttpClient(new DelegateHttpMessageHandler((_, _) =>
                Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK))));

            var helper = CreateHelper(client);
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await helper.SendAsync((HttpRequestMessage)null!));
        }

        [Fact]
        public async Task GetAsync_WithEmptyUrl_ShouldThrow()
        {
            using var client = new HttpClient(new DelegateHttpMessageHandler((_, _) =>
                Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK))));

            var helper = CreateHelper(client);
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await helper.GetAsync(string.Empty));
        }

        [Fact]
        public async Task GetAsync_WithCancellationToken_ShouldCancel()
        {
            using var client = new HttpClient(new DelegateHttpMessageHandler(async (_, cancellation) =>
            {
                await Task.Delay(TimeSpan.FromSeconds(3), cancellation);
                return new HttpResponseMessage(HttpStatusCode.OK)
                       {
                           Content = new StringContent("slow")
                       };
            }));

            var helper = CreateHelper(client);
            using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));

            await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
                await helper.GetAsync("https://unit.test/slow", cancellation: cts.Token));
        }

        [Fact]
        public async Task GetAsync_WithoutTimeoutParameter_ShouldStillSucceed()
        {
            using var client = new HttpClient(new DelegateHttpMessageHandler(async (_, cancellation) =>
            {
                await Task.Delay(TimeSpan.FromMilliseconds(1200), cancellation);
                return new HttpResponseMessage(HttpStatusCode.OK)
                       {
                           Content = new StringContent("slow-but-ok")
                       };
            }));

            var helper = CreateHelper(client);
            var result = await helper.GetAsync("https://unit.test/slow", cancellation: CancellationToken.None);

            Assert.Equal("slow-but-ok", result);
        }

        private static HttpClientHelper CreateHelper(HttpClient client, bool failThrowException = true)
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

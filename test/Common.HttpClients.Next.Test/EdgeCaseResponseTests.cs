using Common.HttpClients.Next.Test.Helpers;
using Microsoft.Extensions.Options;

namespace Common.HttpClients.Next.Test
{
    /// <summary>
    /// 响应内容边缘场景：空响应、空 JSON、null 字段、大响应、UTF-8、JSON 数组
    /// </summary>
    public class EdgeCaseResponseTests
    {
        [Fact]
        public async Task GetAsync_EmptyString_ShouldReturnSuccessWithEmpty()
        {
            using var client = NewClient(_ => Ok(""));
            var helper = CreateHelper(client);

            var result = await helper.GetAsync("https://unit.test/empty");

            Assert.True(result.IsSuccess);
            Assert.Equal("", result.Data);
        }

        [Fact]
        public async Task GetAsync_Generic_EmptyString_ShouldReturnSuccessWithDefault()
        {
            using var client = NewClient(_ => Ok(""));
            var helper = CreateHelper(client);

            var result = await helper.GetAsync<SampleResponse>("https://unit.test/empty");

            Assert.True(result.IsSuccess);
            Assert.Null(result.Data);
        }

        [Fact]
        public async Task GetAsync_EmptyJsonObject_ShouldReturnDefaultInstance()
        {
            using var client = NewClient(_ => Ok("{}"));
            var helper = CreateHelper(client);

            var result = await helper.GetAsync<SampleResponse>("https://unit.test/empty-json");

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal(0, result.Data!.Id);
            Assert.Null(result.Data.Name);
        }

        [Fact]
        public async Task GetAsync_NullFields_ShouldDeserializeCorrectly()
        {
            using var client = NewClient(_ => Ok("{\"id\":1,\"name\":null}"));
            var helper = CreateHelper(client);

            var result = await helper.GetAsync<SampleResponse>("https://unit.test/null-field");

            Assert.True(result.IsSuccess);
            Assert.Equal(1, result.Data?.Id);
            Assert.Null(result.Data?.Name);
        }

        [Fact]
        public async Task GetAsync_204NoContent_ShouldReturnSuccessWithDefault()
        {
            using var client = NewClient(_ => new HttpResponseMessage(HttpStatusCode.NoContent));
            var helper = CreateHelper(client);

            var result = await helper.GetAsync<SampleResponse>("https://unit.test/no-content");

            Assert.True(result.IsSuccess);
            Assert.Null(result.Data);
        }

        [Fact]
        public async Task GetAsync_NotFound_WhenFailThrowDisabled_ShouldReturnFailedResult()
        {
            using var client = NewClient(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                Content = new StringContent("not found")
            });
            var helper = CreateHelper(client, failThrowException: false);

            var result = await helper.GetAsync<SampleResponse>("https://unit.test/404");

            Assert.False(result.IsSuccess);
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
            Assert.Contains("not found", result.ErrorMessage);
        }

        [Fact]
        public async Task GetAsync_LargeResponse_ShouldBeHandled()
        {
            var large = new string('A', 10_000);
            using var client = NewClient(_ => Ok(large));
            var helper = CreateHelper(client);

            var result = await helper.GetAsync("https://unit.test/large");

            Assert.True(result.IsSuccess);
            Assert.Equal(10_000, result.Data?.Length);
        }

        [Fact]
        public async Task GetAsync_Utf8Response_ShouldDecodeChinese()
        {
            var chinese = "测试中文内容";
            using var client = NewClient(_ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(chinese, Encoding.UTF8, "application/json")
            });
            var helper = CreateHelper(client);

            var result = await helper.GetAsync("https://unit.test/utf8");

            Assert.True(result.IsSuccess);
            Assert.Equal(chinese, result.Data);
        }

        [Fact]
        public async Task GetAsync_JsonArray_ShouldDeserializeList()
        {
            using var client = NewClient(_ => Ok("[{\"id\":1,\"name\":\"a\"},{\"id\":2,\"name\":\"b\"}]"));
            var helper = CreateHelper(client);

            var result = await helper.GetAsync<List<SampleResponse>>("https://unit.test/array");

            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Data?.Count);
            Assert.Equal("a", result.Data?[0].Name);
            Assert.Equal("b", result.Data?[1].Name);
        }

        [Fact]
        public async Task GetAsync_ResponseWithBearerToken_ShouldBePreservedInRawBody()
        {
            using var client = NewClient(_ => Ok("{\"access_token\":\"abc.def\"}"));
            var helper = CreateHelper(client);

            var result = await helper.GetAsync<Dictionary<string, string>>("https://unit.test/token");

            Assert.True(result.IsSuccess);
            Assert.Equal("abc.def", result.Data?["access_token"]);
            Assert.Equal("{\"access_token\":\"abc.def\"}", result.RawBody);
        }

        private static HttpResponseMessage Ok(string body) => new(HttpStatusCode.OK)
        {
            Content = new StringContent(body)
        };

        private static HttpClient NewClient(Func<HttpRequestMessage, HttpResponseMessage> factory)
        {
            return new HttpClient(new DelegateHttpMessageHandler((r, _) => Task.FromResult(factory(r))));
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

using Common.HttpClients.Test.Helpers;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using Xunit;

namespace Common.HttpClients.Test
{
    /// <summary>
    /// 边缘响应场景测试
    /// </summary>
    public class EdgeCaseResponseTests
    {
        /// <summary>
        /// 测试空字符串响应
        /// </summary>
        [Fact]
        public async Task EmptyResponse_NonGeneric_ShouldReturnEmptyString()
        {
            using var client = new HttpClient(new DelegateHttpMessageHandler((_, _) =>
                Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(string.Empty)
                })));

            var helper = CreateHelper(client);
            var result = await helper.GetAsync("https://unit.test/empty");

            Assert.Equal(string.Empty, result);
        }

        /// <summary>
        /// 测试空字符串泛型响应
        /// </summary>
        [Fact]
        public async Task EmptyResponse_Generic_ShouldReturnDefault()
        {
            using var client = new HttpClient(new DelegateHttpMessageHandler((_, _) =>
                Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(string.Empty)
                })));

            var helper = CreateHelper(client);
            var result = await helper.GetAsync<SampleResponse>("https://unit.test/empty");

            Assert.Null(result);
        }

        /// <summary>
        /// 测试空JSON对象响应
        /// </summary>
        [Fact]
        public async Task EmptyJsonObject_ShouldReturnDefault()
        {
            using var client = new HttpClient(new DelegateHttpMessageHandler((_, _) =>
                Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{}")
                })));

            var helper = CreateHelper(client);
            var result = await helper.GetAsync<SampleResponse>("https://unit.test/empty-json");

            Assert.NotNull(result);
            Assert.Equal(0, result.Id);
            Assert.Null(result.Name);
        }

        /// <summary>
        /// 测试null JSON字段值
        /// </summary>
        [Fact]
        public async Task NullJsonFields_ShouldDeserializeCorrectly()
        {
            using var client = new HttpClient(new DelegateHttpMessageHandler((_, _) =>
                Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"id\":1,\"name\":null}")
                })));

            var helper = CreateHelper(client);
            var result = await helper.GetAsync<SampleResponse>("https://unit.test/null-field");

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Null(result.Name);
        }

        /// <summary>
        /// 测试404状态码且FailThrowException=false时返回null
        /// </summary>
        [Fact]
        public async Task NotFound_WithFailThrowDisabled_ShouldReturnNull()
        {
            using var client = new HttpClient(new DelegateHttpMessageHandler((_, _) =>
                Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new StringContent("not found")
                })));

            var helper = CreateHelper(client, failThrowException: false);
            var result = await helper.GetAsync<SampleResponse>("https://unit.test/notfound");

            Assert.Null(result);
        }

        /// <summary>
        /// 测试204 No Content响应
        /// </summary>
        [Fact]
        public async Task NoContentResponse_NonGeneric_ShouldReturnEmpty()
        {
            using var client = new HttpClient(new DelegateHttpMessageHandler((_, _) =>
                Task.FromResult(new HttpResponseMessage(HttpStatusCode.NoContent))));

            var helper = CreateHelper(client);
            var result = await helper.GetAsync("https://unit.test/nocontent");

            Assert.Equal(string.Empty, result);
        }

        /// <summary>
        /// 测试204 No Content泛型响应
        /// </summary>
        [Fact]
        public async Task NoContentResponse_Generic_ShouldReturnDefault()
        {
            using var client = new HttpClient(new DelegateHttpMessageHandler((_, _) =>
                Task.FromResult(new HttpResponseMessage(HttpStatusCode.NoContent))));

            var helper = CreateHelper(client);
            var result = await helper.GetAsync<SampleResponse>("https://unit.test/nocontent");

            Assert.Null(result);
        }

        /// <summary>
        /// 测试大响应内容
        /// </summary>
        [Fact]
        public async Task LargeResponse_ShouldHandleCorrectly()
        {
            var largeContent = new string('A', 10000);
            using var client = new HttpClient(new DelegateHttpMessageHandler((_, _) =>
                Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(largeContent)
                })));

            var helper = CreateHelper(client);
            var result = await helper.GetAsync("https://unit.test/large");

            Assert.Equal(10000, result.Length);
        }

        /// <summary>
        /// 测试UTF-8编码响应
        /// </summary>
        [Fact]
        public async Task Utf8Response_ShouldDecodeCorrectly()
        {
            var chineseText = "测试中文内容";
            using var client = new HttpClient(new DelegateHttpMessageHandler((_, _) =>
                Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(chineseText, System.Text.Encoding.UTF8, "application/json")
                })));

            var helper = CreateHelper(client);
            var result = await helper.GetAsync("https://unit.test/utf8");

            Assert.Equal(chineseText, result);
        }

        /// <summary>
        /// 测试JSON数组响应
        /// </summary>
        [Fact]
        public async Task JsonArrayResponse_ShouldDeserializeCorrectly()
        {
            using var client = new HttpClient(new DelegateHttpMessageHandler((_, _) =>
                Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("[{\"id\":1,\"name\":\"a\"},{\"id\":2,\"name\":\"b\"}]")
                })));

            var helper = CreateHelper(client);
            var result = await helper.GetAsync<SampleResponse[]>("https://unit.test/array");

            Assert.NotNull(result);
            Assert.Equal(2, result.Length);
            Assert.Equal(1, result[0].Id);
            Assert.Equal("a", result[0].Name);
        }

        private static HttpClientHelper CreateHelper(HttpClient client, bool failThrowException = true)
        {
            var logger = new ListLogger<HttpClientHelper>();
            var options = Microsoft.Extensions.Options.Options.Create(new Common.HttpClients.HttpClientOptions
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

using Common.HttpClients.Test.Helpers;
using Microsoft.Extensions.Options;
using System.IO;
using System.Net;
using Xunit;

namespace Common.HttpClients.Test
{
    /// <summary>
    /// GetStreamAsync 资源释放和行为测试
    /// </summary>
    public class ResponseStreamTests
    {
        /// <summary>
        /// 测试 GetStreamAsync 返回有效的可读取流
        /// </summary>
        [Fact]
        public async Task GetStreamAsync_ShouldReturnReadableStream()
        {
            using var client = new HttpClient(new DelegateHttpMessageHandler((_, _) =>
                Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("test content for stream")
                })));

            var helper = CreateHelper(client);
            var stream = await helper.GetStreamAsync("https://unit.test/stream-test");

            Assert.NotNull(stream);
            Assert.NotSame(Stream.Null, stream);

            // 验证可以读取流
            using var reader = new StreamReader(stream);
            var content = await reader.ReadToEndAsync();
            Assert.Equal("test content for stream", content);
        }

        /// <summary>
        /// 测试 GetStreamAsync 在错误时返回 Stream.Null
        /// </summary>
        [Fact]
        public async Task GetStreamAsync_WhenError_ShouldReturnStreamNull()
        {
            using var client = new HttpClient(new DelegateHttpMessageHandler((_, _) =>
                Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent("error")
                })));

            var helper = CreateHelper(client, failThrowException: false);
            var stream = await helper.GetStreamAsync("https://unit.test/stream-error");

            Assert.Same(Stream.Null, stream);
        }

        /// <summary>
        /// 测试 GetStreamAsync 支持大文件
        /// </summary>
        [Fact]
        public async Task GetStreamAsync_ShouldHandleLargeContent()
        {
            var largeContent = new string('A', 100000);
            using var client = new HttpClient(new DelegateHttpMessageHandler((_, _) =>
                Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(largeContent)
                })));

            var helper = CreateHelper(client);
            var stream = await helper.GetStreamAsync("https://unit.test/large-stream");

            using var reader = new StreamReader(stream);
            var content = await reader.ReadToEndAsync();

            Assert.Equal(100000, content.Length);
        }

        /// <summary>
        /// 测试 GetStreamAsync 返回的流可以被释放多次
        /// </summary>
        [Fact]
        public async Task GetStreamAsync_MultipleDispose_ShouldNotThrow()
        {
            using var client = new HttpClient(new DelegateHttpMessageHandler((_, _) =>
                Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("test")
                })));

            var helper = CreateHelper(client);
            var stream = await helper.GetStreamAsync("https://unit.test/multi-dispose");

            // 读取内容
            using var reader = new StreamReader(stream);
            await reader.ReadToEndAsync();

            // 多次 dispose 不应抛出异常
            await stream.DisposeAsync();
            await stream.DisposeAsync();

            // 如果没有抛出异常则测试通过
            Assert.True(true);
        }

        /// <summary>
        /// 测试 GetStreamAsync 支持取消令牌
        /// </summary>
        [Fact]
        public async Task GetStreamAsync_ShouldSupportCancellationToken()
        {
            using var client = new HttpClient(new DelegateHttpMessageHandler((_, _) =>
                Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("test")
                })));

            var helper = CreateHelper(client);
            var cts = new CancellationTokenSource();

            // 正常完成的请求
            var stream = await helper.GetStreamAsync("https://unit.test/cancel-test", cancellation: cts.Token);

            using var reader = new StreamReader(stream);
            var content = await reader.ReadToEndAsync();

            Assert.Equal("test", content);
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
    }
}

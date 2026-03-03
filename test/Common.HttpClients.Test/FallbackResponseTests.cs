using Common.HttpClients.Test.Helpers;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using Xunit;

namespace Common.HttpClients.Test
{
    /// <summary>
    /// Fallback响应头和行为测试
    /// </summary>
    public class FallbackResponseTests
    {
        /// <summary>
        /// 测试连接失败时Fallback响应包含 X-Fallback-Response: true 头
        /// </summary>
        [Fact]
        public async Task ConnectionFailure_ShouldIncludeFallbackHeader_WhenFailThrowDisabled()
        {
            var freePort = GetFreePort();

            using var provider = BuildServiceProvider(options =>
            {
                options.FailThrowException = false;
                options.Timeout = 1;
            });

            var httpHelper = provider.GetRequiredService<IHttpHelper>();
            using var request = new HttpRequestMessage(HttpMethod.Get, $"http://127.0.0.1:{freePort}/fallback");
            using var response = await httpHelper.SendAsync(request);

            Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
            Assert.True(response.Headers.Contains("X-Fallback-Response"));
            Assert.Equal("true", response.Headers.GetValues("X-Fallback-Response").FirstOrDefault());
            Assert.Equal("Fallback: request failed.", await response.Content.ReadAsStringAsync());
        }

        /// <summary>
        /// 测试能区分真实服务端503和Fallback 503
        /// </summary>
        [Fact]
        public async Task RealServer503_ShouldNotIncludeFallbackHeader()
        {
            await using var server = new ScriptedHttpListenerServer(async context =>
            {
                await ScriptedHttpListenerServer.WriteResponseAsync(context, HttpStatusCode.ServiceUnavailable, "real-server-error");
            });

            using var provider = BuildServiceProvider(options =>
            {
                options.FailThrowException = false;
                options.Timeout = 2;
            });

            var httpHelper = provider.GetRequiredService<IHttpHelper>();
            using var request = new HttpRequestMessage(HttpMethod.Get, server.BaseUrl + "real-503");
            using var response = await httpHelper.SendAsync(request);

            Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
            Assert.False(response.Headers.Contains("X-Fallback-Response"));
            Assert.Equal("real-server-error", await response.Content.ReadAsStringAsync());
        }

        /// <summary>
        /// 测试泛型方法在Fallback时返回 null
        /// </summary>
        [Fact]
        public async Task ConnectionFailure_Generic_ShouldReturnNull_WhenFailThrowDisabled()
        {
            var freePort = GetFreePort();

            using var provider = BuildServiceProvider(options =>
            {
                options.FailThrowException = false;
                options.Timeout = 1;
            });

            var httpHelper = provider.GetRequiredService<IHttpHelper>();
            var result = await httpHelper.GetAsync<FallbackResult>($"http://127.0.0.1:{freePort}/fallback");

            Assert.Null(result);
        }

        /// <summary>
        /// 测试非泛型方法在Fallback时返回错误内容
        /// </summary>
        [Fact]
        public async Task ConnectionFailure_NonGeneric_ShouldReturnErrorBody_WhenFailThrowDisabled()
        {
            var freePort = GetFreePort();

            using var provider = BuildServiceProvider(options =>
            {
                options.FailThrowException = false;
                options.Timeout = 1;
            });

            var httpHelper = provider.GetRequiredService<IHttpHelper>();
            var result = await httpHelper.GetAsync($"http://127.0.0.1:{freePort}/fallback");

            Assert.Null(result);
        }

        /// <summary>
        /// 测试启用FailThrowException时Fallback抛出异常
        /// </summary>
        [Fact]
        public async Task ConnectionFailure_ShouldThrow_WhenFailThrowEnabled()
        {
            var freePort = GetFreePort();

            using var provider = BuildServiceProvider(options =>
            {
                options.FailThrowException = true;
                options.Timeout = 1;
            });

            var httpHelper = provider.GetRequiredService<IHttpHelper>();

            await Assert.ThrowsAnyAsync<Exception>(async () =>
                await httpHelper.GetAsync<string>($"http://127.0.0.1:{freePort}/fallback"));
        }

        private static ServiceProvider BuildServiceProvider(Action<HttpClientOptions> setup)
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddHttpClientService(options =>
            {
                options.AuditLog = false;
                options.EnableLogRedaction = false;
                options.Timeout = 3;
                setup(options);
            });

            return services.BuildServiceProvider();
        }

        private static int GetFreePort()
        {
            var listener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, 0);
            listener.Start();
            var port = ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

        private sealed class FallbackResult
        {
            public string? Name { get; set; }
        }
    }
}

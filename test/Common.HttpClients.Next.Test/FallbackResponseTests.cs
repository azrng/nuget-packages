using Common.HttpClients.Next.Test.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace Common.HttpClients.Next.Test
{
    /// <summary>
    /// Polly Fallback 策略行为测试
    /// </summary>
    public class FallbackResponseTests
    {
        [Fact]
        public async Task ConnectionFailure_WhenFailThrowDisabled_ShouldReturnFallbackResponse()
        {
            int closedPort = GetFreeTcpPort();
            using var provider = BuildProvider(o =>
            {
                o.FailThrowException = false;
                o.Timeout = 1;
                o.MaxRetryAttempts = 0;
            });

            var helper = provider.GetRequiredService<IHttpHelper>();
            var result = await helper.GetAsync($"http://127.0.0.1:{closedPort}/fail");

            Assert.False(result.IsSuccess);
            Assert.True(result.IsFallbackResponse);
            Assert.Equal(HttpStatusCode.ServiceUnavailable, result.StatusCode);
        }

        [Fact]
        public async Task ConnectionFailure_WhenFailThrowEnabled_ShouldThrowHttpRequestException()
        {
            // 回归：原代码 FallbackAction 在 args.Outcome.Exception 为 null 时会抛 NRE；
            //      失败状态码路径上 Exception 为 null，应改为抛 HttpRequestException
            await using var server = new ScriptedHttpListenerServer(ctx =>
                ScriptedHttpListenerServer.WriteResponseAsync(ctx, HttpStatusCode.InternalServerError, "server-error"));

            using var provider = BuildProvider(o =>
            {
                o.FailThrowException = true;
                o.Timeout = 2;
                o.MaxRetryAttempts = 0;
                o.RetryDelaySeconds = 1;
            });

            var helper = provider.GetRequiredService<IHttpHelper>();
            await Assert.ThrowsAsync<HttpRequestException>(() =>
                helper.GetAsync($"{server.BaseUrl}server-fail"));
        }

        [Fact]
        public async Task ConnectionFailure_NonGeneric_ShouldCarryFallbackBody()
        {
            int closedPort = GetFreeTcpPort();
            using var provider = BuildProvider(o =>
            {
                o.FailThrowException = false;
                o.Timeout = 1;
                o.MaxRetryAttempts = 0;
            });

            var helper = provider.GetRequiredService<IHttpHelper>();
            var result = await helper.GetAsync($"http://127.0.0.1:{closedPort}/fail");

            Assert.Contains("Fallback: request failed.", result.ErrorMessage);
        }

        [Fact]
        public async Task RealServer503_ShouldNotBeMarkedAsFallback()
        {
            await using var server = new ScriptedHttpListenerServer(ctx =>
                ScriptedHttpListenerServer.WriteResponseAsync(ctx, HttpStatusCode.ServiceUnavailable, "real-503"));

            using var provider = BuildProvider(o =>
            {
                o.FailThrowException = false;
                o.Timeout = 2;
                o.MaxRetryAttempts = 0;
                o.RetryDelaySeconds = 1;
            });

            var helper = provider.GetRequiredService<IHttpHelper>();
            var result = await helper.GetAsync($"{server.BaseUrl}real-503");

            Assert.False(result.IsSuccess);
            Assert.False(result.IsFallbackResponse);
            Assert.Equal("real-503", result.RawBody);
        }

        [Fact]
        public async Task FallbackResponse_ShouldCarrySpecialHeader()
        {
            int closedPort = GetFreeTcpPort();
            using var provider = BuildProvider(o =>
            {
                o.FailThrowException = false;
                o.Timeout = 1;
                o.MaxRetryAttempts = 0;
            });

            var helper = provider.GetRequiredService<IHttpHelper>();
            using var request = new HttpRequestMessage(HttpMethod.Get, $"http://127.0.0.1:{closedPort}/fail");
            using var response = await helper.SendAsync(request);

            Assert.True(response.Headers.Contains("X-Fallback-Response"));
            Assert.Equal("true", response.Headers.GetValues("X-Fallback-Response").Single());
        }

        private static ServiceProvider BuildProvider(Action<HttpClientOptions> setup)
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddHttpClientService(o =>
            {
                o.AuditLog = false;
                o.EnableLogRedaction = false;
                o.RetryDelaySeconds = 1;
                setup(o);
            });
            return services.BuildServiceProvider();
        }

        private static int GetFreeTcpPort()
        {
            using var listener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, 0);
            listener.Start();
            int port = ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }
    }
}

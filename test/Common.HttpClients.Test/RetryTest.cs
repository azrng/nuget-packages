using Common.HttpClients.Test.Helpers;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Xunit;

namespace Common.HttpClients.Test
{
    public class RetryTest
    {
        [Fact]
        public async Task FailureStatus_ShouldRetry_AndFinallySucceed()
        {
            var attempts = 0;
            await using var server = new ScriptedHttpListenerServer(async context =>
            {
                var current = Interlocked.Increment(ref attempts);
                if (current <= 3)
                {
                    await ScriptedHttpListenerServer.WriteResponseAsync(context, HttpStatusCode.InternalServerError, "fail");
                    return;
                }

                await ScriptedHttpListenerServer.WriteResponseAsync(context, HttpStatusCode.OK, "ok");
            });

            using var provider = BuildServiceProvider(options =>
            {
                options.FailThrowException = true;
                options.Timeout = 3;
            });

            var httpHelper = provider.GetRequiredService<IHttpHelper>();
            var result = await httpHelper.GetAsync<string>(server.BaseUrl + "failure");

            Assert.Equal("ok", result);
            Assert.Equal(4, attempts);
        }

        [Fact]
        public async Task Timeout_ShouldRetry_AndFinallySucceed()
        {
            var attempts = 0;
            await using var server = new ScriptedHttpListenerServer(async context =>
            {
                var current = Interlocked.Increment(ref attempts);
                if (current <= 2)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(1400));
                }

                await ScriptedHttpListenerServer.WriteResponseAsync(context, HttpStatusCode.OK, "ok");
            });

            using var provider = BuildServiceProvider(options =>
            {
                options.FailThrowException = true;
                options.Timeout = 1;
            });

            var httpHelper = provider.GetRequiredService<IHttpHelper>();
            var sw = Stopwatch.StartNew();
            var result = await httpHelper.GetAsync<string>(server.BaseUrl + "timeout");
            sw.Stop();

            Assert.Equal("ok", result);
            Assert.Equal(3, attempts);
            Assert.True(sw.ElapsedMilliseconds >= 2000);
        }

        [Fact]
        public async Task CallerCancellation_ShouldNotRetry()
        {
            var attempts = 0;
            await using var server = new ScriptedHttpListenerServer(async context =>
            {
                Interlocked.Increment(ref attempts);
                await Task.Delay(TimeSpan.FromSeconds(3));
                await ScriptedHttpListenerServer.WriteResponseAsync(context, HttpStatusCode.OK, "late");
            });

            using var provider = BuildServiceProvider(options =>
            {
                options.FailThrowException = true;
                options.Timeout = 5;
            });

            var httpHelper = provider.GetRequiredService<IHttpHelper>();
            using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(200));

            await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
                await httpHelper.GetAsync<string>(server.BaseUrl + "cancel", cancellation: cts.Token));

            Assert.Equal(1, attempts);
        }

        [Fact]
        public async Task Unauthorized_ShouldRetry_WhenEnabled()
        {
            var attempts = 0;
            await using var server = new ScriptedHttpListenerServer(async context =>
            {
                var current = Interlocked.Increment(ref attempts);
                if (current <= 2)
                {
                    await ScriptedHttpListenerServer.WriteResponseAsync(context, HttpStatusCode.Unauthorized, "unauthorized");
                    return;
                }

                await ScriptedHttpListenerServer.WriteResponseAsync(context, HttpStatusCode.OK, "ok");
            });

            using var provider = BuildServiceProvider(options =>
            {
                options.FailThrowException = true;
                options.Timeout = 3;
                options.RetryOnUnauthorized = true;
            });

            var httpHelper = provider.GetRequiredService<IHttpHelper>();
            var result = await httpHelper.GetAsync<string>(server.BaseUrl + "unauthorized");

            Assert.Equal("ok", result);
            Assert.Equal(3, attempts);
        }

        [Fact]
        public async Task Unauthorized_ShouldNotRetry_WhenDisabled()
        {
            var attempts = 0;
            await using var server = new ScriptedHttpListenerServer(async context =>
            {
                Interlocked.Increment(ref attempts);
                await ScriptedHttpListenerServer.WriteResponseAsync(context, HttpStatusCode.Unauthorized, "unauthorized");
            });

            using var provider = BuildServiceProvider(options =>
            {
                options.FailThrowException = true;
                options.Timeout = 3;
                options.RetryOnUnauthorized = false;
            });

            var httpHelper = provider.GetRequiredService<IHttpHelper>();

            await Assert.ThrowsAsync<HttpRequestException>(async () =>
                await httpHelper.GetAsync<string>(server.BaseUrl + "unauthorized"));

            Assert.Equal(1, attempts);
        }

        [Fact]
        public async Task RequestTimeoutStatus_ShouldRetry_AndFinallySucceed()
        {
            var attempts = 0;
            await using var server = new ScriptedHttpListenerServer(async context =>
            {
                var current = Interlocked.Increment(ref attempts);
                if (current <= 2)
                {
                    await ScriptedHttpListenerServer.WriteResponseAsync(context, HttpStatusCode.RequestTimeout, "request-timeout");
                    return;
                }

                await ScriptedHttpListenerServer.WriteResponseAsync(context, HttpStatusCode.OK, "ok");
            });

            using var provider = BuildServiceProvider(options =>
            {
                options.FailThrowException = true;
                options.Timeout = 3;
            });

            var httpHelper = provider.GetRequiredService<IHttpHelper>();
            var result = await httpHelper.GetAsync<string>(server.BaseUrl + "request-timeout");

            Assert.Equal("ok", result);
            Assert.Equal(3, attempts);
        }

        [Fact]
        public async Task ConnectionFailure_GetAsync_ShouldReturnNull_WhenFailThrowDisabled()
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

        [Fact]
        public async Task ConnectionFailure_SendAsync_ShouldReturn503_WhenFailThrowDisabled()
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
            var content = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
            Assert.Equal("Fallback: request failed.", content);
        }

        [Fact]
        public async Task ConnectionFailure_GenericGet_ShouldReturnDefault_WhenFailThrowDisabled()
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

        [Fact]
        public async Task RetryPolicy_ShouldRespectMaxRetryAttempts()
        {
            var attempts = 0;
            await using var server = new ScriptedHttpListenerServer(async context =>
            {
                Interlocked.Increment(ref attempts);
                await ScriptedHttpListenerServer.WriteResponseAsync(context, HttpStatusCode.InternalServerError, "fail");
            });

            using var provider = BuildServiceProvider(options =>
            {
                options.FailThrowException = true;
                options.Timeout = 2;
                options.MaxRetryAttempts = 1;
            });

            var httpHelper = provider.GetRequiredService<IHttpHelper>();
            await Assert.ThrowsAsync<HttpRequestException>(async () =>
                await httpHelper.GetAsync<string>(server.BaseUrl + "max-retry"));

            Assert.Equal(2, attempts);
        }

        [Fact]
        public async Task RetryPolicy_WithRetryDelaySecondsConfigured_ShouldStillRetryAndSucceed()
        {
            var attempts = 0;
            await using var server = new ScriptedHttpListenerServer(async context =>
            {
                var current = Interlocked.Increment(ref attempts);
                if (current == 1)
                {
                    await ScriptedHttpListenerServer.WriteResponseAsync(context, HttpStatusCode.InternalServerError, "fail");
                    return;
                }

                await ScriptedHttpListenerServer.WriteResponseAsync(context, HttpStatusCode.OK, "ok");
            });

            using var provider = BuildServiceProvider(options =>
            {
                options.FailThrowException = true;
                options.Timeout = 2;
                options.MaxRetryAttempts = 1;
                options.RetryDelaySeconds = 1;
            });

            var httpHelper = provider.GetRequiredService<IHttpHelper>();
            var sw = Stopwatch.StartNew();
            var result = await httpHelper.GetAsync<string>(server.BaseUrl + "retry-delay");
            sw.Stop();

            Assert.Equal("ok", result);
            Assert.Equal(2, attempts);
            Assert.True(sw.ElapsedMilliseconds >= 0);
        }

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
                options.EnableLogRedaction = true;
                options.FailThrowException = true;
                options.Timeout = 3;
                options.MaxOutputResponseLength = 0;
                setup(options);
            });

            return services.BuildServiceProvider();
        }

        private static int GetFreePort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

        private sealed class FallbackResult
        {
            public string? Name { get; set; }
        }
    }
}

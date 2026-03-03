using Common.HttpClients.Test.Helpers;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Net;
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
    }
}

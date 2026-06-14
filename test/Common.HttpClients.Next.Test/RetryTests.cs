using Common.HttpClients.Next.Test.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace Common.HttpClients.Next.Test
{
    /// <summary>
    /// 重试策略测试：5xx 触发重试、401 不重试（默认）、408 重试
    /// </summary>
    public class RetryTests
    {
        [Fact]
        public async Task Retry_OnServerError_ShouldRetryThenSucceed()
        {
            int attempts = 0;
            await using var server = new ScriptedHttpListenerServer(async ctx =>
            {
                attempts++;
                if (attempts < 3)
                {
                    await ScriptedHttpListenerServer.WriteResponseAsync(ctx, HttpStatusCode.InternalServerError, "fail");
                    return;
                }
                await ScriptedHttpListenerServer.WriteResponseAsync(ctx, HttpStatusCode.OK, "{\"id\":1,\"name\":\"ok\"}");
            });

            using var provider = BuildProvider(o =>
            {
                o.MaxRetryAttempts = 5;
                o.RetryDelaySeconds = 1;
            });

            var helper = provider.GetRequiredService<IHttpHelper>();
            var result = await helper.GetAsync<SampleResponse>($"{server.BaseUrl}retry");

            Assert.True(result.IsSuccess);
            Assert.Equal("ok", result.Data?.Name);
            Assert.True(attempts >= 3);
        }

        [Fact]
        public async Task Retry_ExceedMaxAttempts_ShouldReturnFailure()
        {
            int attempts = 0;
            await using var server = new ScriptedHttpListenerServer(async ctx =>
            {
                attempts++;
                await ScriptedHttpListenerServer.WriteResponseAsync(ctx, HttpStatusCode.InternalServerError, "always-fail");
            });

            using var provider = BuildProvider(o =>
            {
                o.MaxRetryAttempts = 2;
                o.RetryDelaySeconds = 1;
            });

            var helper = provider.GetRequiredService<IHttpHelper>();
            var result = await helper.GetAsync<SampleResponse>($"{server.BaseUrl}always");

            Assert.False(result.IsSuccess);
            Assert.True(attempts >= 2);
        }

        [Fact]
        public async Task Retry_ZeroAttempts_ShouldNotRetry()
        {
            int attempts = 0;
            await using var server = new ScriptedHttpListenerServer(async ctx =>
            {
                attempts++;
                await ScriptedHttpListenerServer.WriteResponseAsync(ctx, HttpStatusCode.InternalServerError, "fail");
            });

            using var provider = BuildProvider(o =>
            {
                o.MaxRetryAttempts = 0;
                o.RetryDelaySeconds = 1;
            });

            var helper = provider.GetRequiredService<IHttpHelper>();
            var result = await helper.GetAsync($"{server.BaseUrl}noretry");

            Assert.False(result.IsSuccess);
            Assert.Equal(1, attempts);
        }

        [Fact]
        public async Task Retry_On401_ShouldNotRetryByDefault()
        {
            int attempts = 0;
            await using var server = new ScriptedHttpListenerServer(async ctx =>
            {
                attempts++;
                await ScriptedHttpListenerServer.WriteResponseAsync(ctx, HttpStatusCode.Unauthorized, "no-auth");
            });

            using var provider = BuildProvider(o =>
            {
                o.MaxRetryAttempts = 3;
                o.RetryDelaySeconds = 1;
                o.RetryOnUnauthorized = false;
            });

            var helper = provider.GetRequiredService<IHttpHelper>();
            var result = await helper.GetAsync($"{server.BaseUrl}401");

            Assert.False(result.IsSuccess);
            Assert.Equal(1, attempts);
        }

        private static ServiceProvider BuildProvider(Action<HttpClientOptions> setup)
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddHttpClientService(o =>
            {
                o.AuditLog = false;
                o.EnableLogRedaction = false;
                o.FailThrowException = false;
                o.Timeout = 10;
                o.RetryDelaySeconds = 1;
                setup(o);
            });
            return services.BuildServiceProvider();
        }

        private sealed class SampleResponse
        {
            public int Id { get; set; }
            public string? Name { get; set; }
        }
    }
}

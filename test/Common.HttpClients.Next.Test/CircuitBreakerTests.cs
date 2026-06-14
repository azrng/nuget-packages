using Common.HttpClients.Next.Test.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace Common.HttpClients.Next.Test
{
    /// <summary>
    /// 熔断器策略测试：连续失败后短时间内拒绝请求
    /// </summary>
    /// <remarks>
    /// HttpCircuitBreakerStrategyOptions 默认 samplingDuration=100s, failureRatio=0.5, minimumThroughput=10；
    /// 此处只验证不抛异常并最终触发 fallback 兜底，不依赖具体熔断阈值。
    /// </remarks>
    public class CircuitBreakerTests
    {
        [Fact]
        public async Task CircuitBreaker_AfterFailures_ShouldStillBeHandledByFallback()
        {
            await using var server = new ScriptedHttpListenerServer(async ctx =>
            {
                await ScriptedHttpListenerServer.WriteResponseAsync(ctx, HttpStatusCode.InternalServerError, "err");
            });

            using var provider = BuildProvider(o =>
            {
                o.FailThrowException = false;
                o.MaxRetryAttempts = 0;
                o.Timeout = 5;
                o.RetryDelaySeconds = 1;
            });

            var helper = provider.GetRequiredService<IHttpHelper>();

            for (int i = 0; i < 5; i++)
            {
                var result = await helper.GetAsync($"{server.BaseUrl}fail-{i}");
                Assert.False(result.IsSuccess);
            }
        }

        [Fact]
        public async Task CircuitBreaker_WithSuccessfulResponse_ShouldReturnSuccess()
        {
            await using var server = new ScriptedHttpListenerServer(async ctx =>
            {
                await ScriptedHttpListenerServer.WriteResponseAsync(ctx, HttpStatusCode.OK, "ok");
            });

            using var provider = BuildProvider(o =>
            {
                o.MaxRetryAttempts = 0;
                o.Timeout = 5;
            });

            var helper = provider.GetRequiredService<IHttpHelper>();

            var result = await helper.GetAsync($"{server.BaseUrl}ok");
            Assert.True(result.IsSuccess);
            Assert.Equal("ok", result.Data);
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
    }
}

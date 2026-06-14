using Common.HttpClients.Next.Test.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace Common.HttpClients.Next.Test
{
    /// <summary>
    /// 并发限制策略测试：超限请求被 RateLimiter 拒绝；0 表示禁用限制
    /// </summary>
    public class ConcurrencyLimitTests
    {
        [Fact]
        public async Task ConcurrencyLimit_Zero_ShouldDisableLimitAndAllowParallelRequests()
        {
            // 回归：原 ConcurrencyLimit=0 抛异常，修复后表示禁用
            int concurrency = 0;
            int peak = 0;
            int handled = 0;
            await using var server = new ScriptedHttpListenerServer(async ctx =>
            {
                Interlocked.Increment(ref concurrency);
                int currentPeak = Interlocked.CompareExchange(ref peak, 0, 0);
                while (true)
                {
                    int observed = currentPeak;
                    if (concurrency <= observed) break;
                    if (Interlocked.CompareExchange(ref peak, concurrency, observed) == observed)
                    {
                        break;
                    }
                    currentPeak = Interlocked.CompareExchange(ref peak, 0, 0);
                }

                await Task.Delay(150).ConfigureAwait(false);
                Interlocked.Decrement(ref concurrency);
                Interlocked.Increment(ref handled);
                await ScriptedHttpListenerServer.WriteResponseAsync(ctx, HttpStatusCode.OK, "ok");
            });

            using var provider = BuildProvider(o =>
            {
                o.ConcurrencyLimit = 0;
                o.MaxRetryAttempts = 0;
                o.Timeout = 5;
            });

            var helper = provider.GetRequiredService<IHttpHelper>();
            var tasks = Enumerable.Range(0, 5)
                .Select(_ => helper.GetAsync($"{server.BaseUrl}burst"))
                .ToArray();
            await Task.WhenAll(tasks);

            Assert.Equal(5, handled);
            Assert.True(peak >= 3, $"Peak concurrency should exceed limit, got {peak}");
        }

        [Fact]
        public async Task ConcurrencyLimit_One_ShouldSerializeRequests()
        {
            int concurrency = 0;
            int peak = 0;
            await using var server = new ScriptedHttpListenerServer(async ctx =>
            {
                int current = Interlocked.Increment(ref concurrency);
                int observed;
                do
                {
                    observed = peak;
                    if (current <= observed) break;
                } while (Interlocked.CompareExchange(ref peak, current, observed) != observed);

                await Task.Delay(120).ConfigureAwait(false);
                Interlocked.Decrement(ref concurrency);
                await ScriptedHttpListenerServer.WriteResponseAsync(ctx, HttpStatusCode.OK, "ok");
            });

            using var provider = BuildProvider(o =>
            {
                o.ConcurrencyLimit = 1;
                o.MaxRetryAttempts = 0;
                o.Timeout = 10;
            });

            var helper = provider.GetRequiredService<IHttpHelper>();
            var tasks = Enumerable.Range(0, 3)
                .Select(_ => helper.GetAsync($"{server.BaseUrl}serial"))
                .ToArray();
            await Task.WhenAll(tasks);

            Assert.True(peak <= 1, $"Peak should be <=1 with ConcurrencyLimit=1, got {peak}");
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
                o.RetryDelaySeconds = 1;
                setup(o);
            });
            return services.BuildServiceProvider();
        }
    }
}

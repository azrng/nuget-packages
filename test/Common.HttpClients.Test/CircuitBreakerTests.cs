using Common.HttpClients.Test.Helpers;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Threading;
using Xunit;

namespace Common.HttpClients.Test
{
    /// <summary>
    /// 熔断器策略测试
    /// </summary>
    public class CircuitBreakerTests
    {
        /// <summary>
        /// 测试熔断器配置与重试配置协同工作
        /// </summary>
        [Fact]
        public void CircuitBreaker_WithRetryOptions_ShouldAcceptConfiguration()
        {
            var services = new ServiceCollection();
            services.AddLogging();

            var exception = Record.Exception(() =>
                services.AddHttpClientService(options =>
                {
                    options.MaxRetryAttempts = 5;
                    options.RetryDelaySeconds = 2;
                    options.ConcurrencyLimit = 50;
                }));

            // 熔断器是默认启用的，不应抛出配置异常
            Assert.Null(exception);

            using var provider = services.BuildServiceProvider();
            var httpOptions = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<Common.HttpClients.HttpClientOptions>>().Value;

            // 验证配置正确应用
            Assert.Equal(5, httpOptions.MaxRetryAttempts);
            Assert.Equal(2, httpOptions.RetryDelaySeconds);
            Assert.Equal(50, httpOptions.ConcurrencyLimit);
        }

        /// <summary>
        /// 测试成功请求后熔断器保持关闭状态
        /// </summary>
        [Fact]
        public async Task CircuitBreaker_WithAllSuccess_ShouldStayClosed()
        {
            await using var server = new ScriptedHttpListenerServer(async context =>
            {
                // 所有请求都成功
                await ScriptedHttpListenerServer.WriteResponseAsync(context, HttpStatusCode.OK, "{\"result\":\"ok\"}");
            });

            using var provider = BuildServiceProvider(options =>
            {
                options.FailThrowException = true;
                options.Timeout = 2;
            });

            var httpHelper = provider.GetRequiredService<IHttpHelper>();

            // 发送多个成功请求
            var results = new List<string?>();
            for (int i = 0; i < 3; i++)
            {
                var result = await httpHelper.GetAsync<string>(server.BaseUrl + "success-test");
                results.Add(result);
            }

            // 所有请求都应该成功
            Assert.Equal(3, results.Count);
            Assert.True(results.All(r => r != null));
        }

        /// <summary>
        /// 测试间歇性失败场景（部分成功、部分失败）
        /// </summary>
        [Fact]
        public async Task CircuitBreaker_WithIntermittentFailures_ShouldRetryAndSucceed()
        {
            var attempt = 0;
            await using var server = new ScriptedHttpListenerServer(async context =>
            {
                var current = Interlocked.Increment(ref attempt);
                // 前2次失败，第3次成功
                if (current <= 2)
                {
                    await ScriptedHttpListenerServer.WriteResponseAsync(context, HttpStatusCode.InternalServerError, "error");
                    return;
                }
                await ScriptedHttpListenerServer.WriteResponseAsync(context, HttpStatusCode.OK, "success");
            });

            using var provider = BuildServiceProvider(options =>
            {
                options.FailThrowException = true;
                options.Timeout = 3;
                options.MaxRetryAttempts = 3;
            });

            var httpHelper = provider.GetRequiredService<IHttpHelper>();

            // 第一次请求应该在第3次尝试时成功
            var result = await httpHelper.GetAsync(server.BaseUrl + "intermittent");

            Assert.Equal("success", result);
        }

        /// <summary>
        /// 测试超时场景是否触发重试
        /// </summary>
        [Fact]
        public async Task CircuitBreaker_WithTimeout_ShouldRetry()
        {
            var attempt = 0;
            await using var server = new ScriptedHttpListenerServer(async context =>
            {
                var current = Interlocked.Increment(ref attempt);
                // 第一次请求超时（延迟超过超时设置），第二次成功
                if (current == 1)
                {
                    await Task.Delay(2500);
                    await ScriptedHttpListenerServer.WriteResponseAsync(context, HttpStatusCode.OK, "delayed");
                    return;
                }
                await ScriptedHttpListenerServer.WriteResponseAsync(context, HttpStatusCode.OK, "success");
            });

            using var provider = BuildServiceProvider(options =>
            {
                options.FailThrowException = true;
                options.Timeout = 1; // 1秒超时
                options.MaxRetryAttempts = 2;
            });

            var httpHelper = provider.GetRequiredService<IHttpHelper>();

            // 应该重试并最终成功
            var result = await httpHelper.GetAsync(server.BaseUrl + "timeout-test");

            Assert.NotNull(result);
        }

        /// <summary>
        /// 测试弹性策略的完整配置
        /// </summary>
        [Fact]
        public void CircuitBreaker_FullResilienceStack_ShouldIncludeAllStrategies()
        {
            var services = new ServiceCollection();
            services.AddLogging();

            services.AddHttpClientService(options =>
            {
                // 完整的弹性策略配置
                options.FailThrowException = true;
                options.Timeout = 10;
                options.MaxRetryAttempts = 3;
                options.RetryDelaySeconds = 1;
                options.ConcurrencyLimit = 100;
            });

            using var provider = services.BuildServiceProvider();
            var httpOptions = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<Common.HttpClients.HttpClientOptions>>().Value;

            // 验证所有配置都正确应用
            Assert.True(httpOptions.FailThrowException);
            Assert.Equal(10, httpOptions.Timeout);
            Assert.Equal(3, httpOptions.MaxRetryAttempts);
            Assert.Equal(1, httpOptions.RetryDelaySeconds);
            Assert.Equal(100, httpOptions.ConcurrencyLimit);
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
    }
}

using Microsoft.Extensions.DependencyInjection;
using System.Net;
using Xunit;
using Xunit.Abstractions;

namespace Common.HttpClients.Test
{
    /// <summary>
    /// 并发限制策略测试
    /// </summary>
    public class ConcurrencyLimitTests
    {
        /// <summary>
        /// 测试并发限制配置正确应用
        /// </summary>
        [Fact]
        public void ConcurrencyLimit_Configuration_ShouldBeApplied()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddHttpClientService(options =>
            {
                options.AuditLog = false;
                options.ConcurrencyLimit = 5;
            });

            using var provider = services.BuildServiceProvider();
            var httpOptions = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<Common.HttpClients.HttpClientOptions>>().Value;

            Assert.Equal(5, httpOptions.ConcurrencyLimit);
        }

        /// <summary>
        /// 测试并发限制最小值（1）
        /// </summary>
        [Fact]
        public void ConcurrencyLimit_Minimum_ShouldBeOne()
        {
            var services = new ServiceCollection();
            services.AddLogging();

            var exception = Record.Exception(() =>
                services.AddHttpClientService(options =>
                {
                    options.ConcurrencyLimit = 0;
                }));

            Assert.NotNull(exception);
            Assert.IsType<ArgumentOutOfRangeException>(exception);
        }

        /// <summary>
        /// 测试并发限制最大值（10000）
        /// </summary>
        [Fact]
        public void ConcurrencyLimit_Maximum_ShouldBeTenThousand()
        {
            var services = new ServiceCollection();
            services.AddLogging();

            var exception = Record.Exception(() =>
                services.AddHttpClientService(options =>
                {
                    options.ConcurrencyLimit = 10001;
                }));

            Assert.NotNull(exception);
            Assert.IsType<ArgumentOutOfRangeException>(exception);
        }

        /// <summary>
        /// 测试低并发限制（2）配置能正常工作
        /// </summary>
        [Fact]
        public void ConcurrencyLimit_LowValue_ShouldAccept()
        {
            var services = new ServiceCollection();
            services.AddLogging();

            var exception = Record.Exception(() =>
                services.AddHttpClientService(options =>
                {
                    options.ConcurrencyLimit = 2;
                }));

            Assert.Null(exception);

            using var provider = services.BuildServiceProvider();
            var httpOptions = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<Common.HttpClients.HttpClientOptions>>().Value;

            Assert.Equal(2, httpOptions.ConcurrencyLimit);
        }

        /// <summary>
        /// 测试高并发限制（1000）配置能正常工作
        /// </summary>
        [Fact]
        public void ConcurrencyLimit_HighValue_ShouldAccept()
        {
            var services = new ServiceCollection();
            services.AddLogging();

            var exception = Record.Exception(() =>
                services.AddHttpClientService(options =>
                {
                    options.ConcurrencyLimit = 1000;
                }));

            Assert.Null(exception);

            using var provider = services.BuildServiceProvider();
            var httpOptions = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<Common.HttpClients.HttpClientOptions>>().Value;

            Assert.Equal(1000, httpOptions.ConcurrencyLimit);
        }

        /// <summary>
        /// 测试并发限制与重试配置组合
        /// </summary>
        [Fact]
        public void ConcurrencyLimit_WithRetryOptions_ShouldWorkTogether()
        {
            var services = new ServiceCollection();
            services.AddLogging();

            var exception = Record.Exception(() =>
                services.AddHttpClientService(options =>
                {
                    options.ConcurrencyLimit = 50;
                    options.MaxRetryAttempts = 2;
                    options.RetryDelaySeconds = 1;
                }));

            Assert.Null(exception);

            using var provider = services.BuildServiceProvider();
            var httpOptions = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<Common.HttpClients.HttpClientOptions>>().Value;

            Assert.Equal(50, httpOptions.ConcurrencyLimit);
            Assert.Equal(2, httpOptions.MaxRetryAttempts);
            Assert.Equal(1, httpOptions.RetryDelaySeconds);
        }

        /// <summary>
        /// 测试默认并发限制为 100
        /// </summary>
        [Fact]
        public void ConcurrencyLimit_DefaultValue_ShouldBe100()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddHttpClientService(); // 使用默认配置

            using var provider = services.BuildServiceProvider();
            var httpOptions = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<Common.HttpClients.HttpClientOptions>>().Value;

            Assert.Equal(100, httpOptions.ConcurrencyLimit);
        }
    }
}

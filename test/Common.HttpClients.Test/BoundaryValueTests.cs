using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Common.HttpClients.Test
{
    /// <summary>
    /// 配置参数边界值测试
    /// </summary>
    public class BoundaryValueTests
    {
        /// <summary>
        /// 测试最小超时值 (1秒)
        /// </summary>
        [Fact]
        public void Timeout_MinValue_ShouldAccept()
        {
            var services = new ServiceCollection();
            services.AddLogging();

            var exception = Record.Exception(() =>
                services.AddHttpClientService(options =>
                {
                    options.Timeout = 1;
                }));

            Assert.Null(exception);
        }

        /// <summary>
        /// 测试最大超时值 (3600秒)
        /// </summary>
        [Fact]
        public void Timeout_MaxValue_ShouldAccept()
        {
            var services = new ServiceCollection();
            services.AddLogging();

            var exception = Record.Exception(() =>
                services.AddHttpClientService(options =>
                {
                    options.Timeout = 3600;
                }));

            Assert.Null(exception);
        }

        /// <summary>
        /// 测试最小重试次数 (0)
        /// </summary>
        [Fact]
        public void MaxRetryAttempts_MinValue_ShouldAccept()
        {
            var services = new ServiceCollection();
            services.AddLogging();

            var exception = Record.Exception(() =>
                services.AddHttpClientService(options =>
                {
                    options.MaxRetryAttempts = 0;
                }));

            Assert.Null(exception);
        }

        /// <summary>
        /// 测试最大重试次数 (10)
        /// </summary>
        [Fact]
        public void MaxRetryAttempts_MaxValue_ShouldAccept()
        {
            var services = new ServiceCollection();
            services.AddLogging();

            var exception = Record.Exception(() =>
                services.AddHttpClientService(options =>
                {
                    options.MaxRetryAttempts = 10;
                }));

            Assert.Null(exception);
        }

        /// <summary>
        /// 测试最小并发限制 (1)
        /// </summary>
        [Fact]
        public void ConcurrencyLimit_MinValue_ShouldAccept()
        {
            var services = new ServiceCollection();
            services.AddLogging();

            var exception = Record.Exception(() =>
                services.AddHttpClientService(options =>
                {
                    options.ConcurrencyLimit = 1;
                }));

            Assert.Null(exception);
        }

        /// <summary>
        /// 测试最大并发限制 (10000)
        /// </summary>
        [Fact]
        public void ConcurrencyLimit_MaxValue_ShouldAccept()
        {
            var services = new ServiceCollection();
            services.AddLogging();

            var exception = Record.Exception(() =>
                services.AddHttpClientService(options =>
                {
                    options.ConcurrencyLimit = 10000;
                }));

            Assert.Null(exception);
        }

        /// <summary>
        /// 测试最小重试延迟 (1秒)
        /// </summary>
        [Fact]
        public void RetryDelaySeconds_MinValue_ShouldAccept()
        {
            var services = new ServiceCollection();
            services.AddLogging();

            var exception = Record.Exception(() =>
                services.AddHttpClientService(options =>
                {
                    options.RetryDelaySeconds = 1;
                }));

            Assert.Null(exception);
        }

        /// <summary>
        /// 测试最大重试延迟 (300秒)
        /// </summary>
        [Fact]
        public void RetryDelaySeconds_MaxValue_ShouldAccept()
        {
            var services = new ServiceCollection();
            services.AddLogging();

            var exception = Record.Exception(() =>
                services.AddHttpClientService(options =>
                {
                    options.RetryDelaySeconds = 300;
                }));

            Assert.Null(exception);
        }

        /// <summary>
        /// 测试MaxOutputResponseLength最小值 (0)
        /// </summary>
        [Fact]
        public void MaxOutputResponseLength_MinValue_ShouldAccept()
        {
            var services = new ServiceCollection();
            services.AddLogging();

            var exception = Record.Exception(() =>
                services.AddHttpClientService(options =>
                {
                    options.MaxOutputResponseLength = 0;
                }));

            Assert.Null(exception);
        }

        /// <summary>
        /// 测试所有边界值组合配置
        /// </summary>
        [Fact]
        public void AllBoundaryValues_Combined_ShouldAccept()
        {
            var services = new ServiceCollection();
            services.AddLogging();

            var exception = Record.Exception(() =>
                services.AddHttpClientService(options =>
                {
                    options.Timeout = 3600;
                    options.MaxRetryAttempts = 10;
                    options.RetryDelaySeconds = 300;
                    options.ConcurrencyLimit = 10000;
                    options.MaxOutputResponseLength = 0;
                }));

            Assert.Null(exception);
        }

        /// <summary>
        /// 测试零重试次数场景（不重试）
        /// </summary>
        [Fact]
        public void ZeroRetryAttempts_ShouldWorkCorrectly()
        {
            var services = new ServiceCollection();
            services.AddLogging();

            var exception = Record.Exception(() =>
                services.AddHttpClientService(options =>
                {
                    options.MaxRetryAttempts = 0;
                    options.RetryDelaySeconds = 1;
                }));

            Assert.Null(exception);

            using var provider = services.BuildServiceProvider();
            var httpOptions = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<Common.HttpClients.HttpClientOptions>>().Value;

            Assert.Equal(0, httpOptions.MaxRetryAttempts);
        }
    }
}

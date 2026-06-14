using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Common.HttpClients.Next.Test
{
    /// <summary>
    /// HttpClientOptions 边界值测试（基于 IValidateOptions）
    /// </summary>
    public class BoundaryValueTests
    {
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(3601)]
        public void Timeout_OutOfRange_ShouldFailValidation(int value)
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddHttpClientService(o => o.Timeout = value);

            using var provider = services.BuildServiceProvider();
            var monitor = provider.GetRequiredService<IOptionsMonitor<HttpClientOptions>>();

            Assert.Throws<OptionsValidationException>(() => monitor.Get("default"));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(3600)]
        public void Timeout_InRange_ShouldPass(int value)
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddHttpClientService(o => o.Timeout = value);

            using var provider = services.BuildServiceProvider();
            var monitor = provider.GetRequiredService<IOptionsMonitor<HttpClientOptions>>();

            Assert.Equal(value, monitor.Get("default").Timeout);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(10001)]
        public void ConcurrencyLimit_OutOfRange_ShouldFail(int value)
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddHttpClientService(o => o.ConcurrencyLimit = value);

            using var provider = services.BuildServiceProvider();
            var monitor = provider.GetRequiredService<IOptionsMonitor<HttpClientOptions>>();

            Assert.Throws<OptionsValidationException>(() => monitor.Get("default"));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(10000)]
        public void ConcurrencyLimit_InRange_ShouldPass(int value)
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddHttpClientService(o => o.ConcurrencyLimit = value);

            using var provider = services.BuildServiceProvider();
            var monitor = provider.GetRequiredService<IOptionsMonitor<HttpClientOptions>>();

            Assert.Equal(value, monitor.Get("default").ConcurrencyLimit);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(11)]
        public void MaxRetryAttempts_OutOfRange_ShouldFail(int value)
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddHttpClientService(o => o.MaxRetryAttempts = value);

            using var provider = services.BuildServiceProvider();
            var monitor = provider.GetRequiredService<IOptionsMonitor<HttpClientOptions>>();

            Assert.Throws<OptionsValidationException>(() => monitor.Get("default"));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(10)]
        public void MaxRetryAttempts_InRange_ShouldPass(int value)
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddHttpClientService(o => o.MaxRetryAttempts = value);

            using var provider = services.BuildServiceProvider();
            var monitor = provider.GetRequiredService<IOptionsMonitor<HttpClientOptions>>();

            Assert.Equal(value, monitor.Get("default").MaxRetryAttempts);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(301)]
        public void RetryDelaySeconds_OutOfRange_ShouldFail(int value)
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddHttpClientService(o => o.RetryDelaySeconds = value);

            using var provider = services.BuildServiceProvider();
            var monitor = provider.GetRequiredService<IOptionsMonitor<HttpClientOptions>>();

            Assert.Throws<OptionsValidationException>(() => monitor.Get("default"));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(300)]
        public void RetryDelaySeconds_InRange_ShouldPass(int value)
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddHttpClientService(o => o.RetryDelaySeconds = value);

            using var provider = services.BuildServiceProvider();
            var monitor = provider.GetRequiredService<IOptionsMonitor<HttpClientOptions>>();

            Assert.Equal(value, monitor.Get("default").RetryDelaySeconds);
        }

        [Theory]
        [InlineData(-1)]
        public void MaxOutputResponseLength_Negative_ShouldFail(int value)
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddHttpClientService(o =>
            {
                o.Timeout = 1;
                o.MaxOutputResponseLength = value;
            });

            using var provider = services.BuildServiceProvider();
            var monitor = provider.GetRequiredService<IOptionsMonitor<HttpClientOptions>>();

            Assert.Throws<OptionsValidationException>(() => monitor.Get("default"));
        }

        [Fact]
        public void MaxOutputResponseLength_Zero_ShouldMeanNoTruncation()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddHttpClientService(o =>
            {
                o.Timeout = 1;
                o.MaxOutputResponseLength = 0;
            });

            using var provider = services.BuildServiceProvider();
            var monitor = provider.GetRequiredService<IOptionsMonitor<HttpClientOptions>>();

            Assert.Equal(0, monitor.Get("default").MaxOutputResponseLength);
        }

        [Fact]
        public void CombinedBoundary_ShouldPass()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddHttpClientService(o =>
            {
                o.Timeout = 3600;
                o.MaxRetryAttempts = 10;
                o.RetryDelaySeconds = 300;
                o.ConcurrencyLimit = 0;
                o.MaxOutputResponseLength = 0;
                o.MaxRequestBodyLength = 0;
            });

            using var provider = services.BuildServiceProvider();
            var monitor = provider.GetRequiredService<IOptionsMonitor<HttpClientOptions>>();
            var options = monitor.Get("default");

            Assert.Equal(3600, options.Timeout);
            Assert.Equal(10, options.MaxRetryAttempts);
            Assert.Equal(300, options.RetryDelaySeconds);
            Assert.Equal(0, options.ConcurrencyLimit);
        }
    }
}

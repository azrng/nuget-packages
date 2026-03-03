using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Common.HttpClients.Test
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddHttpClientService_WithNullServices_ShouldThrow()
        {
            ServiceCollection? services = null;
            Assert.Throws<ArgumentNullException>(() => services.AddHttpClientService());
        }

        [Fact]
        public void AddHttpClientService_WithNullConfigure_ShouldThrow()
        {
            var services = new ServiceCollection();
            Assert.Throws<ArgumentNullException>(() => services.AddHttpClientService(null));
        }

        [Fact]
        public void AddHttpClientService_WithInvalidTimeout_ShouldThrow()
        {
            var services = new ServiceCollection();
            Assert.Throws<ArgumentException>(() =>
                services.AddHttpClientService(options =>
                {
                    options.Timeout = 0;
                }));
        }

        [Fact]
        public void AddHttpClientService_WithInvalidMaxOutputResponseLength_ShouldThrow()
        {
            var services = new ServiceCollection();
            Assert.Throws<ArgumentException>(() =>
                services.AddHttpClientService(options =>
                {
                    options.Timeout = 1;
                    options.MaxOutputResponseLength = -1;
                }));
        }

        [Fact]
        public void AddHttpClientService_DefaultOverload_ShouldEnableRedactionByDefault()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddHttpClientService();

            using var provider = services.BuildServiceProvider();
            var options = provider.GetRequiredService<IOptions<HttpClientOptions>>().Value;

            Assert.True(options.AuditLog);
            Assert.True(options.EnableLogRedaction);
            Assert.Equal(100, options.Timeout);
            Assert.False(options.FailThrowException);
        }

        [Fact]
        public void AddHttpClientService_CustomOptions_ShouldApplyConfiguredValues()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddHttpClientService(options =>
            {
                options.AuditLog = false;
                options.EnableLogRedaction = false;
                options.FailThrowException = true;
                options.Timeout = 5;
                options.MaxOutputResponseLength = 128;
            });

            using var provider = services.BuildServiceProvider();
            var options = provider.GetRequiredService<IOptions<HttpClientOptions>>().Value;

            Assert.False(options.AuditLog);
            Assert.False(options.EnableLogRedaction);
            Assert.True(options.FailThrowException);
            Assert.Equal(5, options.Timeout);
            Assert.Equal(128, options.MaxOutputResponseLength);
        }
    }
}

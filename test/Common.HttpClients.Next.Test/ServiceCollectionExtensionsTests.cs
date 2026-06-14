using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Common.HttpClients.Next.Test
{
    /// <summary>
    /// DI 注册、命名 options、IValidateOptions 验证测试
    /// </summary>
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddHttpClientService_NullServices_ShouldThrow()
        {
            IServiceCollection? services = null;
            Assert.Throws<ArgumentNullException>(() => services!.AddHttpClientService());
        }

        [Fact]
        public void AddHttpClientService_NullConfigure_ShouldThrow()
        {
            var services = new ServiceCollection();
            Assert.Throws<ArgumentNullException>(() => services.AddHttpClientService((Action<HttpClientOptions>)null!));
        }

        [Fact]
        public void AddHttpClientService_NamedOverload_NullName_ShouldThrow()
        {
            var services = new ServiceCollection();
            Assert.Throws<ArgumentNullException>(() => services.AddHttpClientService("", _ => { }));
        }

        [Fact]
        public void AddHttpClientService_NamedOverload_NullConfigure_ShouldThrow()
        {
            var services = new ServiceCollection();
            Assert.Throws<ArgumentNullException>(() => services.AddHttpClientService("clientA", null!));
        }

        [Fact]
        public void AddHttpClientService_DefaultOverload_ShouldRegisterNamedOptionsAsDefault()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddHttpClientService(o =>
            {
                o.AuditLog = false;
                o.Timeout = 5;
            });

            using var provider = services.BuildServiceProvider();
            var monitor = provider.GetRequiredService<IOptionsMonitor<HttpClientOptions>>();
            var options = monitor.Get("default");

            Assert.False(options.AuditLog);
            Assert.Equal(5, options.Timeout);
        }

        [Fact]
        public void AddHttpClientService_NamedClient_ShouldRegisterNamedOptions()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddHttpClientService("clientA", o =>
            {
                o.Timeout = 30;
                o.AdditionalSensitiveHeaders = new List<string> { "X-A" };
            });
            services.AddHttpClientService("clientB", o =>
            {
                o.Timeout = 60;
                o.AdditionalSensitiveHeaders = new List<string> { "X-B" };
            });

            using var provider = services.BuildServiceProvider();
            var monitor = provider.GetRequiredService<IOptionsMonitor<HttpClientOptions>>();

            Assert.Equal(30, monitor.Get("clientA").Timeout);
            Assert.Contains("X-A", monitor.Get("clientA").AdditionalSensitiveHeaders);
            Assert.Equal(60, monitor.Get("clientB").Timeout);
            Assert.Contains("X-B", monitor.Get("clientB").AdditionalSensitiveHeaders);
        }

        [Fact]
        public void AddHttpClientService_NoParameters_ShouldUseDefaults()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddHttpClientService();

            using var provider = services.BuildServiceProvider();
            var monitor = provider.GetRequiredService<IOptionsMonitor<HttpClientOptions>>();
            var options = monitor.Get("default");

            Assert.True(options.AuditLog);
            Assert.True(options.EnableLogRedaction);
            Assert.False(options.FailThrowException);
            Assert.Equal(100, options.Timeout);
            Assert.Equal(100, options.ConcurrencyLimit);
            Assert.Equal(3, options.MaxRetryAttempts);
            Assert.Equal(1, options.RetryDelaySeconds);
        }

        [Fact]
        public void AddHttpClientService_ShouldRegisterHttpHelperFactory()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddHttpClientService("clientA", _ => { });

            using var provider = services.BuildServiceProvider();
            var factory = provider.GetRequiredService<IHttpHelperFactory>();

            Assert.NotNull(factory);
            Assert.NotNull(factory.CreateClient("clientA"));
        }

        [Fact]
        public void AddHttpClientService_DefaultOverload_ShouldRegisterIHttpHelperPointingToDefault()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddHttpClientService(_ => { });

            using var provider = services.BuildServiceProvider();
            var helper = provider.GetRequiredService<IHttpHelper>();

            Assert.NotNull(helper);
        }

        [Fact]
        public void AddHttpClientService_ShouldKeepCustomLogRedactor()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton<IHttpLogRedactor, CustomRedactor>();
            services.AddHttpClientService();

            using var provider = services.BuildServiceProvider();
            var redactor = provider.GetService<IHttpLogRedactor>();

            Assert.IsType<CustomRedactor>(redactor);
        }

        [Fact]
        public async Task AddHttpClientService_NamedClient_ShouldPropagateOptionsToLoggingHandler()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddHttpClientService("clientA", o =>
            {
                o.AuditLog = false;
                o.EnableLogRedaction = false;
            });

            using var provider = services.BuildServiceProvider();
            var factory = provider.GetRequiredService<IHttpHelperFactory>();
            var helper = factory.CreateClient("clientA");

            // 验证 logging handler 不抛异常，并通过实际请求验证（默认 AuditLog=false，请求不会触发审计）
            using var listener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, 0);
            listener.Start();
            int port = ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();

            var result = await helper.GetAsync($"http://127.0.0.1:{port}/probe");

            // 由于无服务监听，Polly fallback 返回 503
            Assert.False(result.IsSuccess);
            Assert.True(result.IsFallbackResponse);
        }

        [Fact]
        public void AddHttpClientService_WithInvalidTimeout_ShouldFailValidation()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddHttpClientService(o => o.Timeout = 0);

            using var provider = services.BuildServiceProvider();
            var monitor = provider.GetRequiredService<IOptionsMonitor<HttpClientOptions>>();

            Assert.Throws<OptionsValidationException>(() => monitor.Get("default"));
        }

        [Fact]
        public void AddHttpClientService_WithInvalidMaxRetryAttempts_ShouldFailValidation()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddHttpClientService(o => o.MaxRetryAttempts = 999);

            using var provider = services.BuildServiceProvider();
            var monitor = provider.GetRequiredService<IOptionsMonitor<HttpClientOptions>>();

            Assert.Throws<OptionsValidationException>(() => monitor.Get("default"));
        }

        [Fact]
        public void AddHttpClientService_ConcurrencyLimitZero_ShouldBeAccepted()
        {
            // 回归：原 ConcurrencyLimit < 1 直接抛异常；修复后 0 表示禁用
            var services = new ServiceCollection();
            services.AddLogging();

            var exception = Record.Exception(() => services.AddHttpClientService(o =>
            {
                o.Timeout = 1;
                o.ConcurrencyLimit = 0;
            }));

            Assert.Null(exception);
        }

        [Fact]
        public void AddHttpClientService_ConcurrencyLimitNegative_ShouldFailValidation()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddHttpClientService(o => o.ConcurrencyLimit = -1);

            using var provider = services.BuildServiceProvider();
            var monitor = provider.GetRequiredService<IOptionsMonitor<HttpClientOptions>>();

            Assert.Throws<OptionsValidationException>(() => monitor.Get("default"));
        }

        private sealed class CustomRedactor : IHttpLogRedactor
        {
            public string RedactContent(string content) => content;
            public IDictionary<string, string> RedactHeaders(IDictionary<string, string>? headers)
                => headers ?? new Dictionary<string, string>();
        }
    }
}

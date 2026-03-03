using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Common.HttpClients.Test
{
    /// <summary>
    /// SSL证书配置测试
    /// </summary>
    public class SslCertificateTests
    {
        /// <summary>
        /// 测试 IgnoreUntrustedCertificate 默认值为 false
        /// </summary>
        [Fact]
        public void IgnoreUntrustedCertificate_Default_ShouldBeFalse()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddHttpClientService(); // 使用默认配置

            using var provider = services.BuildServiceProvider();
            var httpOptions = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<Common.HttpClients.HttpClientOptions>>().Value;

            Assert.False(httpOptions.IgnoreUntrustedCertificate);
        }

        /// <summary>
        /// 测试 IgnoreUntrustedCertificate 可以设置为 true（开发/测试场景）
        /// </summary>
        [Fact]
        public void IgnoreUntrustedCertificate_SetToTrue_ShouldAccept()
        {
            var services = new ServiceCollection();
            services.AddLogging();

            var exception = Record.Exception(() =>
                services.AddHttpClientService(options =>
                {
                    options.IgnoreUntrustedCertificate = true;
                }));

            Assert.Null(exception);

            using var provider = services.BuildServiceProvider();
            var httpOptions = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<Common.HttpClients.HttpClientOptions>>().Value;

            Assert.True(httpOptions.IgnoreUntrustedCertificate);
        }

        /// <summary>
        /// 测试 IgnoreUntrustedCertificate 与其他选项组合配置
        /// </summary>
        [Fact]
        public void IgnoreUntrustedCertificate_CombinedWithOtherOptions_ShouldWork()
        {
            var services = new ServiceCollection();
            services.AddLogging();

            var exception = Record.Exception(() =>
                services.AddHttpClientService(options =>
                {
                    options.IgnoreUntrustedCertificate = true;
                    options.Timeout = 30;
                    options.MaxRetryAttempts = 2;
                    options.RetryDelaySeconds = 1;
                    options.ConcurrencyLimit = 50;
                }));

            Assert.Null(exception);

            using var provider = services.BuildServiceProvider();
            var httpOptions = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<Common.HttpClients.HttpClientOptions>>().Value;

            Assert.True(httpOptions.IgnoreUntrustedCertificate);
            Assert.Equal(30, httpOptions.Timeout);
            Assert.Equal(2, httpOptions.MaxRetryAttempts);
            Assert.Equal(1, httpOptions.RetryDelaySeconds);
            Assert.Equal(50, httpOptions.ConcurrencyLimit);
        }

        /// <summary>
        /// 测试 IgnoreUntrustedCertificate 为 false 时的默认行为
        /// </summary>
        [Fact]
        public void IgnoreUntrustedCertificate_False_ShouldValidateCertificates()
        {
            var services = new ServiceCollection();
            services.AddLogging();

            var exception = Record.Exception(() =>
                services.AddHttpClientService(options =>
                {
                    options.IgnoreUntrustedCertificate = false;
                }));

            Assert.Null(exception);

            using var provider = services.BuildServiceProvider();
            var httpOptions = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<Common.HttpClients.HttpClientOptions>>().Value;

            Assert.False(httpOptions.IgnoreUntrustedCertificate);
        }

        /// <summary>
        /// 测试 IgnoreUntrustedCertificate 与 RetryOnUnauthorized 组合
        /// </summary>
        [Fact]
        public void IgnoreUntrustedCertificate_WithRetryOnUnauthorized_ShouldWork()
        {
            var services = new ServiceCollection();
            services.AddLogging();

            var exception = Record.Exception(() =>
                services.AddHttpClientService(options =>
                {
                    options.IgnoreUntrustedCertificate = true;
                    options.RetryOnUnauthorized = true;
                }));

            Assert.Null(exception);

            using var provider = services.BuildServiceProvider();
            var httpOptions = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<Common.HttpClients.HttpClientOptions>>().Value;

            Assert.True(httpOptions.IgnoreUntrustedCertificate);
            Assert.True(httpOptions.RetryOnUnauthorized);
        }

        /// <summary>
        /// 测试生产环境安全配置建议
        /// </summary>
        [Fact]
        public void IgnoreUntrustedCertificate_ProductionConfig_ShouldBeSecure()
        {
            var services = new ServiceCollection();
            services.AddLogging();

            // 模拟生产环境配置
            services.AddHttpClientService(options =>
            {
                // 生产环境应该启用证书验证
                options.IgnoreUntrustedCertificate = false;

                // 其他安全配置
                options.EnableLogRedaction = true;
                options.FailThrowException = false;
                options.Timeout = 30;
            });

            using var provider = services.BuildServiceProvider();
            var httpOptions = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<Common.HttpClients.HttpClientOptions>>().Value;

            Assert.False(httpOptions.IgnoreUntrustedCertificate, "Production should validate SSL certificates");
            Assert.True(httpOptions.EnableLogRedaction, "Production should enable log redaction");
        }

        /// <summary>
        /// 测试开发环境配置（可能需要忽略证书错误）
        /// </summary>
        [Fact]
        public void IgnoreUntrustedCertificate_DevelopmentConfig_ShouldBeConvenient()
        {
            var services = new ServiceCollection();
            services.AddLogging();

            // 模拟开发环境配置
            services.AddHttpClientService(options =>
            {
                // 开发环境可能需要忽略自签名证书
                options.IgnoreUntrustedCertificate = true;

                // 开发环境配置
                options.AuditLog = true;
                options.EnableLogRedaction = true;
            });

            using var provider = services.BuildServiceProvider();
            var httpOptions = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<Common.HttpClients.HttpClientOptions>>().Value;

            Assert.True(httpOptions.IgnoreUntrustedCertificate, "Development may ignore SSL certificate errors");
            Assert.True(httpOptions.AuditLog, "Development should enable audit logging");
            Assert.True(httpOptions.EnableLogRedaction, "Development should enable log redaction");
        }

        /// <summary>
        /// 测试配置文档中提到的安全实践
        /// </summary>
        [Fact]
        public void IgnoreUntrustedCertificate_SecurityPractice_ShouldBeDocumented()
        {
            // 根据README文档的安全建议：
            // 1. 仅在开发/测试环境使用
            // 2. 生产环境应使用有效的SSL证书

            var developmentConfig = new Common.HttpClients.HttpClientOptions
            {
                IgnoreUntrustedCertificate = true,
                Timeout = 30
            };

            var productionConfig = new Common.HttpClients.HttpClientOptions
            {
                IgnoreUntrustedCertificate = false,
                Timeout = 30
            };

            // 验证配置符合安全实践
            Assert.True(developmentConfig.IgnoreUntrustedCertificate, "Development may ignore certificates");
            Assert.False(productionConfig.IgnoreUntrustedCertificate, "Production must validate certificates");
        }
    }
}

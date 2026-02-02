using Azrng.AspNetCore.Job.Quartz;
using Azrng.AspNetCore.Job.Quartz.Options;
using Azrng.AspNetCore.Job.Quartz.Services;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Azrng.AspNetCore.Job.Quartz.Test
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddQuartzService_ShouldRegisterAllRequiredServices()
        {
            var services = new ServiceCollection();
            services.AddQuartzService();
            var provider = services.BuildServiceProvider();

            Assert.NotNull(provider);
            provider.Dispose();
        }

        [Fact]
        public void AddQuartzService_ShouldRegisterJobHistoryService()
        {
            var services = new ServiceCollection();
            services.AddLogging(); // 添加日志支持
            services.AddQuartzService();
            var provider = services.BuildServiceProvider();

            var historyService = provider.GetService<IJobExecutionHistoryService>();
            Assert.NotNull(historyService);
            provider.Dispose();
        }

        [Fact]
        public void AddQuartzService_DefaultOptions_ShouldUseDefaultValues()
        {
            var services = new ServiceCollection();
            services.AddLogging(); // 添加日志支持
            services.AddQuartzService();
            var provider = services.BuildServiceProvider();

            var options = provider.GetService<QuartzOptions>();
            Assert.NotNull(options);
            Assert.True(options.EnableJobHistory);
            Assert.Empty(options.AssemblyNamesToScan);
            provider.Dispose();
        }
    }
}

using Azrng.AspNetCore.Job.Quartz;
using Azrng.AspNetCore.Job.Quartz.Listeners;
using Azrng.AspNetCore.Job.Quartz.Options;
using Azrng.AspNetCore.Job.Quartz.Schedules;
using Azrng.AspNetCore.Job.Quartz.Services;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
            using var provider = services.BuildServiceProvider();
            provider.Should().NotBeNull();
        }

        [Fact]
        public void AddQuartzService_ShouldRegisterJobHistoryService()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddQuartzService();
            using var provider = services.BuildServiceProvider();

            provider.GetService<IJobExecutionHistoryService>().Should().NotBeNull();
        }

        [Fact]
        public void AddQuartzService_DefaultOptions_ShouldUseDefaultValues()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddQuartzService();
            using var provider = services.BuildServiceProvider();

            var options = provider.GetRequiredService<QuartzOptions>();
            options.EnableJobHistory.Should().BeTrue();
            options.AssemblyNamesToScan.Should().BeEmpty();
        }

        [Fact]
        public void AddQuartzService_ShouldRegisterJobListener()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddQuartzService();
            using var provider = services.BuildServiceProvider();

            provider.GetService<QuartzJobListener>().Should().NotBeNull();
        }

        [Fact]
        public void AddQuartzService_ShouldRegisterHistoryCleanupHostedService()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddQuartzService();
            using var provider = services.BuildServiceProvider();

            provider.GetServices<IHostedService>()
                .OfType<JobHistoryCleanupHostedService>()
                .Should().NotBeEmpty();
        }

        [Fact]
        public void AddQuartzService_DefaultOptions_ShouldIncludeSchedulerNameAndRetention()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddQuartzService();
            using var provider = services.BuildServiceProvider();

            var options = provider.GetRequiredService<QuartzOptions>();
            options.SchedulerName.Should().Be("QuartzScheduler");
            options.JobHistoryRetentionDays.Should().Be(30);
        }
    }
}

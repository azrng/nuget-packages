using Azrng.AspNetCore.Job.Quartz.Schedules;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Impl.Triggers;
using Quartz.Spi;
using Xunit;

namespace Azrng.AspNetCore.Job.Quartz.Test
{
    /// <summary>
    /// DependencyInjectionJobFactory 单元测试，重点验证 ReturnJob 释放 scope（修复内存泄漏）
    /// </summary>
    public class DependencyInjectionJobFactoryTests
    {
        private static TriggerFiredBundle BuildBundle<T>() where T : IJob
        {
            var jobDetail = JobBuilder.Create<T>().WithIdentity("j", "g").StoreDurably().Build();
            var trigger = new SimpleTriggerImpl("t", "g");
            return new TriggerFiredBundle(
                jobDetail, trigger, null, false,
                DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, null, null);
        }

        [Fact]
        public void NewJob_ShouldResolveJobFromScope()
        {
            var services = new ServiceCollection();
            services.AddScoped<JobWithDisposable>();
            services.AddScoped<DisposableDependency>();
            using var provider = services.BuildServiceProvider();

            var factory = new DependencyInjectionJobFactory(provider);
            var job = factory.NewJob(BuildBundle<JobWithDisposable>(), null!);

            job.Should().NotBeNull().And.BeOfType<JobWithDisposable>();
        }

        [Fact]
        public void ReturnJob_ShouldDisposeCorrespondingScope()
        {
            var services = new ServiceCollection();
            services.AddScoped<JobWithDisposable>();
            services.AddScoped<DisposableDependency>();
            using var provider = services.BuildServiceProvider();

            var factory = new DependencyInjectionJobFactory(provider);
            var job = (JobWithDisposable)factory.NewJob(BuildBundle<JobWithDisposable>(), null!)!;
            var dependency = job.Dep;
            dependency.Disposed.Should().BeFalse();

            factory.ReturnJob(job);

            // ReturnJob 必须释放对应 DI scope，从而释放其中的 Scoped 服务
            dependency.Disposed.Should().BeTrue("ReturnJob 应释放对应 DI scope");
        }

        [Fact]
        public void ReturnJob_WithUnknownJob_ShouldNotThrow()
        {
            var services = new ServiceCollection();
            using var provider = services.BuildServiceProvider();
            var factory = new DependencyInjectionJobFactory(provider);

            // 未通过 NewJob 创建的 job 不在字典中，ReturnJob 应安全无异常
            var act = () => factory.ReturnJob(new UnknownJob());
            act.Should().NotThrow();
        }

        private sealed class DisposableDependency : IDisposable
        {
            public bool Disposed { get; private set; }
            public void Dispose() => Disposed = true;
        }

        private sealed class JobWithDisposable : IJob
        {
            public DisposableDependency Dep { get; }
            public JobWithDisposable(DisposableDependency dep) => Dep = dep;
            public Task Execute(IJobExecutionContext context) => Task.CompletedTask;
        }

        private sealed class UnknownJob : IJob
        {
            public Task Execute(IJobExecutionContext context) => Task.CompletedTask;
        }
    }
}

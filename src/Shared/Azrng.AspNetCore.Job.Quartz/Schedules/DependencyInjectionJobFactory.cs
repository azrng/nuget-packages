using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Spi;

namespace Azrng.AspNetCore.Job.Quartz.Schedules
{
    internal class DependencyInjectionJobFactory : IJobFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public DependencyInjectionJobFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            using var scope = _serviceProvider.CreateScope();
            return scope.ServiceProvider.GetRequiredService(bundle.JobDetail.JobType) as IJob;
        }

        public void ReturnJob(IJob job)
        {
            (job as IDisposable)?.Dispose();
        }
    }
}
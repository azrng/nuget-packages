using Microsoft.Extensions.Hosting;
using Quartz;
using Quartz.Spi;
using System.Reflection;

namespace Azrng.AspNetCore.Job.Quartz.Schedules
{
    public class JobHostedService : IHostedService
    {
        private readonly IScheduler _scheduler;

        public JobHostedService(ISchedulerFactory schedulerFactory, IJobFactory jobFactory)
        {
            _scheduler = schedulerFactory.GetScheduler().GetAwaiter().GetResult();
            _scheduler.JobFactory = jobFactory;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _scheduler.Start(cancellationToken);
            var jobTypes = Assembly.GetEntryAssembly()!.DefinedTypes.Where(type => type.IsClass && typeof(IJob).IsAssignableFrom(type))
                                   .ToList();

            await Task.WhenAll(jobTypes.Select(async jobType =>
            {
                var customAttribute = jobType.GetCustomAttribute<JobConfigAttribute>();
                if (customAttribute != null)
                {
                    var jobDetail = JobBuilder.Create(jobType.AsType())
                                              .WithIdentity(customAttribute.Name, customAttribute.Group)
                                              .Build();

                    var triggerBuilder = TriggerBuilder.Create();
                    if (string.IsNullOrEmpty(customAttribute.CornExpression))
                    {
                        triggerBuilder.WithSimpleSchedule();
                    }
                    else
                    {
                        triggerBuilder.WithCronSchedule(customAttribute.CornExpression);
                    }

                    var trigger = triggerBuilder.Build();

                    await _scheduler.ScheduleJob(jobDetail, trigger, cancellationToken);
                }
            }));
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _scheduler.Shutdown(cancellationToken);
        }
    }
}
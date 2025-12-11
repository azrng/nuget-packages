using Azrng.AspNetCore.Job.Quartz.Schedules;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using System.Reflection;

namespace Azrng.AspNetCore.Job.Quartz
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 添加Quartz服务
        /// </summary>
        /// <param name="services"></param>
        /// <remarks>
        /// 自动获取入口欧程序集下的所有IJob实现类
        /// </remarks>
        public static IServiceCollection AddQuartzService(this IServiceCollection services)
        {
            foreach (var item in Assembly.GetEntryAssembly()!.DefinedTypes.Where(type =>
                         type.IsClass && typeof(IJob).IsAssignableFrom(type)))
            {
                services.AddScoped(item.AsType());
            }

            services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();
            services.AddSingleton<IJobFactory, DependencyInjectionJobFactory>();
            services.AddHostedService<JobHostedService>();

            services.AddScoped<IJobService, JobService>();
            services.AddScoped<ITriggerService, TriggerService>();
            services.AddScoped<ISchedulerService, SchedulerService>();

            return services;
        }
    }
}
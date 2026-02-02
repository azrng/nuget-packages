using Azrng.AspNetCore.Job.Quartz.Options;
using Azrng.AspNetCore.Job.Quartz.Schedules;
using Azrng.AspNetCore.Job.Quartz.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using System.Reflection;

namespace Azrng.AspNetCore.Job.Quartz
{
    /// <summary>
    /// Quartz服务注册扩展
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 添加Quartz服务
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <param name="configure">配置委托</param>
        /// <param name="assemblies">要扫描的程序集（可选，默认自动扫描入口程序集和调用程序集）</param>
        /// <returns></returns>
        /// <remarks>
        /// 从指定程序集或自动从入口程序集扫描并注册IJob实现类
        /// </remarks>
        public static IServiceCollection AddQuartzService(
            this IServiceCollection services,
            Action<QuartzOptions>? configure = null,
            params Assembly[] assemblies)
        {
            // 配置选项
            if (configure != null)
            {
                services.Configure(configure);
            }

            services.AddSingleton(resolver =>
                resolver.GetRequiredService<IOptions<QuartzOptions>>().Value);

            // 注册作业历史服务
            services.AddSingleton<IJobExecutionHistoryService, InMemoryJobExecutionHistoryService>();

            // 注册作业实现类（Scoped生命周期）
            RegisterJobs(services, assemblies);

            // 注册Quartz核心服务
            services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();
            services.AddSingleton<IJobFactory, DependencyInjectionJobFactory>();
            services.AddHostedService<JobHostedService>();

            // 注册应用服务
            services.AddScoped<IJobService, JobService>();
            services.AddScoped<ITriggerService, TriggerService>();
            services.AddScoped<ISchedulerService, SchedulerService>();
            services.AddScoped<IJobStatusService, JobStatusService>();

            return services;
        }

        /// <summary>
        /// 注册作业实现类
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <param name="assemblies">要扫描的程序集，为空时自动扫描入口程序集和调用程序集</param>
        private static void RegisterJobs(IServiceCollection services, params Assembly[] assemblies)
        {
            // 1. 如果指定了程序集，从指定程序集注册
            if (assemblies != null && assemblies.Length > 0)
            {
                foreach (var assembly in assemblies)
                {
                    RegisterJobsFromAssembly(services, assembly);
                }
                return;
            }

            // 2. 否则自动从入口程序集和调用程序集注册
            var entryAssembly = Assembly.GetEntryAssembly();
            if (entryAssembly != null)
            {
                RegisterJobsFromAssembly(services, entryAssembly);
            }

            var callingAssembly = Assembly.GetCallingAssembly();
            if (callingAssembly != null && callingAssembly != entryAssembly)
            {
                RegisterJobsFromAssembly(services, callingAssembly);
            }
        }

        /// <summary>
        /// 从指定程序集注册作业
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <param name="assembly">程序集</param>
        private static void RegisterJobsFromAssembly(IServiceCollection services, Assembly assembly)
        {
            foreach (var type in assembly.DefinedTypes.Where(t =>
                t.IsClass && typeof(IJob).IsAssignableFrom(t)))
            {
                services.AddScoped(type.AsType());
            }
        }
    }
}

using Azrng.AspNetCore.Job.Quartz.Listeners;
using Azrng.AspNetCore.Job.Quartz.Options;
using Azrng.AspNetCore.Job.Quartz.Schedules;
using Azrng.AspNetCore.Job.Quartz.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using System.Collections.Specialized;
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
            // 构造一份 options 实例用于注册阶段的程序集解析（与运行时 IOptions 配置保持一致）
            var options = new QuartzOptions();
            configure?.Invoke(options);

            if (configure != null)
            {
                services.Configure(configure);
            }

            services.AddSingleton(resolver =>
                resolver.GetRequiredService<IOptions<QuartzOptions>>().Value);

            // 注册作业历史服务
            services.AddSingleton<IJobExecutionHistoryService, InMemoryJobExecutionHistoryService>();

            // 注册作业监听器（调度器启动时挂载到 ListenerManager，使历史记录与详细日志配置生效）
            services.AddSingleton<QuartzJobListener>();

            // 注册作业实现类（Scoped生命周期），与调度扫描共用同一程序集解析逻辑
            var entryAssembly = Assembly.GetEntryAssembly();
            var callingAssembly = Assembly.GetCallingAssembly();
            RegisterJobs(services, options, assemblies, entryAssembly, callingAssembly);

            // 注册Quartz核心服务，支持自定义调度器名称
            services.AddSingleton<ISchedulerFactory>(resolver =>
            {
                var opt = resolver.GetRequiredService<IOptions<QuartzOptions>>().Value;
                var props = new NameValueCollection();
                if (!string.IsNullOrWhiteSpace(opt.SchedulerName))
                {
                    props[StdSchedulerFactory.PropertySchedulerInstanceName] = opt.SchedulerName;
                }
                return new StdSchedulerFactory(props);
            });
            services.AddSingleton<IJobFactory, DependencyInjectionJobFactory>();
            services.AddHostedService<JobHostedService>();
            services.AddHostedService<JobHistoryCleanupHostedService>();

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
        private static void RegisterJobs(
            IServiceCollection services,
            QuartzOptions options,
            Assembly[] explicitAssemblies,
            Assembly? entryAssembly,
            Assembly? callingAssembly)
        {
            var assembliesToScan = AssemblyResolver.Resolve(
                options,
                explicitAssemblies.Length > 0 ? explicitAssemblies : null,
                entryAssembly,
                callingAssembly,
                logger: null);

            foreach (var assembly in assembliesToScan)
            {
                RegisterJobsFromAssembly(services, assembly);
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
                t.IsClass && !t.IsAbstract && typeof(IJob).IsAssignableFrom(t)))
            {
                services.AddScoped(type.AsType());
            }
        }
    }
}

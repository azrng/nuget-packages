using Azrng.AspNetCore.Job.Quartz.Listeners;
using Azrng.AspNetCore.Job.Quartz.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using Quartz.Spi;
using System.Reflection;

namespace Azrng.AspNetCore.Job.Quartz.Schedules
{
    /// <summary>
    /// Quartz作业托管服务
    /// </summary>
    public class JobHostedService : IHostedService
    {
        private readonly ISchedulerFactory _schedulerFactory;
        private readonly IJobFactory _jobFactory;
        private readonly QuartzJobListener _jobListener;
        private readonly ILogger<JobHostedService> _logger;
        private readonly QuartzOptions _options;
        private IScheduler? _scheduler;

        public JobHostedService(
            ISchedulerFactory schedulerFactory,
            IJobFactory jobFactory,
            QuartzJobListener jobListener,
            ILogger<JobHostedService> logger,
            IOptions<QuartzOptions> options)
        {
            _schedulerFactory = schedulerFactory;
            _jobFactory = jobFactory;
            _jobListener = jobListener;
            _logger = logger;
            _options = options.Value;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _scheduler = await _schedulerFactory.GetScheduler(cancellationToken);
            _scheduler.JobFactory = _jobFactory;

            // 注册作业监听器，使历史记录与详细日志配置真正生效
            _scheduler.ListenerManager.AddJobListener(_jobListener);

            _logger.LogInformation("Quartz 调度器正在启动...");
            await _scheduler.Start(cancellationToken);
            _logger.LogInformation("Quartz 调度器启动成功 | 调度器名称: {SchedulerName}", _scheduler.SchedulerName);

            // 获取要扫描的程序集（与 DI 注册共用同一解析逻辑，确保范围一致）
            var entryAssembly = Assembly.GetEntryAssembly();
            var assembliesToScan = AssemblyResolver.Resolve(_options, null, entryAssembly, null, _logger);

            if (assembliesToScan.Count == 0)
            {
                _logger.LogWarning("未找到可扫描的程序集，跳过自动注册作业");
                return;
            }

            _logger.LogInformation("开始扫描 {Count} 个程序集", assembliesToScan.Count);

            var allJobTypes = new List<Type>();

            foreach (var assembly in assembliesToScan)
            {
                try
                {
                    var assemblyName = assembly.GetName().Name;
                    var jobTypes = assembly.DefinedTypes
                                           .Where(type => type.IsClass && !type.IsAbstract && typeof(IJob).IsAssignableFrom(type))
                                           .ToList();

                    allJobTypes.AddRange(jobTypes);
                    _logger.LogInformation("从程序集 {AssemblyName} 扫描到 {Count} 个作业类",
                        assemblyName, jobTypes.Count);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "扫描程序集 {AssemblyName} 时发生错误", assembly.GetName().Name);
                }
            }

            _logger.LogInformation("总共扫描到 {Count} 个作业类", allJobTypes.Count);

            var registeredJobs = new List<(string Name, string Group)>();

            await Task.WhenAll(allJobTypes.Select(async jobType =>
            {
                var customAttribute = jobType.GetCustomAttribute<JobConfigAttribute>();
                if (customAttribute != null)
                {
                    try
                    {
                        var jobDetail = JobBuilder.Create(jobType)
                                                  .WithIdentity(customAttribute.Name, customAttribute.Group)
                                                  .Build();

                        var triggerBuilder = TriggerBuilder.Create();
                        var hasCron = !string.IsNullOrEmpty(customAttribute.CronExpression);
                        if (hasCron)
                        {
                            triggerBuilder.WithCronSchedule(customAttribute.CronExpression);
                        }
                        else
                        {
                            // 未配置 Cron 时按简单调度执行一次
                            triggerBuilder.WithSimpleSchedule();
                        }

                        var trigger = triggerBuilder.Build();

                        await _scheduler.ScheduleJob(jobDetail, trigger, cancellationToken);

                        lock (registeredJobs)
                        {
                            registeredJobs.Add((customAttribute.Name, customAttribute.Group));
                        }

                        _logger.LogInformation("作业已自动注册 [{Group}].[{Name}] | Cron: {Cron} | 类型: {JobType}",
                            customAttribute.Group,
                            customAttribute.Name,
                            hasCron ? customAttribute.CronExpression : "(简单调度)",
                            jobType.FullName);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "注册作业 {JobType} 时发生错误", jobType.FullName);
                    }
                }
            }));

            _logger.LogInformation("自动注册完成 | 成功注册 {Count} 个定时任务", registeredJobs.Count);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Quartz 调度器正在关闭...");

            // 清理所有尚未回收的 scope
            if (_jobFactory is DependencyInjectionJobFactory diJobFactory)
            {
                diJobFactory.DisposeAllScopes();
                _logger.LogDebug("已清理所有作业scope");
            }

            if (_scheduler != null)
            {
                await _scheduler.Shutdown(cancellationToken);
            }
            _logger.LogInformation("Quartz 调度器已关闭");
        }
    }
}

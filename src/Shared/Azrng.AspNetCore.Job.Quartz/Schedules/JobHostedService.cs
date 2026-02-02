using Azrng.AspNetCore.Job.Quartz.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using Quartz.Spi;
using System.Reflection;

namespace Azrng.AspNetCore.Job.Quartz.Schedules
{
    public class JobHostedService : IHostedService
    {
        private readonly IScheduler _scheduler;
        private readonly ILogger<JobHostedService> _logger;
        private readonly QuartzOptions _options;

        public JobHostedService(
            ISchedulerFactory schedulerFactory,
            IJobFactory jobFactory,
            ILogger<JobHostedService> logger,
            IOptions<QuartzOptions> options)
        {
            _scheduler = schedulerFactory.GetScheduler().GetAwaiter().GetResult();
            _scheduler.JobFactory = jobFactory;
            _logger = logger;
            _options = options.Value;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Quartz 调度器正在启动...");
            await _scheduler.Start(cancellationToken);
            _logger.LogInformation("Quartz 调度器启动成功 | 调度器名称: {SchedulerName}", _scheduler.SchedulerName);

            // 获取要扫描的程序集
            var assembliesToScan = GetAssembliesToScan();

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
                    var jobTypes = assembly.DefinedTypes
                                           .Where(type => type.IsClass && !type.IsAbstract && typeof(IJob).IsAssignableFrom(type))
                                           .ToList();

                    allJobTypes.AddRange(jobTypes);
                    _logger.LogInformation("从程序集 {AssemblyName} 扫描到 {Count} 个作业类",
                        assembly.GetName().Name, jobTypes.Count);
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
                        if (string.IsNullOrEmpty(customAttribute.CronExpression))
                        {
                            triggerBuilder.WithSimpleSchedule();
                        }
                        else
                        {
                            triggerBuilder.WithCronSchedule(customAttribute.CronExpression);
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
                            customAttribute.CronExpression ?? "(简单调度)",
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
            await _scheduler.Shutdown(cancellationToken);
            _logger.LogInformation("Quartz 调度器已关闭");
        }

        /// <summary>
        /// 获取要扫描的程序集列表
        /// </summary>
        /// <returns></returns>
        private List<Assembly> GetAssembliesToScan()
        {
            var assemblies = new List<Assembly>();

            // 1. 如果配置了指定的程序集名称，则按名称加载
            if (_options.AssemblyNamesToScan.Any())
            {
                _logger.LogInformation("使用配置的程序集列表进行扫描: {Assemblies}",
                    string.Join(", ", _options.AssemblyNamesToScan));

                foreach (var assemblyName in _options.AssemblyNamesToScan)
                {
                    try
                    {
                        var assembly = Assembly.Load(assemblyName);
                        assemblies.Add(assembly);
                        _logger.LogDebug("添加配置程序集: {AssemblyName}", assemblyName);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "无法加载程序集 {AssemblyName}", assemblyName);
                    }
                }

                return assemblies.Distinct().ToList();
            }

            // 2. 如果启用了扫描所有已加载的程序集
            if (_options.ScanAllLoadedAssemblies)
            {
                _logger.LogWarning("启用了扫描所有已加载程序集选项，这可能包含系统程序集");
                assemblies.AddRange(AppDomain.CurrentDomain.GetAssemblies());
                return FilterExcludedAssemblies(assemblies);
            }

            // 3. 默认行为：添加入口程序集和调用程序集
            var entryAssembly = Assembly.GetEntryAssembly();
            if (entryAssembly != null)
            {
                assemblies.Add(entryAssembly);
                _logger.LogDebug("添加入口程序集: {AssemblyName}", entryAssembly.GetName().Name);
            }

            var callingAssembly = Assembly.GetCallingAssembly();
            if (callingAssembly != null && callingAssembly != entryAssembly)
            {
                assemblies.Add(callingAssembly);
                _logger.LogDebug("添加调用程序集: {AssemblyName}", callingAssembly.GetName().Name);
            }

            // 4. 应用排除规则
            return FilterExcludedAssemblies(assemblies);
        }

        /// <summary>
        /// 应用排除规则过滤程序集
        /// </summary>
        private List<Assembly> FilterExcludedAssemblies(List<Assembly> assemblies)
        {
            if (!_options.ExcludedAssemblyPatterns.Any())
            {
                return assemblies;
            }

            var filtered = assemblies.Where(assembly =>
                                     {
                                         var assemblyName = assembly.GetName().Name;
                                         foreach (var pattern in _options.ExcludedAssemblyPatterns)
                                         {
                                             if (SimpleWildcardMatch(assemblyName, pattern))
                                             {
                                                 _logger.LogDebug("排除程序集: {AssemblyName} (匹配模式: {Pattern})", assemblyName, pattern);
                                                 return false;
                                             }
                                         }

                                         return true;
                                     })
                                     .ToList();

            if (filtered.Count < assemblies.Count)
            {
                _logger.LogInformation("应用排除规则后剩余 {Count} 个程序集", filtered.Count);
            }

            return filtered;
        }

        /// <summary>
        /// 简单的通配符匹配
        /// </summary>
        private bool SimpleWildcardMatch(string input, string pattern)
        {
            // 转换通配符为正则表达式
            var regexPattern = "^" +
                               System.Text.RegularExpressions.Regex.Escape(pattern)
                                     .Replace("\\*", ".*")
                                     .Replace("\\?", ".") +
                               "$";

            return System.Text.RegularExpressions.Regex.IsMatch(input, regexPattern,
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }
    }
}
using Azrng.AspNetCore.Job.Quartz.Model;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Impl.Triggers;

namespace Azrng.AspNetCore.Job.Quartz
{
    public interface IJobService
    {
        /// <summary>
        /// 开启单次任务
        /// </summary>
        /// <param name="jobName"></param>
        /// <param name="startTime"></param>
        /// <param name="jobGroup"></param>
        /// <param name="jobDataMap"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task StartJobAsync<T>(string jobName, DateTime startTime, string jobGroup = "default",
                              IDictionary<string, object>? jobDataMap = null)
            where T : IJob;

        /// <summary>
        /// 开启定时任务
        /// </summary>
        /// <param name="jobName"></param>
        /// <param name="cronExpression"></param>
        /// <param name="jobGroup"></param>
        /// <param name="jobDataMap"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task StartCronJobAsync<T>(string jobName, string cronExpression, string jobGroup = "default",
                                  IDictionary<string, object>? jobDataMap = null)
            where T : IJob;

        /// <summary>
        /// 暂停一个job的调度。防止新的实例被触发
        /// </summary>
        /// <param name="jobName"></param>
        /// <param name="jobGroup"></param>
        /// <returns></returns>
        Task<bool> PauseJobAsync(string jobName, string jobGroup = "default");

        /// <summary>
        /// 启动任务(还没好封装好)
        /// </summary>
        /// <param name="jobName"></param>
        /// <param name="jobGroup"></param>
        /// <returns></returns>
        Task<bool> ResumeJobAsync(string jobName, string jobGroup = "default");

        /// <summary>
        /// 中断一个正在运行的job
        /// </summary>
        /// <param name="jobName"></param>
        /// <param name="jobGroup"></param>
        /// <returns></returns>
        Task<bool> InterruptJobAsync(string jobName, string jobGroup = "default");

        /// <summary>
        /// 立即执行Job
        /// </summary>
        /// <param name="jobName"></param>
        /// <param name="jobGroup"></param>
        /// <returns></returns>
        Task ExecuteJobAsync(string jobName, string jobGroup = "default");

        /// <summary>
        /// 删除任务
        /// </summary>
        /// <param name="jobName"></param>
        /// <param name="jobGroup"></param>
        /// <returns></returns>
        Task<bool> DeleteJobAsync(string jobName, string jobGroup = "default");
    }

    public class JobService : IJobService
    {
        private readonly ISchedulerFactory _schedulerFactory;
        private readonly ITriggerService _triggerService;
        private readonly ILogger<JobService> _logger;

        public JobService(ISchedulerFactory schedulerFactory, ITriggerService triggerService, ILogger<JobService> logger)
        {
            _schedulerFactory = schedulerFactory;
            _triggerService = triggerService;
            _logger = logger;
        }

        public async Task StartJobAsync<T>(string jobName, DateTime startTime, string jobGroup = "default",
                                           IDictionary<string, object>? jobDataMap = null)
            where T : IJob
        {
            var scheduler = await _schedulerFactory.GetScheduler();

            var jsonData = jobDataMap ?? new Dictionary<string, object>();
            var job = JobBuilder.Create<T>()
                                .WithIdentity(jobName, jobGroup)
                                .SetJobData(new JobDataMap(jsonData))
                                .Build();

            var trigger = TriggerBuilder.Create()
                                        .WithIdentity(jobName, jobGroup)
                                        .StartAt(startTime)
                                        .Build();

            await scheduler.ScheduleJob(job, trigger);
            _logger.LogInformation("单次任务创建成功 [{Group}].[{Name}] | 执行时间: {StartTime:yyyy-MM-dd HH:mm:ss} | 类型: {JobType}",
                jobGroup, jobName, startTime, typeof(T).Name);
        }

        public async Task StartCronJobAsync<T>(string jobName, string cronExpression, string jobGroup = "default",
                                               IDictionary<string, object>? jobDataMap = null)
            where T : IJob
        {
            var scheduler = await _schedulerFactory.GetScheduler();

            var jsonData = jobDataMap ?? new Dictionary<string, object>();
            var job = JobBuilder.Create<T>()
                                .WithIdentity(jobName, jobGroup)
                                .SetJobData(new JobDataMap(jsonData))
                                .Build();

            var trigger = TriggerBuilder.Create()
                                        .WithIdentity(jobName, jobGroup)
                                        .WithCronSchedule(cronExpression)
                                        .Build();

            await scheduler.ScheduleJob(job, trigger);
            _logger.LogInformation("定时任务创建成功 [{Group}].[{Name}] | Cron表达式: {CronExpression} | 类型: {JobType}",
                jobGroup, jobName, cronExpression, typeof(T).Name);
        }

        public async Task<bool> PauseJobAsync(string jobName, string jobGroup = "default")
        {
            var scheduler = await _schedulerFactory.GetScheduler();
            var jobKey = new JobKey(jobName, jobGroup);

            // 暂停触发器
            var triggers = await scheduler.GetTriggersOfJob(jobKey);
            var pausedCount = 0;
            foreach (var trigger in triggers)
            {
                if (trigger is CronTriggerImpl)
                {
                    await scheduler.PauseTrigger(trigger.Key);
                    pausedCount++;
                }
            }

            _logger.LogInformation("任务已暂停 [{Group}].[{Name}] | 暂停触发器数量: {Count}",
                jobGroup, jobName, pausedCount);

            return true;
        }

        public async Task<bool> ResumeJobAsync(string jobName, string jobGroup = "default")
        {
            var scheduler = await _schedulerFactory.GetScheduler();

            // 检查任务是否存在
            var jobKey = new JobKey(jobName, jobGroup);
            if (!await scheduler.CheckExists(jobKey))
            {
                _logger.LogWarning("尝试恢复不存在的任务 [{Group}].[{Name}]", jobGroup, jobName);
                throw new ArgumentException("任务不存在", nameof(jobName));
            }

            // 获取当前任务的触发器
            var triggers = await scheduler.GetTriggersOfJob(jobKey);
            var resumedCount = 0;
            foreach (var trigger in triggers)
            {
                if (trigger is CronTriggerImpl)
                {
                    await scheduler.ResumeTrigger(trigger.Key);
                    resumedCount++;
                }
            }

            _logger.LogInformation("任务已恢复 [{Group}].[{Name}] | 恢复触发器数量: {Count}",
                jobGroup, jobName, resumedCount);

            return true;
        }

        public async Task<bool> InterruptJobAsync(string jobName, string jobGroup = "default")
        {
            var scheduler = await _schedulerFactory.GetScheduler();
            var jobKey = new JobKey(jobName, jobGroup);

            var isExist = await scheduler.CheckExists(jobKey);
            if (!isExist)
            {
                _logger.LogWarning("尝试中断不存在的任务 [{Group}].[{Name}]", jobGroup, jobName);
                throw new ArgumentException($"不存在该任务{jobKey}");
            }

            var result = await scheduler.Interrupt(jobKey);
            _logger.LogInformation("任务已中断 [{Group}].[{Name}] | 中断结果: {Result}",
                jobGroup, jobName, result);

            return result;
        }

        public async Task ExecuteJobAsync(string jobName, string jobGroup = "default")
        {
            var scheduler = await _schedulerFactory.GetScheduler();
            var jobKey = new JobKey(jobName, jobGroup);

            if (!await scheduler.CheckExists(jobKey))
            {
                _logger.LogWarning("尝试立即执行不存在的任务 [{Group}].[{Name}]", jobGroup, jobName);
                throw new ArgumentException($"不存在该任务{jobKey}");
            }

            await scheduler.TriggerJob(jobKey);
            _logger.LogInformation("任务已触发立即执行 [{Group}].[{Name}]", jobGroup, jobName);
        }

        public async Task<bool> DeleteJobAsync(string jobName, string jobGroup = "default")
        {
            var scheduler = await _schedulerFactory.GetScheduler();
            var jobKey = new JobKey(jobName, jobGroup);

            if (!await scheduler.CheckExists(jobKey))
            {
                _logger.LogWarning("尝试删除不存在的任务 [{Group}].[{Name}]", jobGroup, jobName);
                throw new ArgumentException($"不存在该任务{jobKey}");
            }

            // 获取当前任务的触发器
            var triggers = await scheduler.GetTriggersOfJob(jobKey);
            var unscheduledCount = 0;
            foreach (var trigger in triggers)
            {
                if (trigger is CronTriggerImpl)
                {
                    await scheduler.PauseTrigger(trigger.Key);
                    await scheduler.UnscheduleJob(trigger.Key);
                    unscheduledCount++;
                }
            }

            var result = await scheduler.DeleteJob(jobKey);
            _logger.LogInformation("任务已删除 [{Group}].[{Name}] | 移除触发器数量: {Count} | 删除结果: {Result}",
                jobGroup, jobName, unscheduledCount, result);

            return result;
        }

        private ITrigger GetTrigger(ScheduleViewModel entity)
        {
            var trigger = entity.TriggerType switch
            {
                TriggerTypeEnum.Cron => _triggerService.CreateCronTrigger(entity),
                TriggerTypeEnum.Simple => _triggerService.CreateSimpleTrigger(entity),
                _ => _triggerService.CreateStarNowTrigger(entity)
            };
            return trigger;
        }
    }
}
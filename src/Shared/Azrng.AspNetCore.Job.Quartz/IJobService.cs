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
                              IDictionary<string, object> jobDataMap = null)
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
                                  IDictionary<string, object> jobDataMap = null)
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
        /// <param name="jobGroup"></param>
        /// <param name="jobName"></param>
        /// <returns></returns>
        Task<bool> InterruptJobAsync(string jobGroup, string jobName = "default");

        /// <summary>
        /// 立即执行Job
        /// </summary>
        Task ExecuteJobAsync(string jobGroup, string jobName = "default");

        /// <summary>
        /// 删除任务
        /// </summary>
        /// <param name="jobGroup"></param>
        /// <param name="jobName"></param>
        /// <returns></returns>
        Task<bool> DeleteJobAsync(string jobGroup, string jobName = "default");
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
                                           IDictionary<string, object> jobDataMap = null)
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
            _logger.LogDebug("任务[{Group}]:{Name}创建成功", jobGroup, jobName);
        }

        public async Task StartCronJobAsync<T>(string jobName, string cronExpression, string jobGroup = "default",
                                               IDictionary<string, object> jobDataMap = null)
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
        }

        public async Task<bool> PauseJobAsync(string jobName, string jobGroup = "default")
        {
            var scheduler = await _schedulerFactory.GetScheduler();

            var jobKey = new JobKey(jobName, jobGroup);

            // 暂停触发器
            var triggers = await scheduler.GetTriggersOfJob(jobKey);
            foreach (var trigger in triggers)
            {
                if (trigger is CronTriggerImpl)
                    await scheduler.PauseTrigger(trigger.Key);
            }

            _logger.LogDebug("任务已暂停");

            return true;
        }

        public async Task<bool> ResumeJobAsync(string jobName, string jobGroup = "default")
        {
            var scheduler = await _schedulerFactory.GetScheduler();

            // 检查任务是否存在
            var jobKey = new JobKey(jobName, jobGroup);
            if (!await scheduler.CheckExists(jobKey))
            {
                throw new ArgumentException("任务不存在");
            }

            // 获取当前任务的触发器
            var triggers = await scheduler.GetTriggersOfJob(jobKey);
            foreach (var trigger in triggers)
            {
                if (trigger is CronTriggerImpl)
                    await scheduler.ResumeTrigger(trigger.Key);
            }

            _logger.LogDebug("任务已恢复");

            return true;
        }

        public async Task<bool> InterruptJobAsync(string jobGroup, string jobName)
        {
            var scheduler = await _schedulerFactory.GetScheduler();
            var jobKey = new JobKey(jobName, jobGroup);

            var isExist = await scheduler.CheckExists(jobKey);
            if (!isExist)
            {
                throw new ArgumentException($"不存在该任务{jobKey}");
            }

            return await scheduler.Interrupt(jobKey);
        }

        public async Task ExecuteJobAsync(string jobGroup, string jobName)
        {
            var scheduler = await _schedulerFactory.GetScheduler();

            var jobKey = new JobKey(jobName, jobGroup);
            if (!await scheduler.CheckExists(jobKey))
            {
                throw new ArgumentException($"不存在该任务{jobKey}");
            }

            await scheduler.TriggerJob(jobKey);
        }

        public async Task<bool> DeleteJobAsync(string jobGroup, string jobName)
        {
            var scheduler = await _schedulerFactory.GetScheduler();
            var jobKey = new JobKey(jobName, jobGroup);

            if (!await scheduler.CheckExists(jobKey))
            {
                throw new ArgumentException($"不存在该任务{jobKey}");
            }

            // 获取当前任务的触发器
            var triggers = await scheduler.GetTriggersOfJob(jobKey);
            foreach (var trigger in triggers)
            {
                if (trigger is CronTriggerImpl)
                {
                    await scheduler.PauseTrigger(trigger.Key);
                    await scheduler.UnscheduleJob(trigger.Key); // 移除触发器
                }
            }

            return await scheduler.DeleteJob(jobKey);
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
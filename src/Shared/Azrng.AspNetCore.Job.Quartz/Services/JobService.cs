using Microsoft.Extensions.Logging;
using Quartz;

namespace Azrng.AspNetCore.Job.Quartz.Services
{
    public class JobService : IJobService
    {
        private readonly ISchedulerFactory _schedulerFactory;
        private readonly ILogger<JobService> _logger;

        public JobService(ISchedulerFactory schedulerFactory, ILogger<JobService> logger)
        {
            _schedulerFactory = schedulerFactory;
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

            // 暂停该作业的所有触发器（对所有触发器类型生效，不再仅限 CronTrigger）
            await scheduler.PauseJob(jobKey);

            _logger.LogInformation("任务已暂停 [{Group}].[{Name}]", jobGroup, jobName);

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

            // 恢复该作业的所有触发器（对所有触发器类型生效）
            await scheduler.ResumeJob(jobKey);

            _logger.LogInformation("任务已恢复 [{Group}].[{Name}]", jobGroup, jobName);

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

            // DeleteJob 会自动级联删除关联的所有触发器
            var result = await scheduler.DeleteJob(jobKey);
            _logger.LogInformation("任务已删除 [{Group}].[{Name}] | 删除结果: {Result}",
                jobGroup, jobName, result);

            return result;
        }
    }
}

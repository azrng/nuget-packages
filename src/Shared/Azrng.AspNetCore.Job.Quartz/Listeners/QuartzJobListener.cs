using Azrng.AspNetCore.Job.Quartz.Model;
using Azrng.AspNetCore.Job.Quartz.Options;
using Azrng.AspNetCore.Job.Quartz.Services;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Azrng.AspNetCore.Job.Quartz.Listeners
{
    /// <summary>
    /// Quartz作业监听器
    /// </summary>
    public class QuartzJobListener : IJobListener
    {
        private readonly ILogger<QuartzJobListener> _logger;
        private readonly QuartzOptions _options;
        private readonly IJobExecutionHistoryService? _historyService;

        public QuartzJobListener(
            ILogger<QuartzJobListener> logger,
            QuartzOptions options,
            IJobExecutionHistoryService? historyService = null)
        {
            _logger = logger;
            _options = options;
            _historyService = historyService;
            Name = "QuartzJobListener";
        }

        public string Name { get; }

        public Task JobToBeExecuted(IJobExecutionContext context, CancellationToken cancellationToken = default)
        {
            var jobKey = context.JobDetail.Key;
            var fireTime = context.FireTimeUtc.ToLocalTime();

            if (_options.EnableDetailedLogging)
            {
                _logger.LogInformation("作业 [{JobGroup}].[{JobName}] 开始执行 | 计划时间: {FireTime:yyyy-MM-dd HH:mm:ss}",
                    jobKey.Group,
                    jobKey.Name,
                    fireTime);
            }

            return Task.CompletedTask;
        }

        public Task JobExecutionVetoed(IJobExecutionContext context, CancellationToken cancellationToken = default)
        {
            var jobKey = context.JobDetail.Key;

            _logger.LogWarning("作业 [{JobGroup}].[{JobName}] 执行被拒绝",
                jobKey.Group,
                jobKey.Name);

            return Task.CompletedTask;
        }

        public async Task JobWasExecuted(IJobExecutionContext context, JobExecutionException? jobException,
                                         CancellationToken cancellationToken = default)
        {
            var jobKey = context.JobDetail.Key;
            var triggerKey = context.Trigger?.Key;
            var fireTime = context.FireTimeUtc.ToLocalTime();
            var completedTime = DateTime.Now;
            var duration = completedTime - (fireTime.DateTime);

            var history = new JobExecutionHistory
                          {
                              ExecutionId = Guid.NewGuid().ToString(),
                              JobName = jobKey.Name,
                              JobGroup = jobKey.Group,
                              TriggerName = triggerKey?.Name ?? string.Empty,
                              TriggerGroup = triggerKey?.Group ?? string.Empty,
                              StartTime = fireTime.DateTime,
                              EndTime = completedTime,
                              Result = jobException == null ? JobExecutionResult.Success : JobExecutionResult.Failed,
                              ErrorMessage = jobException?.Message,
                              ExceptionStackTrace = jobException?.ToString()
                          };

            // 收集作业数据
            foreach (var key in context.MergedJobDataMap.Keys)
            {
                history.JobData[key] = context.MergedJobDataMap[key]!;
            }

            // 保存历史记录
            if (_options.EnableJobHistory && _historyService != null)
            {
                try
                {
                    await _historyService.AddHistoryAsync(history, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "保存作业执行历史失败");
                }
            }

            if (jobException == null)
            {
                if (_options.EnableDetailedLogging)
                {
                    _logger.LogInformation(
                        "作业 [{JobGroup}].[{JobName}] 执行成功 | 开始: {StartTime:yyyy-MM-dd HH:mm:ss} | 完成: {EndTime:yyyy-MM-dd HH:mm:ss} | 耗时: {Duration}ms",
                        jobKey.Group,
                        jobKey.Name,
                        fireTime.DateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        completedTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        (int)duration.TotalMilliseconds);
                }
            }
            else
            {
                _logger.LogError(jobException,
                    "作业 [{JobGroup}].[{JobName}] 执行失败 | 开始: {StartTime:yyyy-MM-dd HH:mm:ss} | 错误: {ErrorMessage}",
                    jobKey.Group,
                    jobKey.Name,
                    fireTime.DateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    jobException.Message);
            }
        }
    }
}
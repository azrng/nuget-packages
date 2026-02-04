using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Impl.Matchers;

namespace Azrng.AspNetCore.Job.Quartz.Services
{
    /// <summary>
    /// 作业状态服务实现
    /// </summary>
    public class JobStatusService : IJobStatusService
    {
        private readonly ISchedulerFactory _schedulerFactory;
        private readonly ILogger<JobStatusService> _logger;

        public JobStatusService(
            ISchedulerFactory schedulerFactory,
            ILogger<JobStatusService> logger)
        {
            _schedulerFactory = schedulerFactory;
            _logger = logger;
        }

        public async Task<List<RunningJobInfo>> GetRunningJobsAsync(CancellationToken cancellationToken = default)
        {
            var scheduler = await _schedulerFactory.GetScheduler(cancellationToken);
            var currentlyExecutingJobs = await scheduler.GetCurrentlyExecutingJobs(cancellationToken);

            var runningJobs = new List<RunningJobInfo>();
            var now = DateTime.Now;

            foreach (var jobContext in currentlyExecutingJobs)
            {
                var fireTimeUtc = jobContext.FireTimeUtc.ToLocalTime().DateTime;
                var runningTime = (long)(now - fireTimeUtc).TotalMilliseconds;

                runningJobs.Add(new RunningJobInfo
                                {
                                    JobName = jobContext.JobDetail.Key.Name,
                                    JobGroup = jobContext.JobDetail.Key.Group,
                                    TriggerName = jobContext.Trigger?.Key.Name ?? string.Empty,
                                    TriggerGroup = jobContext.Trigger?.Key.Group ?? string.Empty,
                                    FireTime = fireTimeUtc,
                                    RunningTime = runningTime
                                });
            }

            return runningJobs;
        }

        public async Task<List<ScheduledJobInfo>> GetScheduledJobsAsync(CancellationToken cancellationToken = default)
        {
            var scheduler = await _schedulerFactory.GetScheduler(cancellationToken);
            var jobKeys = await scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup(), cancellationToken);

            var scheduledJobs = new List<ScheduledJobInfo>();

            foreach (var jobKey in jobKeys)
            {
                var jobDetail = await scheduler.GetJobDetail(jobKey, cancellationToken);
                if (jobDetail == null)
                    continue;

                var triggers = await scheduler.GetTriggersOfJob(jobKey, cancellationToken);
                var jobInfo = new ScheduledJobInfo
                              {
                                  JobName = jobDetail.Key.Name,
                                  JobGroup = jobDetail.Key.Group,
                                  Description = jobDetail.Description ?? string.Empty,
                                  IsPaused =
                                      triggers.Any(t =>
                                          scheduler.GetTriggerState(t.Key, cancellationToken).GetAwaiter().GetResult() ==
                                          TriggerState.Paused),
                                  IsDurable = jobDetail.Durable,
                                  IsConcurrent = jobDetail.ConcurrentExecutionDisallowed == false,
                                  NextFireTime =
                                      triggers.Select(t => t.GetNextFireTimeUtc()?.ToLocalTime())
                                              .OrderBy(t => t)
                                              .FirstOrDefault()
                                              ?.DateTime,
                                  PreviousFireTime =
                                      triggers.Select(t => t.GetPreviousFireTimeUtc()?.ToLocalTime())
                                              .OrderByDescending(t => t)
                                              .FirstOrDefault()
                                              ?.DateTime,
                                  Triggers = new List<TriggerInfo>()
                              };

                foreach (var trigger in triggers)
                {
                    var state = await scheduler.GetTriggerState(trigger.Key, cancellationToken);
                    jobInfo.Triggers.Add(new TriggerInfo
                                         {
                                             TriggerName = trigger.Key.Name,
                                             TriggerGroup = trigger.Key.Group,
                                             TriggerType = trigger.GetType().Name,
                                             State = state,
                                             NextFireTime = trigger.GetNextFireTimeUtc()?.DateTime,
                                             PreviousFireTime = trigger.GetPreviousFireTimeUtc()?.DateTime,
                                             Priority = trigger.Priority,
                                             StartTime = trigger.StartTimeUtc.DateTime,
                                             EndTime = trigger.EndTimeUtc?.DateTime
                                         });
                }

                scheduledJobs.Add(jobInfo);
            }

            return scheduledJobs;
        }

        public async Task<bool> IsJobRunningAsync(
            string jobName,
            string jobGroup = "default",
            CancellationToken cancellationToken = default)
        {
            var runningJobs = await GetRunningJobsAsync(cancellationToken);
            return runningJobs.Any(j => j.JobName == jobName && j.JobGroup == jobGroup);
        }

        public async Task<DateTime?> GetNextFireTimeAsync(
            string jobName,
            string jobGroup = "default",
            CancellationToken cancellationToken = default)
        {
            var scheduler = await _schedulerFactory.GetScheduler(cancellationToken);
            var jobKey = new JobKey(jobName, jobGroup);

            if (!await scheduler.CheckExists(jobKey, cancellationToken))
                return null;

            var triggers = await scheduler.GetTriggersOfJob(jobKey, cancellationToken);
            var nextFireTimes = triggers
                                .Select(t => t.GetNextFireTimeUtc()?.ToLocalTime())
                                .Where(t => t.HasValue)
                                .Select(t => t!.Value)
                                .ToList();

            return nextFireTimes.Any() ? nextFireTimes.Min().DateTime : null;
        }

        public async Task<JobDetailInfo?> GetJobDetailAsync(
            string jobName,
            string jobGroup = "default",
            CancellationToken cancellationToken = default)
        {
            var scheduler = await _schedulerFactory.GetScheduler(cancellationToken);
            var jobKey = new JobKey(jobName, jobGroup);

            var jobDetail = await scheduler.GetJobDetail(jobKey, cancellationToken);
            if (jobDetail == null)
                return null;

            var triggers = await scheduler.GetTriggersOfJob(jobKey, cancellationToken);
            var triggerInfos = new List<TriggerInfo>();

            foreach (var trigger in triggers)
            {
                var state = await scheduler.GetTriggerState(trigger.Key, cancellationToken);
                triggerInfos.Add(new TriggerInfo
                                 {
                                     TriggerName = trigger.Key.Name,
                                     TriggerGroup = trigger.Key.Group,
                                     TriggerType = trigger.GetType().Name,
                                     State = state,
                                     NextFireTime = trigger.GetNextFireTimeUtc()?.DateTime,
                                     PreviousFireTime = trigger.GetPreviousFireTimeUtc()?.DateTime,
                                     Priority = trigger.Priority,
                                     StartTime = trigger.StartTimeUtc.DateTime,
                                     EndTime = trigger.EndTimeUtc?.DateTime
                                 });
            }

            var jobDataMap = new Dictionary<string, object>();
            if (jobDetail.JobDataMap != null)
            {
                foreach (var key in jobDetail.JobDataMap.Keys)
                {
                    jobDataMap[key] = jobDetail.JobDataMap[key] ?? string.Empty;
                }
            }

            return new JobDetailInfo
                   {
                       JobName = jobDetail.Key.Name,
                       JobGroup = jobDetail.Key.Group,
                       Description = jobDetail.Description ?? string.Empty,
                       JobType = jobDetail.JobType?.Name ?? string.Empty,
                       IsDurable = jobDetail.Durable,
                       IsRecoverable = jobDetail.RequestsRecovery,
                       IsConcurrent = jobDetail.ConcurrentExecutionDisallowed == false,
                       JobDataMap = jobDataMap,
                       Triggers = triggerInfos,
                       NextFireTime = triggerInfos.Select(t => t.NextFireTime).OrderBy(t => t).FirstOrDefault(),
                       PreviousFireTime = triggerInfos.Select(t => t.PreviousFireTime).OrderByDescending(t => t).FirstOrDefault(),
                       IsPaused = triggerInfos.Any(t => t.State == TriggerState.Paused)
                   };
        }
    }
}
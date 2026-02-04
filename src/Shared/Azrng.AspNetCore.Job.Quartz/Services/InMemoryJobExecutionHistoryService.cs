using Azrng.AspNetCore.Job.Quartz.Model;
using Azrng.AspNetCore.Job.Quartz.Options;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Azrng.AspNetCore.Job.Quartz.Services
{
    /// <summary>
    /// 内存实现的作业执行历史服务
    /// </summary>
    public class InMemoryJobExecutionHistoryService : IJobExecutionHistoryService
    {
        private readonly ConcurrentDictionary<string, JobExecutionHistory> _histories = new();
        private readonly ILogger<InMemoryJobExecutionHistoryService> _logger;

        public InMemoryJobExecutionHistoryService(ILogger<InMemoryJobExecutionHistoryService> logger)
        {
            _logger = logger;
        }

        public Task AddHistoryAsync(JobExecutionHistory history, CancellationToken cancellationToken = default)
        {
            _histories.TryAdd(history.ExecutionId, history);
            return Task.CompletedTask;
        }

        public Task<(List<JobExecutionHistory> Histories, int TotalCount)> GetHistoryAsync(
            string jobName,
            string jobGroup = "default",
            int pageIndex = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var query = _histories.Values
                .Where(h => h.JobName == jobName && h.JobGroup == jobGroup)
                .OrderByDescending(h => h.StartTime);

            var totalCount = query.Count();
            var histories = query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Task.FromResult((histories, totalCount));
        }

        public Task<List<JobExecutionHistory>> GetRecentHistoryAsync(
            int count = 50,
            CancellationToken cancellationToken = default)
        {
            var recent = _histories.Values
                .OrderByDescending(h => h.StartTime)
                .Take(count)
                .ToList();

            return Task.FromResult(recent);
        }

        public Task CleanupExpiredHistoryAsync(
            int retentionDays,
            CancellationToken cancellationToken = default)
        {
            var cutoffDate = DateTime.Now.AddDays(-retentionDays);
            var expiredKeys = _histories
                .Where(kvp => kvp.Value.StartTime < cutoffDate)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in expiredKeys)
            {
                _histories.TryRemove(key, out _);
            }

            _logger.LogInformation("清理了 {Count} 条过期历史记录", expiredKeys.Count);
            return Task.CompletedTask;
        }

        public Task<JobExecutionStatistics> GetStatisticsAsync(
            string jobName,
            string jobGroup = "default",
            CancellationToken cancellationToken = default)
        {
            var jobHistories = _histories.Values
                .Where(h => h.JobName == jobName && h.JobGroup == jobGroup)
                .ToList();

            var statistics = new JobExecutionStatistics
            {
                TotalExecutions = jobHistories.Count,
                SuccessCount = jobHistories.Count(h => h.Result == JobExecutionResult.Success),
                FailedCount = jobHistories.Count(h => h.Result == JobExecutionResult.Failed),
                LastExecutionTime = jobHistories.Any() ? jobHistories.Max(h => h.StartTime) : null,
                LastExecutionResult = jobHistories.Any() ? jobHistories.MaxBy(h => h.StartTime)?.Result : null
            };

            if (jobHistories.Any())
            {
                var executionTimes = jobHistories
                    .Where(h => h.EndTime.HasValue)
                    .Select(h => (h.EndTime!.Value - h.StartTime).TotalMilliseconds)
                    .ToList();

                statistics.AverageExecutionTime = executionTimes.Any()
                    ? executionTimes.Average()
                    : 0;
            }

            return Task.FromResult(statistics);
        }
    }
}

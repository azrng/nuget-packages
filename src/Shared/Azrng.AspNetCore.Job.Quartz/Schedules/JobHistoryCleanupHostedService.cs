using Azrng.AspNetCore.Job.Quartz.Options;
using Azrng.AspNetCore.Job.Quartz.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Azrng.AspNetCore.Job.Quartz.Schedules
{
    /// <summary>
    /// 作业执行历史周期清理服务，避免内存历史记录无界增长
    /// </summary>
    public class JobHistoryCleanupHostedService : IHostedService, IDisposable
    {
        private readonly IJobExecutionHistoryService _historyService;
        private readonly QuartzOptions _options;
        private readonly ILogger<JobHistoryCleanupHostedService> _logger;
        private Timer? _timer;

        // 首次延迟 1 小时，之后每 24 小时清理一次
        private static readonly TimeSpan InitialDelay = TimeSpan.FromHours(1);
        private static readonly TimeSpan Period = TimeSpan.FromHours(24);

        public JobHistoryCleanupHostedService(
            IJobExecutionHistoryService historyService,
            IOptions<QuartzOptions> options,
            ILogger<JobHistoryCleanupHostedService> logger)
        {
            _historyService = historyService;
            _options = options.Value;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(Cleanup, null, InitialDelay, Period);
            _logger.LogDebug("作业历史清理服务已启动 | 保留天数: {Days} | 清理周期: {Period}h",
                _options.JobHistoryRetentionDays, Period.TotalHours);
            return Task.CompletedTask;
        }

        private async void Cleanup(object? state)
        {
            try
            {
                await _historyService.CleanupExpiredHistoryAsync(_options.JobHistoryRetentionDays);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "清理作业执行历史时发生错误");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}

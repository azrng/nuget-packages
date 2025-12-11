using Microsoft.Extensions.Logging;
using Quartz;

namespace Azrng.AspNetCore.Job.Quartz
{
    public interface ISchedulerService
    {
        /// <summary>
        /// 启动任务调度
        /// </summary>
        Task StartAsync();

        /// <summary>
        /// 停止任务调度
        /// </summary>
        Task StopAsync();
    }

    public class SchedulerService : ISchedulerService
    {
        private readonly ISchedulerFactory _schedulerFactory;
        private readonly ILogger<SchedulerService> _logger;

        public SchedulerService(ISchedulerFactory schedulerFactory, ILogger<SchedulerService> logger)
        {
            _schedulerFactory = schedulerFactory;
            _logger = logger;
        }

        public async Task StartAsync()
        {
            var scheduler = await _schedulerFactory.GetScheduler();

            //开启调度器
            if (scheduler.InStandbyMode)
            {
                await scheduler.Start();
                _logger.LogInformation("任务调度启动！");
            }
        }

        public async Task StopAsync()
        {
            var scheduler = await _schedulerFactory.GetScheduler();

            //判断调度是否已经关闭
            if (!scheduler.InStandbyMode)
            {
                await scheduler.Standby();
                _logger.LogInformation("任务调度暂停！");
            }
        }
    }
}
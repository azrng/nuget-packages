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
}

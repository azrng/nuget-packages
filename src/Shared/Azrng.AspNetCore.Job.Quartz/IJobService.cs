using Quartz;

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
}

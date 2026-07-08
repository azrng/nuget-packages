using Azrng.AspNetCore.Job.Quartz.Model;

namespace Azrng.AspNetCore.Job.Quartz.Services
{
    /// <summary>
    /// 作业状态服务接口
    /// </summary>
    public interface IJobStatusService
    {
        /// <summary>
        /// 获取所有正在运行的作业
        /// </summary>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns></returns>
        Task<List<RunningJobInfo>> GetRunningJobsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// 获取所有已调度的作业
        /// </summary>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns></returns>
        Task<List<ScheduledJobInfo>> GetScheduledJobsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// 检查作业是否正在运行
        /// </summary>
        /// <param name="jobName">作业名称</param>
        /// <param name="jobGroup">作业分组</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns></returns>
        Task<bool> IsJobRunningAsync(
            string jobName,
            string jobGroup = "default",
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 获取作业下次执行时间
        /// </summary>
        /// <param name="jobName">作业名称</param>
        /// <param name="jobGroup">作业分组</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns></returns>
        Task<DateTime?> GetNextFireTimeAsync(
            string jobName,
            string jobGroup = "default",
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 获取作业详情
        /// </summary>
        /// <param name="jobName">作业名称</param>
        /// <param name="jobGroup">作业分组</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns></returns>
        Task<JobDetailInfo?> GetJobDetailAsync(
            string jobName,
            string jobGroup = "default",
            CancellationToken cancellationToken = default);
    }
}
using Azrng.AspNetCore.Job.Quartz.Model;
using Azrng.AspNetCore.Job.Quartz.Options;

namespace Azrng.AspNetCore.Job.Quartz.Services
{
    /// <summary>
    /// 作业执行历史服务接口
    /// </summary>
    public interface IJobExecutionHistoryService
    {
        /// <summary>
        /// 添加作业执行历史
        /// </summary>
        /// <param name="history">历史记录</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns></returns>
        Task AddHistoryAsync(JobExecutionHistory history, CancellationToken cancellationToken = default);

        /// <summary>
        /// 获取作业执行历史
        /// </summary>
        /// <param name="jobName">作业名称</param>
        /// <param name="jobGroup">作业分组</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">页大小</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns></returns>
        Task<(List<JobExecutionHistory> Histories, int TotalCount)> GetHistoryAsync(
            string jobName,
            string jobGroup = "default",
            int pageIndex = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 获取最近的执行历史
        /// </summary>
        /// <param name="count">记录数量</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns></returns>
        Task<List<JobExecutionHistory>> GetRecentHistoryAsync(
            int count = 50,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 清理过期的历史记录
        /// </summary>
        /// <param name="retentionDays">保留天数</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns></returns>
        Task CleanupExpiredHistoryAsync(
            int retentionDays,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 获取作业执行统计信息
        /// </summary>
        /// <param name="jobName">作业名称</param>
        /// <param name="jobGroup">作业分组</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns></returns>
        Task<JobExecutionStatistics> GetStatisticsAsync(
            string jobName,
            string jobGroup = "default",
            CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// 作业执行统计信息
    /// </summary>
    public class JobExecutionStatistics
    {
        /// <summary>
        /// 总执行次数
        /// </summary>
        public int TotalExecutions { get; set; }

        /// <summary>
        /// 成功次数
        /// </summary>
        public int SuccessCount { get; set; }

        /// <summary>
        /// 失败次数
        /// </summary>
        public int FailedCount { get; set; }

        /// <summary>
        /// 成功率
        /// </summary>
        public double SuccessRate => TotalExecutions > 0 ? (double)SuccessCount / TotalExecutions * 100 : 0;

        /// <summary>
        /// 平均执行时间（毫秒）
        /// </summary>
        public double AverageExecutionTime { get; set; }

        /// <summary>
        /// 最后执行时间
        /// </summary>
        public DateTime? LastExecutionTime { get; set; }

        /// <summary>
        /// 最后执行结果
        /// </summary>
        public JobExecutionResult? LastExecutionResult { get; set; }
    }
}

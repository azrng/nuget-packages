using Azrng.AspNetCore.Job.Quartz.Options;

namespace Azrng.AspNetCore.Job.Quartz.Model
{
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
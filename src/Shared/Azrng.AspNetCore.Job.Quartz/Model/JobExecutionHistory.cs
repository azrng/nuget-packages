using Azrng.AspNetCore.Job.Quartz.Options;

namespace Azrng.AspNetCore.Job.Quartz.Model
{
    /// <summary>
    /// 作业执行历史记录
    /// </summary>
    public class JobExecutionHistory
    {
        /// <summary>
        /// 执行ID
        /// </summary>
        public string ExecutionId { get; set; } = string.Empty;

        /// <summary>
        /// 作业名称
        /// </summary>
        public string JobName { get; set; } = string.Empty;

        /// <summary>
        /// 作业分组
        /// </summary>
        public string JobGroup { get; set; } = string.Empty;

        /// <summary>
        /// 触发器名称
        /// </summary>
        public string TriggerName { get; set; } = string.Empty;

        /// <summary>
        /// 触发器分组
        /// </summary>
        public string TriggerGroup { get; set; } = string.Empty;

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// 执行结果
        /// </summary>
        public JobExecutionResult Result { get; set; }

        /// <summary>
        /// 错误消息
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// 异常堆栈
        /// </summary>
        public string? ExceptionStackTrace { get; set; }

        /// <summary>
        /// 作业数据
        /// </summary>
        public Dictionary<string, object> JobData { get; set; } = new();
    }
}
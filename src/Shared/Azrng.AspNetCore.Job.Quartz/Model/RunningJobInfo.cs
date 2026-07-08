namespace Azrng.AspNetCore.Job.Quartz.Model
{
    /// <summary>
    /// 正在运行的作业信息
    /// </summary>
    public class RunningJobInfo
    {
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
        /// 触发时间
        /// </summary>
        public DateTime FireTime { get; set; }

        /// <summary>
        /// 运行时长（毫秒）
        /// </summary>
        public long RunningTime { get; set; }
    }
}
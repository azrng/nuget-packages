using Quartz;

namespace Azrng.AspNetCore.Job.Quartz.Model
{
    /// <summary>
    /// 触发器信息
    /// </summary>
    public class TriggerInfo
    {
        /// <summary>
        /// 触发器名称
        /// </summary>
        public string TriggerName { get; set; } = string.Empty;

        /// <summary>
        /// 触发器分组
        /// </summary>
        public string TriggerGroup { get; set; } = string.Empty;

        /// <summary>
        /// 触发器类型
        /// </summary>
        public string TriggerType { get; set; } = string.Empty;

        /// <summary>
        /// 触发器状态
        /// </summary>
        public TriggerState State { get; set; }

        /// <summary>
        /// 下次触发时间
        /// </summary>
        public DateTime? NextFireTime { get; set; }

        /// <summary>
        /// 上次触发时间
        /// </summary>
        public DateTime? PreviousFireTime { get; set; }

        /// <summary>
        /// 优先级
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? EndTime { get; set; }
    }
}
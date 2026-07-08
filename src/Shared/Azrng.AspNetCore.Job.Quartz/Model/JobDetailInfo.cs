namespace Azrng.AspNetCore.Job.Quartz.Model
{
    /// <summary>
    /// 作业详情信息
    /// </summary>
    public class JobDetailInfo
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
        /// 作业描述
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 作业类型
        /// </summary>
        public string JobType { get; set; } = string.Empty;

        /// <summary>
        /// 是否持久化
        /// </summary>
        public bool IsDurable { get; set; }

        /// <summary>
        /// 是否可恢复
        /// </summary>
        public bool IsRecoverable { get; set; }

        /// <summary>
        /// 是否并发执行
        /// </summary>
        public bool IsConcurrent { get; set; }

        /// <summary>
        /// 作业数据
        /// </summary>
        public Dictionary<string, object> JobDataMap { get; set; } = new();

        /// <summary>
        /// 触发器列表
        /// </summary>
        public List<TriggerInfo> Triggers { get; set; } = new();

        /// <summary>
        /// 下次执行时间
        /// </summary>
        public DateTime? NextFireTime { get; set; }

        /// <summary>
        /// 上次执行时间
        /// </summary>
        public DateTime? PreviousFireTime { get; set; }

        /// <summary>
        /// 是否暂停
        /// </summary>
        public bool IsPaused { get; set; }
    }
}
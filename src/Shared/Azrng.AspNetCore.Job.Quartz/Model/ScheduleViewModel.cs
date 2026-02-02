namespace Azrng.AspNetCore.Job.Quartz.Model
{
    public class ScheduleViewModel
    {
        /// <summary>
        /// 作业名称
        /// </summary>
        public string JobName { get; set; } = string.Empty;

        /// <summary>
        /// 任务分组
        /// </summary>
        public string JobGroup { get; set; } = "default";

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTimeOffset BeginTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTimeOffset? EndTime { get; set; }

        /// <summary>
        /// Cron表达式
        /// </summary>
        public string Cron { get; set; } = string.Empty;

        /// <summary>
        /// 执行次数（默认无限循环）
        /// </summary>
        public int? RunTimes { get; set; }

        /// <summary>
        /// 执行间隔时间，单位秒（如果有Cron，则IntervalSecond失效）
        /// </summary>
        public int? IntervalSecond { get; set; }

        /// <summary>
        /// 触发器类型
        /// </summary>
        public TriggerTypeEnum TriggerType { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 参数
        /// </summary>
        public IDictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();
    }

    public enum TriggerTypeEnum
    {
        None = 0,
        Cron = 1,
        Simple = 2,

        /// <summary>
        /// 现在执行一次
        /// </summary>
        StartNow = 3
    }
}
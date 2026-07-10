namespace Azrng.AspNetCore.Job.Quartz.Options
{
    /// <summary>
    /// Quartz调度配置选项
    /// </summary>
    public class QuartzOptions
    {
        /// <summary>
        /// 是否启用作业执行历史记录
        /// </summary>
        public bool EnableJobHistory { get; set; } = true;

        /// <summary>
        /// 是否记录详细的作业执行日志
        /// </summary>
        public bool EnableDetailedLogging { get; set; } = true;

        /// <summary>
        /// 调度器名称（对应 quartz.scheduler.name，默认与 Quartz 内置一致）
        /// </summary>
        public string SchedulerName { get; set; } = "QuartzScheduler";

        /// <summary>
        /// 作业执行历史保留天数，超过则被自动清理（由历史清理服务周期执行）
        /// </summary>
        public int JobHistoryRetentionDays { get; set; } = 30;

        /// <summary>
        /// 要扫描的程序集列表
        /// 如果为空，则自动扫描入口程序集和调用程序集
        /// </summary>
        public List<string> AssemblyNamesToScan { get; set; } = new();

        /// <summary>
        /// 排除的程序集名称模式列表（支持通配符）
        /// 例如：["System.*", "Microsoft.*", "Newtonsoft.*"]
        /// </summary>
        public List<string> ExcludedAssemblyPatterns { get; set; } = new();
    }

    /// <summary>
    /// 作业执行结果
    /// </summary>
    public enum JobExecutionResult
    {
        /// <summary>
        /// 成功
        /// </summary>
        Success = 0,

        /// <summary>
        /// 失败
        /// </summary>
        Failed = 1,

        /// <summary>
        /// 被中断
        /// </summary>
        Interrupted = 2
    }
}
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
        /// 要扫描的程序集列表
        /// 如果为空，则自动扫描入口程序集和调用程序集
        /// </summary>
        public List<string> AssemblyNamesToScan { get; set; } = new();

        /// <summary>
        /// 是否扫描所有已加载的程序集
        /// 警告：启用此选项会扫描所有已加载的程序集，包括系统程序集
        /// </summary>
        public bool ScanAllLoadedAssemblies { get; set; } = false;

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
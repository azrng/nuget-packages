using Quartz;

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

    /// <summary>
    /// 已调度的作业信息
    /// </summary>
    public class ScheduledJobInfo
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
        /// 是否暂停
        /// </summary>
        public bool IsPaused { get; set; }

        /// <summary>
        /// 是否可恢复
        /// </summary>
        public bool IsDurable { get; set; }

        /// <summary>
        /// 是否并发执行
        /// </summary>
        public bool IsConcurrent { get; set; }

        /// <summary>
        /// 下次执行时间
        /// </summary>
        public DateTime? NextFireTime { get; set; }

        /// <summary>
        /// 上次执行时间
        /// </summary>
        public DateTime? PreviousFireTime { get; set; }

        /// <summary>
        /// 触发器列表
        /// </summary>
        public List<TriggerInfo> Triggers { get; set; } = new();
    }

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

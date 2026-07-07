namespace Azrng.AspNetCore.Job.Quartz
{
    /// <summary>
    /// job特性配置
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class JobConfigAttribute : Attribute
    {
        /// <summary>
        /// 任务名称
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 任务所属分组
        /// </summary>
        public string Group { get; }

        /// <summary>
        /// 任务Cron表达式
        /// </summary>
        public string CronExpression { get; }

        /// <summary>
        /// 初始化不带 Cron 表达式的作业配置（按简单调度执行一次）
        /// </summary>
        /// <param name="name"></param>
        /// <param name="group"></param>
        public JobConfigAttribute(string name, string group)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("请检查定时任务名称", nameof(name));
            }
            if (string.IsNullOrWhiteSpace(group))
            {
                throw new ArgumentException("请检查定时任务分组", nameof(group));
            }

            Name = name;
            Group = group;
            CronExpression = string.Empty;
        }

        /// <summary>
        /// 初始化带 Cron 表达式的作业配置
        /// </summary>
        public JobConfigAttribute(string name, string group, string cronExpression)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("请检查定时任务名称", nameof(name));
            }
            if (string.IsNullOrWhiteSpace(group))
            {
                throw new ArgumentException("请检查定时任务分组", nameof(group));
            }

            if (string.IsNullOrWhiteSpace(cronExpression))
            {
                throw new ArgumentException("请检查定时任务Cron表达式", nameof(cronExpression));
            }
            Name = name;
            Group = group;
            CronExpression = cronExpression;
        }
    }
}
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
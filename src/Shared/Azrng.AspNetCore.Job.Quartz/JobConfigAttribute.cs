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
        /// 任务cron
        /// </summary>
        public string CornExpression { get; }

        public JobConfigAttribute(string name, string group, string cornExpression)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("请检查定时任务名称");
            }
            if (string.IsNullOrWhiteSpace(group))
            {
                throw new ArgumentException("请检查定时任务分组");
            }

            if (string.IsNullOrWhiteSpace(cornExpression))
            {
                throw new ArgumentException("请检查定时任务Corn表达式");
            }
            Name = name;
            Group = group;
            CornExpression = cornExpression;
        }
    }
}
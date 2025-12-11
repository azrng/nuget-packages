namespace Azrng.AspNetCore.Core.PreConfigure
{
    /// <summary>
    /// 将多个委托包装成一个对象
    /// </summary>
    /// <typeparam name="TOptions"></typeparam>
    public class PreConfigureActionList<TOptions> : List<Action<TOptions>>
    {
        /// <summary>
        /// 依次按顺序执行委托
        /// </summary>
        /// <param name="options"></param>
        public void Configure(TOptions options)
        {
            foreach (var action in this)
            {
                action(options);
            }
        }

        /// <summary>
        /// 反射获取单实例
        /// </summary>
        /// <returns></returns>
        public TOptions Configure()
        {
            var options = Activator.CreateInstance<TOptions>();
            Configure(options);
            return options;
        }
    }
}
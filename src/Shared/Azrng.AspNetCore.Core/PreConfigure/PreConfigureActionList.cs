using System.Diagnostics.CodeAnalysis;

namespace Azrng.AspNetCore.Core.PreConfigure
{
    /// <summary>
    /// 预配置动作列表，将多个配置委托按顺序执行
    /// 用于在正式配置 Options 之前进行预配置
    /// </summary>
    /// <typeparam name="TOptions">选项类型</typeparam>
    public class PreConfigureActionList<TOptions> : List<Action<TOptions>>
    {
        /// <summary>
        /// 按顺序执行所有配置委托
        /// </summary>
        /// <param name="options">要配置的选项对象</param>
        public void Configure(TOptions options)
        {
            foreach (var action in this)
            {
                action(options);
            }
        }

        /// <summary>
        /// 创建选项实例并执行所有预配置
        /// </summary>
        /// <returns>配置好的选项实例</returns>
        [RequiresUnreferencedCode("Calls System.Activator.CreateInstance<T>()")]
        public TOptions Configure()
        {
            var options = Activator.CreateInstance<TOptions>();
            Configure(options);
            return options;
        }
    }
}

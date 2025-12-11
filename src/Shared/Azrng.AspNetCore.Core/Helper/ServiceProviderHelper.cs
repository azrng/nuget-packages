using Microsoft.Extensions.DependencyInjection;

namespace Azrng.AspNetCore.Core.Helper
{
    public static class ServiceProviderHelper
    {
        /// <summary>
        /// 全局服务提供者
        /// </summary>
        public static IServiceProvider ServiceProvider { get; private set; } = null!;

        /// <summary>
        /// 初始化构建ServiceProvider对象
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void InitServiceProvider(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        /// <summary>
        /// 获取服务
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static T GetRequiredService<T>() where T : class
        {
            return ServiceProvider.GetRequiredService<T>();
        }
    }
}
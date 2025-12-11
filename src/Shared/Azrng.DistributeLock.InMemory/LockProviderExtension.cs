using Azrng.DistributeLock.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Azrng.DistributeLock.InMemory
{
    /// <summary>
    /// 分布式锁
    /// </summary>
    public static class LockProviderExtension
    {
        /// <summary>
        /// 添加内存锁服务
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddInMemory(this IServiceCollection services)
        {
            services.AddSingleton<ILockProvider, InMemoryLockProvider>();
            return services;
        }
    }
}
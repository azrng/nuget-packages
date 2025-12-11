using Azrng.DistributeLock.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Azrng.DistributeLock.Redis
{
    /// <summary>
    /// 分布式锁
    /// </summary>
    public static class LockProviderExtension
    {
        /// <summary>
        /// 添加Redis分布式锁服务
        /// </summary>
        /// <param name="services"></param>
        /// <param name="connectionString">Redis连接字符串</param>
        /// <param name="defaultExpireTime">默认过期时间（5s）</param>
        /// <returns></returns>
        public static IServiceCollection AddRedisLockProvider(this IServiceCollection services, string connectionString,
            TimeSpan? defaultExpireTime = null)
        {
            services.AddSingleton<ILockProvider, RedisLockProvider>();
            services.AddOptions().Configure<RedisLockOptions>(x =>
            {
                x.ConnectionString = connectionString;
                x.DefaultExpireTime = defaultExpireTime ?? TimeSpan.FromSeconds(5);
            });
            return services;
        }
    }
}
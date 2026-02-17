using Azrng.DistributeLock.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

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
            // 注册 RedisLockOptions
            services.AddOptions().Configure<RedisLockOptions>(x =>
            {
                x.ConnectionString = connectionString;
                x.DefaultExpireTime = defaultExpireTime ?? TimeSpan.FromSeconds(5);
            });

            // 注册 ConnectionMultiplexer 为单例
            services.AddSingleton(sp =>
            {
                var options = sp.GetRequiredService<IOptions<RedisLockOptions>>();
                var configurationOptions = ConfigurationOptions.Parse(options.Value.ConnectionString);
                return ConnectionMultiplexer.Connect(configurationOptions);
            });

            // 注册 ILockProvider
            services.AddSingleton<ILockProvider, RedisLockProvider>();

            return services;
        }
    }
}
using FreeRedis;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Azrng.Cache.FreeRedis
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 添加redis缓存配置
        /// </summary>
        /// <param name="services"></param>
        public static void AddRedisCacheService(this IServiceCollection services, Func<RedisConfig> func)
        {
            if (services is null)
                throw new ArgumentNullException(nameof(services));

            services.AddTransient<IRedisCache, RedisCache>();

            var config = func.Invoke();

            services.AddSingleton<RedisClient>(new RedisClient(config.ConnectionString));
        }
    }
}

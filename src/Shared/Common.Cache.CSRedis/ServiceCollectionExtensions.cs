using CSRedis;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Common.Cache.CSRedis
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 添加redis缓存配置
        /// </summary>
        /// <param name="services"></param>
        /// <param name="func"></param>
        public static void AddRedisCacheStore(this IServiceCollection services, Func<RedisConfig> func)
        {
            if (services is null)
                throw new ArgumentNullException(nameof(services));

            services.AddTransient<IRedisCache, RedisCache>();
            var config = func.Invoke();
            RedisHelper.Initialization(new CSRedisClient($"{config.ConnectionString},prefix={config.InstanceName}"));
        }

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
            RedisHelper.Initialization(new CSRedisClient($"{config.ConnectionString},prefix={config.InstanceName}"));
        }
    }
}
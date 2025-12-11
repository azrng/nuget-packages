using Azrng.Cache.Core;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Common.Cache.Redis
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 添加redis缓存配置
        /// </summary>
        /// <param name="services"></param>
        /// <param name="action"></param>
        public static IServiceCollection AddRedisCacheStore(this IServiceCollection services,
                                                            Action<RedisConfig> action = null)
        {
            if (services is null)
                throw new ArgumentNullException(nameof(services));

            services.Configure(action ?? (config => { }));

            services.AddScoped<ICacheProvider, RedisProvider>();
            services.AddSingleton<RedisManage>();
            services.AddSingleton<ICacheProvider, RedisProvider>();
            services.AddSingleton<IRedisProvider, RedisProvider>();
            return services;
        }

        /// <summary>
        /// 添加redis缓存配置
        /// </summary>
        /// <param name="services"></param>
        /// <param name="action"></param>
        [Obsolete]
        public static IServiceCollection AddRedisCacheService(this IServiceCollection services,
                                                              Action<RedisConfig> action)
        {
            if (services is null)
                throw new ArgumentNullException(nameof(services));
            if (action is null)
                throw new ArgumentNullException(nameof(action));

            services.Configure(action);

            services.AddScoped<ICacheProvider, RedisProvider>();
            services.AddSingleton<RedisManage>();
            services.AddSingleton<ICacheProvider, RedisProvider>();
            services.AddSingleton<IRedisProvider, RedisProvider>();
            return services;
        }
    }
}
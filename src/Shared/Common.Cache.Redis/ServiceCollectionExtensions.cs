using Azrng.Cache.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.Configure(action ?? (_ => { }));
            services.TryAddSingleton<RedisManage>();
            services.TryAddSingleton<IRedisProvider, RedisProvider>();
            services.TryAddSingleton<ICacheProvider>(sp => sp.GetRequiredService<IRedisProvider>());
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
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return services.AddRedisCacheStore(action);
        }
    }
}

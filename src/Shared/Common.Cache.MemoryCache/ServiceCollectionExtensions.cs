using Azrng.Cache.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace Azrng.Cache.MemoryCache
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 添加内存缓存
        /// </summary>
        /// <param name="services"></param>
        /// <param name="action"></param>
        public static IServiceCollection AddMemoryCacheStore(this IServiceCollection services,
                                                             Action<MemoryCacheOptions>? action = null)
        {
            ArgumentNullException.ThrowIfNull(services);

            services.Configure(action ?? (config => { }));
            services.AddMemoryCache();
            services.TryAddSingleton<MemoryCacheKeyManager>();
            // Provider 无实例状态（状态在单例的 IMemoryCache/KeyManager 中），
            // 注册为 Singleton 与 Redis 实现保持一致，单例服务也能直接注入 ICacheProvider。
            services.TryAddSingleton<MemoryCacheProvider>();
            services.TryAddSingleton<IMemoryCacheProvider>(sp => sp.GetRequiredService<MemoryCacheProvider>());
            services.TryAddSingleton<ICacheProvider>(sp => sp.GetRequiredService<MemoryCacheProvider>());

            return services;
        }

        /// <summary>
        /// 添加内存缓存
        /// </summary>
        /// <param name="services"></param>
        /// <param name="action"></param>
        [Obsolete]
        public static IServiceCollection AddMemoryCacheExtension(this IServiceCollection services,
                                                                 Action<MemoryCacheOptions> action)
        {
            ArgumentNullException.ThrowIfNull(action);

            return services.AddMemoryCacheStore(action);
        }
    }
}

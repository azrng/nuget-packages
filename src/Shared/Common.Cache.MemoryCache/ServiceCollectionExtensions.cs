using Azrng.Cache.Core;
using Microsoft.Extensions.DependencyInjection;
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
                                                             Action<MemoryConfig> action = null)
        {
            ArgumentNullException.ThrowIfNull(services);

            services.Configure(action ?? (config => { }));
            services.AddMemoryCache();
            services.AddScoped<IMemoryCacheProvider, MemoryCacheProvider>();
            services.AddScoped<ICacheProvider, MemoryCacheProvider>();

            return services;
        }

        /// <summary>
        /// 添加内存缓存
        /// </summary>
        /// <param name="services"></param>
        /// <param name="action"></param>
        [Obsolete]
        public static IServiceCollection AddMemoryCacheExtension(this IServiceCollection services,
                                                                 Action<MemoryConfig> action)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(action);

            services.Configure(action);
            services.AddMemoryCache();
            services.AddScoped<IMemoryCacheProvider, MemoryCacheProvider>();
            services.AddScoped<ICacheProvider, MemoryCacheProvider>();

            return services;
        }
    }
}
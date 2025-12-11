using Azrng.Cache.MemoryCache;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Cache.MemoryCache.Test
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMemoryCacheStore(x =>
            {
                x.DefaultExpiry = TimeSpan.FromSeconds(5);
                x.CacheEmptyCollections = false;
            });
        }
    }
}
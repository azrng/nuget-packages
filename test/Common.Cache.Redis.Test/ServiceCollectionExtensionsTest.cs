using Azrng.Cache.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Cache.Redis.Test
{
    public class ServiceCollectionExtensionsTest
    {
        [Fact]
        public void AddRedisCacheStore_RegistersSingleSharedProvider()
        {
            var services = new ServiceCollection();
            services.AddLogging();

            services.AddRedisCacheStore(config =>
            {
                config.ConnectionString = "localhost:6379,DefaultDatabase=0";
                config.KeyPrefix = "test";
            });

            using var serviceProvider = services.BuildServiceProvider();
            var cacheProvider = serviceProvider.GetRequiredService<ICacheProvider>();
            var redisProvider = serviceProvider.GetRequiredService<IRedisProvider>();

            Assert.Same(redisProvider, cacheProvider);

            var cacheProviderRegistrations = services.Where(descriptor => descriptor.ServiceType == typeof(ICacheProvider)).ToList();
            Assert.Single(cacheProviderRegistrations);
            Assert.Equal(ServiceLifetime.Singleton, cacheProviderRegistrations[0].Lifetime);
        }
    }
}

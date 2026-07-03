using Microsoft.Extensions.DependencyInjection;
using Xunit.DependencyInjection.Logging;

namespace Common.Cache.Redis.Test
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // 默认连本地 Redis；如需连其它实例，设置环境变量 COMMON_CACHE_REDIS_TEST_CONNECTION
            var connectionString = Environment.GetEnvironmentVariable("COMMON_CACHE_REDIS_TEST_CONNECTION")
                ?? "127.0.0.1:6379,DefaultDatabase=0";

            services.AddRedisCacheStore(x =>
            {
                x.ConnectionString = connectionString;
                x.KeyPrefix = "azrng";
                x.CacheEmptyCollections = false;
                x.InitErrorIntervalSecond = 0;
            });

            services.AddLogging(x => x.AddXunitOutput());
        }
    }
}
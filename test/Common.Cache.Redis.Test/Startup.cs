using Microsoft.Extensions.DependencyInjection;
using Xunit.DependencyInjection.Logging;

namespace Common.Cache.Redis.Test
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            var connectionString = "127.0.0.1:6379,DefaultDatabase=0";

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
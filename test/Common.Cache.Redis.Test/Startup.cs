using Microsoft.Extensions.DependencyInjection;
using Xunit.DependencyInjection.Logging;

namespace Common.Cache.Redis.Test
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // services.AddRedisCacheStore(x =>
            // {
            //     x.ConnectionString = "localhost:6379,password=,DefaultDatabase=0";
            //     x.KeyPrefix = "test";
            //     x.CacheEmptyCollections = false;
            // });

            services.AddRedisCacheStore(x =>
            {
                x.ConnectionString = "172.16.127.100:25089,password=,DefaultDatabase=0";
                x.KeyPrefix = "azrng";
                x.CacheEmptyCollections = false;
            });

            services.AddLogging(x => x.AddXunitOutput());
        }
    }
}
using Azrng.DistributeLock.Redis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit.DependencyInjection.Logging;

namespace Azrng.DistributeLock.Redis.Test
{
    public class Startup
    {
        public void ConfigureHost(IHostBuilder hostBuilder) { }

        public void ConfigureServices(IServiceCollection services)
        {
            var conn = "localhost:6379,password=123456,defaultdatabase=0,abortConnect=false";
            // conn = "172.16.127.101:36379,defaultDatabase=0,connectTimeout=100000,syncTimeout=100000,connectRetry=50";
            services.AddRedisLockProvider(conn);
            services.AddLogging(x => x.AddXunitOutput());
        }
    }
}
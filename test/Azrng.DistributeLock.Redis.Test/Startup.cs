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
            // 优先读环境变量，避免真实连接信息硬编码进仓库
            var conn = Environment.GetEnvironmentVariable("AZRNG_LOCK_REDIS_CONN")
                       ?? "localhost:6379,abortConnect=false";
            services.AddRedisLockProvider(conn);
            services.AddLogging(x => x.AddXunitOutput());
        }
    }
}
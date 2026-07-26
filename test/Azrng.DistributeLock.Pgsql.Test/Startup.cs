using Azrng.DistributeLock.PostgreSql;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit.DependencyInjection.Logging;

namespace Azrng.DistributeLock.Pgsql.Test
{
    public class Startup
    {
        public void ConfigureHost(IHostBuilder hostBuilder) { }

        public void ConfigureServices(IServiceCollection services)
        {
            // 优先读环境变量，避免真实连接信息硬编码进仓库
            var conn = Environment.GetEnvironmentVariable("AZRNG_LOCK_PG_CONN")
                       ?? "Host=localhost;Username=postgres;Password=123456;Database=zyp-test";
            services.AddDbLockProvider(conn);
            services.AddLogging(x => x.AddXunitOutput());
        }
    }
}
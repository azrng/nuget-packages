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
            var conn = "Host=localhost;Username=postgres;Password=123456;Database=zyp-test";
            services.AddDbLockProvider(conn);
            services.AddLogging(x => x.AddXunitOutput());
        }
    }
}
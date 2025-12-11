using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit.DependencyInjection.Logging;

namespace Azrng.DistributeLock.InMemory.Test
{
    public class Startup
    {
        public void ConfigureHost(IHostBuilder hostBuilder) { }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddInMemory();
            services.AddLogging(x => x.AddXunitOutput());
        }
    }
}
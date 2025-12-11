using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit.DependencyInjection.Logging;

namespace Common.HttpClients.Test
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(x => x.AddXunitOutput());
            services.AddHttpClientService(options =>
            {
                options.FailThrowException = false;
                options.Timeout = 10;
                options.IgnoreUntrustedCertificate = true;
            });
        }

        public void Configure() { }
    }
}
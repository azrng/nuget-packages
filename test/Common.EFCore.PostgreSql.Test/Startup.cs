using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit.DependencyInjection.Logging;

namespace Common.EFCore.PostgreSql.Test;

public class Startup
{
    public void ConfigureHost(IHostBuilder hostBuilder) { }

    public void ConfigureServices(IServiceCollection services, HostBuilderContext hostBuilderContext)
    {
        services.AddLogging(x => x.AddXunitOutput());
    }
}
using Microsoft.Extensions.DependencyInjection;
using Xunit.DependencyInjection.Logging;

namespace Azrng.NmcWeather.Test;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddLogging(x => x.AddXunitOutput());
        services.AddNmcWeather();
    }
}
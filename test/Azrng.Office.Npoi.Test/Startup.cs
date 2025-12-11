using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Azrng.Office.Npoi.Test;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        var outputHelper = new TestOutputHelper();
        services.AddSingleton<ITestOutputHelper>(outputHelper);
        services.AddLogging(builder => builder.AddXUnit(outputHelper));
    }
}
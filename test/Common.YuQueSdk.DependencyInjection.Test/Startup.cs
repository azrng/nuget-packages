using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Common.YuQueSdk.DependencyInjection.Test;

public class Startup
{
    public void ConfigureHost(IHostBuilder hostBuilder) { }

    public void ConfigureServices(IServiceCollection services)
    {
        //var configuration = hostBuilderContext.Configuration;
        services.AddYuQueService(x => x.AuthToken = "Aombx9s000UCHLM9qtk4b9StsapocQ16QIYP2JGz");

        services.AddHttpClient();

        var outputHelper = new TestOutputHelper();
        services.AddSingleton<ITestOutputHelper>(outputHelper);
        services.AddLogging(builder => builder.AddXUnit(outputHelper));

        //services.ConfigureNewtonsoftJson(options => { });
    }

    //public void Configure()
    //{
    //}
}
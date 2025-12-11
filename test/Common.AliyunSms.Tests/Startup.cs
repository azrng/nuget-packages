using Azrng.AliyunSms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Common.AliyunSms.Tests;

public class Startup
{
    public void ConfigureHost(IHostBuilder hostBuilder)
    {
    }

    public void ConfigureServices(IServiceCollection services, HostBuilderContext hostBuilderContext)
    {
        services.AddAliyunSms(options =>
        {
        });
    }
}
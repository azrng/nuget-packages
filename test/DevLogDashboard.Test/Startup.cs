using Azrng.DevLogDashboard.Extensions;
using Azrng.DevLogDashboard.Storage;
using DevLogDashboard.Test.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit.DependencyInjection.Logging;

namespace DevLogDashboard.Test;

public class Startup
{
    public void ConfigureHost(IHostBuilder hostBuilder)
    {
        // 配置测试主机
    }

    public void ConfigureServices(IServiceCollection services)
    {
        // 注册 DevLogDashboard 服务
        services.AddDevLogDashboard<InMemoryLogStore>(options =>
        {
            options.EndpointPath = "/dev-logs";
            options.MaxLogCount = 1000;
            options.ApplicationName = "TestApp";
        });

        // 注册测试辅助服务
        services.AddSingleton<Helpers.TestDataGenerator>();

        // 配置日志
        services.AddLogging(x => x.AddXunitOutput());
    }
}

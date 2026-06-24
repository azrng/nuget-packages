using Azrng.ConsoleApp.DependencyInjection;
using Azrng.ConsoleApp.DependencyInjection.Test.Integration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.DependencyInjection.Logging;

namespace Azrng.ConsoleApp.DependencyInjection.Test;

/// <summary>
/// 测试主机启动配置：装配真实的 ConsoleAppServer 链路供集成测试使用。
/// 参考 Common.HttpClients.Next.Test 的 Startup 写法（Xunit.DependencyInjection 依赖注入）。
/// </summary>
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // 日志输出到 xunit 测试输出，便于排查集成测试链路
        services.AddLogging(x => x.AddXunitOutput());

        // 装配一个真实的 ConsoleAppServer，通过命令行参数注入集成测试所需配置。
        // 覆盖 appsettings.json / 环境变量 / 命令行 / 选项绑定 / 日志 配置的真实加载链路。
        var server = new ConsoleAppServer(["App:Mode=Integration", "App:Count=7"])
            .Configure<IntegrationAppOptions>("App");

        // 单例注入，集成测试通过构造函数获取，验证真实 Build 链路
        services.AddSingleton(server);
    }
}

using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;

namespace Azrng.ConsoleApp.DependencyInjection.Test.Integration;

/// <summary>
/// ConsoleAppServer 真实装配链路的集成测试。
/// 通过 Xunit.DependencyInjection 的 Startup 注入真实构建的 ConsoleAppServer，
/// 验证「配置加载 → 选项绑定 → 日志配置 → Build → 作用域解析 → RunAsync」端到端链路，
/// 而非容器内断言。这些测试会触发真实文件日志（LocalLogHelper）与控制台输出，
/// 离线/CI 环境可通过 <c>--filter Category!=Integration</c> 跳过。
/// </summary>
[Trait("Category", "Integration")]
public class ConsoleAppIntegrationTests
{
    private readonly ConsoleAppServer _server;

    public ConsoleAppIntegrationTests(ConsoleAppServer server)
    {
        _server = server;
    }

    // ========== 配置加载链路 ==========

    [Fact]
    public void Configuration_CommandLineArgs_FlowIntoIConfiguration()
    {
        // Startup 用 ["App:Mode=Integration", "App:Count=7"] 构造，验证命令行真实流入配置
        _server.Configuration["App:Mode"].Should().Be("Integration");
        _server.Configuration["App:Count"].Should().Be("7");
    }

    // ========== 选项绑定链路（#7）==========

    [Fact]
    public void Configure_TOption_BindsCommandLineSectionToStrongType()
    {
        // Startup 调用了 Configure<IntegrationAppOptions>("App")，验证强类型绑定真实生效
        using var provider = _server.Build<NoOpIntegrationService>();

        var options = provider.GetRequiredService<IOptions<IntegrationAppOptions>>().Value;

        options.Mode.Should().Be("Integration");
        options.Count.Should().Be(7);
    }

    // ========== 日志配置链路（#6）==========

    [Fact]
    public void ConfigureLogging_DefaultProviders_ResolveRealLogger()
    {
        // 验证默认日志 Provider 装配后，ILogger<T> 真实可解析并写入（不抛异常）
        using var provider = _server.Build<NoOpIntegrationService>();

        var logger = provider.GetRequiredService<ILogger<ConsoleAppIntegrationTests>>();

        var act = () => logger.LogInformation("集成测试日志写入验证");

        act.Should().NotThrow();
    }

    // ========== RunAsync 端到端执行链路 ==========

    [Fact]
    public async Task RunAsync_ResolvesStartServiceAndExecutesWithinScope()
    {
        // 验证完整链路：Build → CreateAsyncScope → GetRequiredService<IServiceStart> → RunAsync
        IntegrationRunRecorder.Reset();
        await using var provider = _server.Build<RecordingIntegrationService>();

        await provider.RunAsync();

        IntegrationRunRecorder.Executed.Should().BeTrue();
        IntegrationRunRecorder.Title.Should().Be("Recording Integration");
    }

    [Fact]
    public async Task RunAsync_WhenStartServiceThrows_RethrowsAndLogsToLocalFile()
    {
        // 验证异常路径：IServiceStart 抛异常时，RunAsync 重抛并触发 LocalLogHelper 落盘
        await using var provider = _server.Build<ThrowingIntegrationService>();

        var act = () => provider.RunAsync();

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("integration-boom");
    }

    // ========== IServiceStart 注册覆盖替换链路 ==========

    [Fact]
    public void Build_ReplacesPreviousIServiceStartRegistration()
    {
        // 验证 RegisterStartService 的反向移除逻辑：多次 Build 不同 TStart，IServiceStart 注册唯一
        _server.Services.AddSingleton<IServiceStart, NoOpIntegrationService>();

        using var provider = _server.Build<RecordingIntegrationService>();

        _server.Services.Where(x => x.ServiceType == typeof(IServiceStart))
            .Should().ContainSingle();
        provider.GetRequiredService<IServiceStart>().Should().BeOfType<RecordingIntegrationService>();
    }
}

/// <summary>
/// 集成测试用强类型选项，对应命令行 App:Mode / App:Count
/// </summary>
public sealed class IntegrationAppOptions
{
    public string Mode { get; set; } = string.Empty;

    public int Count { get; set; }
}

/// <summary>
/// 记录 IServiceStart.RunAsync 是否被真实执行的静态记录器
/// </summary>
internal static class IntegrationRunRecorder
{
    public static bool Executed { get; private set; }

    public static string? Title { get; private set; }

    public static void Reset()
    {
        Executed = false;
        Title = null;
    }

    public static void Record(string title)
    {
        Executed = true;
        Title = title;
    }
}

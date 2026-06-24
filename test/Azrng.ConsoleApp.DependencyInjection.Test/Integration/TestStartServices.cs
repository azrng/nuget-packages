namespace Azrng.ConsoleApp.DependencyInjection.Test.Integration;

/// <summary>
/// 无操作启动服务，仅用于装配链路验证
/// </summary>
public sealed class NoOpIntegrationService : IServiceStart
{
    public string Title => "NoOp Integration";

    public Task RunAsync() => Task.CompletedTask;
}

/// <summary>
/// 记录执行状态的启动服务，用于验证 RunAsync 端到端被真实调用
/// </summary>
public sealed class RecordingIntegrationService : IServiceStart
{
    public string Title => "Recording Integration";

    public Task RunAsync()
    {
        IntegrationRunRecorder.Record(Title);
        return Task.CompletedTask;
    }
}

/// <summary>
/// 抛异常的启动服务，用于验证异常重抛与本地日志落盘链路
/// </summary>
public sealed class ThrowingIntegrationService : IServiceStart
{
    public string Title => "Throwing Integration";

    public Task RunAsync() => throw new InvalidOperationException("integration-boom");
}

namespace Azrng.EventBus.InMemory;

/// <summary>
/// InMemory 事件总线配置选项
/// </summary>
public class InMemoryEventBusOptions
{
    /// <summary>
    /// 处理器失败策略，默认 LogAndContinue（只记日志，发布方不感知）
    /// </summary>
    public HandlerFailurePolicy HandlerFailurePolicy { get; set; } = HandlerFailurePolicy.LogAndContinue;
}

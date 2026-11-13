namespace Azrng.EventBus.InMemory;

/// <summary>
/// 处理器失败策略
/// </summary>
public enum HandlerFailurePolicy
{
    /// <summary>
    /// 记录错误日志并继续，发布方不感知失败（默认，与 1.1.1 行为兼容）
    /// </summary>
    LogAndContinue = 0,

    /// <summary>
    /// 等全部处理器执行完毕后，向发布方抛出 AggregateException（单个失败仍不阻断其他处理器）
    /// </summary>
    Throw = 1
}

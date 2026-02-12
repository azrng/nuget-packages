using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Azrng.EventBus.Core.Events;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Azrng.EventBus.Core.Abstractions;

/// <summary>
/// 事件总线基类，提供公共的序列化和反序列化方法
/// </summary>
public abstract class EventBusBase
{
    /// <summary>
    /// 订阅信息
    /// </summary>
    protected EventBusSubscriptionInfo SubscriptionInfo { get; }

    /// <summary>
    /// 日志记录器
    /// </summary>
    protected ILogger Logger { get; }

    /// <summary>
    /// 初始化事件总线基类
    /// </summary>
    protected EventBusBase(ILogger logger, IOptions<EventBusSubscriptionInfo> subscriptionOptions)
    {
        Logger = logger;
        SubscriptionInfo = subscriptionOptions.Value;
    }

    /// <summary>
    /// 序列化事件为 JSON 字符串
    /// </summary>
    [UnconditionalSuppressMessage("Trimming", "IL2026:RequiresUnreferencedCode",
        Justification =
            "The 'JsonSerializer.IsReflectionEnabledByDefault' feature switch, which is set to false by default for trimmed .NET apps, ensures the JsonSerializer doesn't use Reflection.")]
    [UnconditionalSuppressMessage("AOT", "IL3050:RequiresDynamicCode", Justification = "See above.")]
    protected string SerializeMessage(IntegrationEvent @event)
    {
        return JsonSerializer.Serialize(@event, @event.GetType(), SubscriptionInfo.JsonSerializerOptions);
    }

    /// <summary>
    /// 序列化事件为 UTF-8 字节数组
    /// </summary>
    [UnconditionalSuppressMessage("Trimming", "IL2026:RequiresUnreferencedCode",
        Justification =
            "The 'JsonSerializer.IsReflectionEnabledByDefault' feature switch, which is set to false by default for trimmed .NET apps, ensures the JsonSerializer doesn't use Reflection.")]
    [UnconditionalSuppressMessage("AOT", "IL3050:RequiresDynamicCode", Justification = "See above.")]
    protected byte[] SerializeMessageToUtf8Bytes(IntegrationEvent @event)
    {
        return JsonSerializer.SerializeToUtf8Bytes(@event, @event.GetType(), SubscriptionInfo.JsonSerializerOptions);
    }

    /// <summary>
    /// 从 JSON 字符串反序列化事件
    /// </summary>
    [UnconditionalSuppressMessage("Trimming", "IL2026:RequiresUnreferencedCode",
        Justification =
            "The 'JsonSerializer.IsReflectionEnabledByDefault' feature switch, which is set to false by default for trimmed .NET apps, ensures the JsonSerializer doesn't use Reflection.")]
    [UnconditionalSuppressMessage("AOT", "IL3050:RequiresDynamicCode", Justification = "See above.")]
    protected IntegrationEvent? DeserializeMessage(string message, Type eventType)
    {
        return JsonSerializer.Deserialize(message, eventType, SubscriptionInfo.JsonSerializerOptions) as IntegrationEvent;
    }

    /// <summary>
    /// 从 UTF-8 字节数组反序列化事件
    /// </summary>
    [UnconditionalSuppressMessage("Trimming", "IL2026:RequiresUnreferencedCode",
        Justification =
            "The 'JsonSerializer.IsReflectionEnabledByDefault' feature switch, which is set to false by default for trimmed .NET apps, ensures the JsonSerializer doesn't use Reflection.")]
    [UnconditionalSuppressMessage("AOT", "IL3050:RequiresDynamicCode", Justification = "See above.")]
    protected IntegrationEvent? DeserializeMessage(ReadOnlySpan<byte> bytes, Type eventType)
    {
        return JsonSerializer.Deserialize(bytes, eventType, SubscriptionInfo.JsonSerializerOptions) as IntegrationEvent;
    }
}

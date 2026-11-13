using System.Collections.Concurrent;
using Azrng.EventBus.Core.Abstractions;
using Azrng.EventBus.Core.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Azrng.EventBus.InMemory;

/// <summary>
/// 内存事件总线实现，适用于单机环境的同步事件驱动架构。
/// 处理器直接收到发布的事件对象（同一实例，无序列化往返），不应在处理器内修改事件。
/// </summary>
public sealed class InMemoryEventBus : EventBusBase, IEventBus
{
    private readonly IServiceProvider _serviceProvider;
    private readonly InMemoryEventBusOptions _options;

    public InMemoryEventBus(ILogger<InMemoryEventBus> logger, IServiceProvider serviceProvider,
                            IOptions<EventBusSubscriptionInfo> subscriptionOptions,
                            IOptions<InMemoryEventBusOptions>? options = null)
        : base(logger, subscriptionOptions)
    {
        _serviceProvider = serviceProvider;
        _options = options?.Value ?? new InMemoryEventBusOptions();
    }

    /// <summary>
    /// 在当前调用链中分发事件，完成全部处理器后返回。
    /// </summary>
    public async Task PublishAsync(IntegrationEvent @event, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(@event);
        cancellationToken.ThrowIfCancellationRequested();

        var eventName = @event.GetType().Name;
        if (Logger.IsEnabled(LogLevel.Trace))
        {
            Logger.LogTrace("Publishing InMemory event: {EventId} ({EventName})", @event.Id, eventName);
        }

        if (!SubscriptionInfo.EventTypes.TryGetValue(eventName, out var eventType))
        {
            Logger.LogWarning("Unable to resolve event type for event name {EventName}", eventName);
            return;
        }

        await using var scope = _serviceProvider.CreateAsyncScope();

        List<IIntegrationEventHandler> handlers;
        try
        {
            // 激活（构造）失败遵循与处理器失败相同的策略；默认只记日志
            handlers = scope.ServiceProvider.GetKeyedServices<IIntegrationEventHandler>(eventType).ToList();
        }
        catch (Exception ex) when (_options.HandlerFailurePolicy == HandlerFailurePolicy.LogAndContinue)
        {
            Logger.LogError(ex, "Error activating handlers for event {EventName}", eventName);
            return;
        }

        if (handlers.Count == 0)
        {
            Logger.LogWarning("No handlers registered for event {EventName}", eventName);
            return;
        }

        if (Logger.IsEnabled(LogLevel.Trace))
        {
            Logger.LogTrace("Processing InMemory event: {EventName} with {HandlerCount} handler(s)", eventName,
                handlers.Count);
        }

        // 处理失败先收集再按策略决定是否抛出，保证单个失败不阻断其他处理器并行执行
        var failures = new ConcurrentQueue<Exception>();

        await Task.WhenAll(handlers.Select(async handler =>
        {
            try
            {
                await handler.Handle(@event, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                failures.Enqueue(ex);
                Logger.LogError(ex, "Error processing event {EventName} with handler {HandlerType}", eventName,
                    handler.GetType().Name);
            }
        })).ConfigureAwait(false);

        if (_options.HandlerFailurePolicy == HandlerFailurePolicy.Throw && !failures.IsEmpty)
        {
            // 全部处理器执行完毕后统一抛出：既让发布方感知失败，又保持错误隔离
            throw new AggregateException($"One or more handlers failed for event {eventName}", failures);
        }
    }
}

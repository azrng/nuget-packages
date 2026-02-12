using Azrng.EventBus.Core.Abstractions;
using Azrng.EventBus.Core.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Azrng.EventBus.InMemory;

/// <summary>
/// 内存事件总线实现，适用于单机环境的事件驱动架构
/// </summary>
public sealed class InMemoryEventBus : EventBusBase, IEventBus
{
    private readonly IServiceProvider _serviceProvider;

    public InMemoryEventBus(ILogger<InMemoryEventBus> logger, IServiceProvider serviceProvider,
                            IOptions<EventBusSubscriptionInfo> subscriptionOptions)
        : base(logger, subscriptionOptions)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// 发布事件到内存总线
    /// </summary>
    /// <param name="event">要发布的事件</param>
    /// <param name="cancellationToken">取消令牌</param>
    public async Task PublishAsync(IntegrationEvent @event, CancellationToken cancellationToken = default)
    {
        var eventName = @event.GetType().Name;

        if (Logger.IsEnabled(LogLevel.Trace))
        {
            Logger.LogTrace("Publishing InMemory event: {EventId} ({EventName})", @event.Id, eventName);
        }

        var message = SerializeMessage(@event);

        await ProcessEventAsync(eventName, message, cancellationToken);
    }

    /// <summary>
    /// 处理事件，调用所有订阅的处理器
    /// </summary>
    private async Task ProcessEventAsync(string eventName, string message, CancellationToken cancellationToken)
    {
        if (Logger.IsEnabled(LogLevel.Trace))
        {
            Logger.LogTrace("Processing InMemory event: {EventName}", eventName);
        }

        await using var scope = _serviceProvider.CreateAsyncScope();

        if (!SubscriptionInfo.EventTypes.TryGetValue(eventName, out var eventType))
        {
            Logger.LogWarning("Unable to resolve event type for event name {EventName}", eventName);
            return;
        }

        // 反序列化事件
        var integrationEvent = DeserializeMessage(message, eventType);
        if (integrationEvent == null)
        {
            Logger.LogError("Failed to deserialize event {EventName}", eventName);
            return;
        }

        // 获取所有事件处理器并执行
        var handlers = scope.ServiceProvider.GetKeyedServices<IIntegrationEventHandler>(eventType).ToList();

        if (handlers.Count == 0)
        {
            Logger.LogWarning("No handlers registered for event {EventName}", eventName);
            return;
        }

        // 并行执行所有事件处理器
        var handlerTasks = handlers.Select(async handler =>
        {
            try
            {
                await handler.Handle(integrationEvent, cancellationToken);
            }
            catch (Exception ex)
            {
                // 错误隔离：一个处理器失败不影响其他处理器
                Logger.LogError(ex, "Error processing event {EventName} with handler {HandlerType}",
                    eventName, handler.GetType().Name);
            }
        });

        await Task.WhenAll(handlerTasks);
    }
}

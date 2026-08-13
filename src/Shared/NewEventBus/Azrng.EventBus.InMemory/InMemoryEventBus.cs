using Azrng.EventBus.Core.Abstractions;
using Azrng.EventBus.Core.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Azrng.EventBus.InMemory;

/// <summary>
/// 内存事件总线实现，适用于单机环境的同步事件驱动架构。
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
    /// 在当前调用链中分发事件，完成全部处理器后返回。
    /// </summary>
    public async Task PublishAsync(IntegrationEvent @event, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(@event);
        cancellationToken.ThrowIfCancellationRequested();

        var eventName = @event.GetType().Name;
        if (Logger.IsEnabled(LogLevel.Trace))
            Logger.LogTrace("Publishing InMemory event: {EventId} ({EventName})", @event.Id, eventName);

        var message = SerializeMessage(@event);
        await ProcessEventAsync(eventName, message, cancellationToken).ConfigureAwait(false);
    }

    private async Task ProcessEventAsync(string eventName, string message, CancellationToken cancellationToken)
    {
        if (Logger.IsEnabled(LogLevel.Trace))
            Logger.LogTrace("Processing InMemory event: {EventName}", eventName);

        if (!SubscriptionInfo.EventTypes.TryGetValue(eventName, out var eventType))
        {
            Logger.LogWarning("Unable to resolve event type for event name {EventName}", eventName);
            return;
        }

        var integrationEvent = DeserializeMessage(message, eventType);
        if (integrationEvent is null)
        {
            Logger.LogError("Failed to deserialize event {EventName}", eventName);
            return;
        }

        await using var scope = _serviceProvider.CreateAsyncScope();
        var handlers = scope.ServiceProvider.GetKeyedServices<IIntegrationEventHandler>(eventType).ToList();
        if (handlers.Count == 0)
        {
            Logger.LogWarning("No handlers registered for event {EventName}", eventName);
            return;
        }

        await Task.WhenAll(handlers.Select(handler => HandleAsync(handler, integrationEvent, eventName, cancellationToken)))
                  .ConfigureAwait(false);
    }

    private async Task HandleAsync(IIntegrationEventHandler handler, IntegrationEvent integrationEvent, string eventName,
                                   CancellationToken cancellationToken)
    {
        try
        {
            await handler.Handle(integrationEvent, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error processing event {EventName} with handler {HandlerType}", eventName,
                handler.GetType().Name);
        }
    }
}

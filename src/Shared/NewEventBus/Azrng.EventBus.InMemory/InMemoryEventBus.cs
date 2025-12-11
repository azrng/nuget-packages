using Azrng.EventBus.Core.Abstractions;
using Azrng.EventBus.Core.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;

namespace Azrng.EventBus.InMemory;

/// <summary>
/// rabbitmq 事件总线实现
/// </summary>
public sealed class InMemoryEventBus : IEventBus
{
    private readonly ILogger<InMemoryEventBus> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly EventBusSubscriptionInfo _subscriptionInfo;

    public InMemoryEventBus(ILogger<InMemoryEventBus> logger, IServiceProvider serviceProvider,
                            IOptions<EventBusSubscriptionInfo> subscriptionOptions)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _subscriptionInfo = subscriptionOptions.Value;
    }

    public async Task PublishAsync(IntegrationEvent @event)
    {
        var routingKey = @event.GetType().Name;

        if (_logger.IsEnabled(LogLevel.Trace))
        {
            _logger.LogTrace("Creating InMemory channel to publish event: {EventId} ({EventName})", @event.Id,
                routingKey);
        }

        var message = SerializeMessage(@event);

        await ProcessEvent(routingKey, message);
    }

    private async Task ProcessEvent(string eventName, string message)
    {
        if (_logger.IsEnabled(LogLevel.Trace))
        {
            _logger.LogTrace("Processing InMemory event: {EventName}", eventName);
        }

        await using var scope = _serviceProvider.CreateAsyncScope();

        if (!_subscriptionInfo.EventTypes.TryGetValue(eventName, out var eventType))
        {
            _logger.LogWarning("Unable to resolve event type for event name {EventName}", eventName);
            return;
        }

        // Deserialize the event
        var integrationEvent = DeserializeMessage(message, eventType);

        // REVIEW: This could be done in parallel

        // Get all the handlers using the event type as the key
        foreach (var handler in scope.ServiceProvider.GetKeyedServices<IIntegrationEventHandler>(eventType))
        {
            await handler.Handle(integrationEvent);
        }
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026:RequiresUnreferencedCode",
        Justification =
            "The 'JsonSerializer.IsReflectionEnabledByDefault' feature switch, which is set to false by default for trimmed .NET apps, ensures the JsonSerializer doesn't use Reflection.")]
    [UnconditionalSuppressMessage("AOT", "IL3050:RequiresDynamicCode", Justification = "See above.")]
    private IntegrationEvent DeserializeMessage(string message, Type eventType)
    {
        return JsonSerializer.Deserialize(message, eventType, _subscriptionInfo.JsonSerializerOptions) as
            IntegrationEvent;
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026:RequiresUnreferencedCode",
        Justification =
            "The 'JsonSerializer.IsReflectionEnabledByDefault' feature switch, which is set to false by default for trimmed .NET apps, ensures the JsonSerializer doesn't use Reflection.")]
    [UnconditionalSuppressMessage("AOT", "IL3050:RequiresDynamicCode", Justification = "See above.")]
    private string SerializeMessage(IntegrationEvent @event)
    {
        return JsonSerializer.Serialize(@event, @event.GetType(), _subscriptionInfo.JsonSerializerOptions);
    }
}
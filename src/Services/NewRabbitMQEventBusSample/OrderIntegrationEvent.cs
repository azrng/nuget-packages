using Azrng.EventBus.Core.Events;

namespace NewRabbitMQEventBusSample;

public record OrderIntegrationEvent(int OrderId, IEnumerable<string> OrderStockItems)
    : IntegrationEvent;
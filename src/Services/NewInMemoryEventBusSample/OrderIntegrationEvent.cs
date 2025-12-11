using Azrng.EventBus.Core.Events;

namespace NewInMemoryEventBusSample;

public record OrderIntegrationEvent(int OrderId, IEnumerable<string> OrderStockItems)
    : IntegrationEvent;
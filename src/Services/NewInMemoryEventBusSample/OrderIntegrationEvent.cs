using Azrng.EventBus.Core.Events;

namespace NewInMemoryEventBusSample;

public class OrderIntegrationEvent(int OrderId, IEnumerable<string> OrderStockItems)
    : IntegrationEvent
{
    public int OrderId { get; set; }

    public IEnumerable<string> OrderStockItems { get; set; }
}
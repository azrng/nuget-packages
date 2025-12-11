using Azrng.EventBus.Core.Abstractions;

namespace NewInMemoryEventBusSample;

public class OrderIntegrationEventHandler(
    ILogger<OrderIntegrationEventHandler> logger) : IIntegrationEventHandler<OrderIntegrationEvent>
{
    public async Task Handle(OrderIntegrationEvent @event)
    {
        logger.LogInformation("Handling integration event: {IntegrationEventId} - ({@IntegrationEvent})", @event.Id,
            @event);

        logger.LogInformation($"接收到信息：{@event.OrderId}");
    }
}

public class OrderIntegrationEventHandler2 : IIntegrationEventHandler<OrderIntegrationEvent>
{
    private readonly ILogger<OrderIntegrationEventHandler> _logger;

    public OrderIntegrationEventHandler2(ILogger<OrderIntegrationEventHandler> logger)
    {
        this._logger = logger;
    }

    public async Task Handle(OrderIntegrationEvent @event)
    {
        _logger.LogInformation("Handling integration event: {IntegrationEventId} - ({@IntegrationEvent})", @event.Id,
            @event);

        _logger.LogInformation($"消费着2 接收到信息：{@event.OrderId}");
    }
}
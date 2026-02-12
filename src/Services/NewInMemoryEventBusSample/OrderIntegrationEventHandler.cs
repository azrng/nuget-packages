using Azrng.EventBus.Core.Abstractions;

namespace NewInMemoryEventBusSample;

public class OrderIntegrationEventHandler : IIntegrationEventHandler<OrderIntegrationEvent>
{
    private readonly ILogger<OrderIntegrationEventHandler> _logger;

    public OrderIntegrationEventHandler(ILogger<OrderIntegrationEventHandler> logger)
    {
        this._logger = logger;
    }

    public Task Handle(OrderIntegrationEvent @event, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling integration event: {IntegrationEventId} - ({@IntegrationEvent})", @event.Id,
            @event);

        _logger.LogInformation($"接收到信息：{@event.OrderId}");

        return Task.CompletedTask;
    }
}

public class OrderIntegrationEventHandler2 : IIntegrationEventHandler<OrderIntegrationEvent>
{
    private readonly ILogger<OrderIntegrationEventHandler> _logger;

    public OrderIntegrationEventHandler2(ILogger<OrderIntegrationEventHandler> logger)
    {
        this._logger = logger;
    }

    public Task Handle(OrderIntegrationEvent @event, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling integration event: {IntegrationEventId} - ({@IntegrationEvent})", @event.Id,
            @event);

        _logger.LogInformation($"消费着2 接收到信息：{@event.OrderId}");
        return Task.CompletedTask;
    }
}
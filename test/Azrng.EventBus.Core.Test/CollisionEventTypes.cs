using Azrng.EventBus.Core.Abstractions;
using Azrng.EventBus.Core.Events;

namespace CollisionA
{
    internal sealed class OrderCreatedEvent : IntegrationEvent;

    internal sealed class OrderCreatedEventHandler : IIntegrationEventHandler<OrderCreatedEvent>
    {
        public Task Handle(OrderCreatedEvent @event, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}

namespace CollisionB
{
    internal sealed class OrderCreatedEvent : IntegrationEvent;

    internal sealed class OrderCreatedEventHandler : IIntegrationEventHandler<OrderCreatedEvent>
    {
        public Task Handle(OrderCreatedEvent @event, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}

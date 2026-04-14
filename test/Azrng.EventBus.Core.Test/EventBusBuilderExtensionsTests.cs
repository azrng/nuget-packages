using System.Text.Json;
using Azrng.EventBus.Core.Abstractions;
using Azrng.EventBus.Core.Events;
using Azrng.EventBus.Core.Extensions;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Azrng.EventBus.Core.Test;

public class EventBusBuilderExtensionsTests
{
    [Fact]
    public void GetGenericTypeName_ShouldReturnReadableName()
    {
        typeof(List<int>).GetGenericTypeName().Should().Be("List<Int32>");
        typeof(Dictionary<string, List<int>>).GetGenericTypeName()
            .Should().Be("Dictionary<String,List`1>");
    }

    [Fact]
    public void ConfigureJsonOptions_ShouldUpdateSubscriptionSerializerOptions()
    {
        var services = new ServiceCollection();
        var builder = new TestEventBusBuilder(services);

        builder.ConfigureJsonOptions(options => options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase);

        using var provider = services.BuildServiceProvider();
        provider.GetRequiredService<IOptions<EventBusSubscriptionInfo>>()
            .Value.JsonSerializerOptions.PropertyNamingPolicy
            .Should().Be(JsonNamingPolicy.CamelCase);
    }

    [Fact]
    public void AddSubscription_ShouldRegisterKeyedHandlerAndEventType()
    {
        var services = new ServiceCollection();
        var builder = new TestEventBusBuilder(services);

        builder.AddSubscription<TestEvent, TestEventHandler>();

        using var provider = services.BuildServiceProvider();
        var subscriptionInfo = provider.GetRequiredService<IOptions<EventBusSubscriptionInfo>>().Value;
        var handlers = provider.GetKeyedServices<IIntegrationEventHandler>(typeof(TestEvent));

        subscriptionInfo.EventTypes.Should().ContainKey(nameof(TestEvent))
            .WhoseValue.Should().Be(typeof(TestEvent));
        handlers.Should().ContainSingle().Which.Should().BeOfType<TestEventHandler>();
    }

    [Fact]
    public void AddAutoSubscription_ShouldRegisterConcreteHandlersFromAssembly()
    {
        var services = new ServiceCollection();
        var builder = new TestEventBusBuilder(services);

        builder.AddAutoSubscription(typeof(AutoDiscoveredHandler).Assembly);

        using var provider = services.BuildServiceProvider();
        var subscriptionInfo = provider.GetRequiredService<IOptions<EventBusSubscriptionInfo>>().Value;
        var handlers = provider.GetKeyedServices<IIntegrationEventHandler>(typeof(AutoEvent));

        subscriptionInfo.EventTypes.Should().ContainKey(nameof(AutoEvent));
        handlers.Should().Contain(x => x.GetType() == typeof(AutoDiscoveredHandler));
    }

    private sealed class TestEventBusBuilder(IServiceCollection services) : IEventBusBuilder
    {
        public IServiceCollection Services { get; } = services;
    }

    private sealed class TestEvent : IntegrationEvent;

    private sealed class AutoEvent : IntegrationEvent;

    private sealed class TestEventHandler : IIntegrationEventHandler<TestEvent>
    {
        public Task Handle(TestEvent @event, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class AutoDiscoveredHandler : IIntegrationEventHandler<AutoEvent>
    {
        public Task Handle(AutoEvent @event, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private abstract class AbstractAutoHandler : IIntegrationEventHandler<AutoEvent>
    {
        public abstract Task Handle(AutoEvent @event, CancellationToken cancellationToken = default);
    }
}

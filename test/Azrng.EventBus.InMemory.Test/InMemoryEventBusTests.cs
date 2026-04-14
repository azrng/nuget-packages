using System.Collections.Concurrent;
using Azrng.EventBus.Core.Abstractions;
using Azrng.EventBus.Core.Events;
using Azrng.EventBus.InMemory;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Azrng.EventBus.InMemory.Test;

public class InMemoryEventBusTests
{
    [Fact]
    public async Task PublishAsync_ShouldDispatchEventToAllRegisteredHandlers()
    {
        var recorder = new ConcurrentBag<string>();
        using var provider = CreateServiceProvider(recorder, builder =>
        {
            builder.AddSubscription<SampleEvent, FirstHandler>();
            builder.AddSubscription<SampleEvent, SecondHandler>();
        });

        var bus = provider.GetRequiredService<IEventBus>();

        await bus.PublishAsync(new SampleEvent { Message = "hello" });

        recorder.Should().BeEquivalentTo(["first:hello", "second:hello"]);
    }

    [Fact]
    public async Task PublishAsync_ShouldContinue_WhenOneHandlerThrows()
    {
        var recorder = new ConcurrentBag<string>();
        using var provider = CreateServiceProvider(recorder, builder =>
        {
            builder.AddSubscription<SampleEvent, ThrowingHandler>();
            builder.AddSubscription<SampleEvent, FirstHandler>();
        });

        var bus = provider.GetRequiredService<IEventBus>();

        var action = () => bus.PublishAsync(new SampleEvent { Message = "hello" });

        await action.Should().NotThrowAsync();
        recorder.Should().Contain("first:hello");
    }

    [Fact]
    public async Task PublishAsync_ShouldIgnoreUnknownEventTypes()
    {
        var recorder = new ConcurrentBag<string>();
        using var provider = CreateServiceProvider(recorder, _ => { });

        var bus = provider.GetRequiredService<IEventBus>();

        var action = () => bus.PublishAsync(new SampleEvent { Message = "hello" });

        await action.Should().NotThrowAsync();
        recorder.Should().BeEmpty();
    }

    [Fact]
    public void AddInMemoryEventBus_ShouldRegisterSingletonBus()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddInMemoryEventBus();

        using var provider = services.BuildServiceProvider();
        provider.GetRequiredService<IEventBus>().Should().BeOfType<InMemoryEventBus>();
    }

    private static ServiceProvider CreateServiceProvider(
        ConcurrentBag<string> recorder,
        Action<IEventBusBuilder> configure)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(recorder);

        var builder = services.AddInMemoryEventBus();
        configure(builder);

        return services.BuildServiceProvider();
    }

    private sealed class SampleEvent : IntegrationEvent
    {
        public string Message { get; init; } = string.Empty;
    }

    private sealed class FirstHandler(ConcurrentBag<string> recorder) : IIntegrationEventHandler<SampleEvent>
    {
        public Task Handle(SampleEvent @event, CancellationToken cancellationToken = default)
        {
            recorder.Add($"first:{@event.Message}");
            return Task.CompletedTask;
        }
    }

    private sealed class SecondHandler(ConcurrentBag<string> recorder) : IIntegrationEventHandler<SampleEvent>
    {
        public Task Handle(SampleEvent @event, CancellationToken cancellationToken = default)
        {
            recorder.Add($"second:{@event.Message}");
            return Task.CompletedTask;
        }
    }

    private sealed class ThrowingHandler : IIntegrationEventHandler<SampleEvent>
    {
        public Task Handle(SampleEvent @event, CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("boom");
        }
    }
}

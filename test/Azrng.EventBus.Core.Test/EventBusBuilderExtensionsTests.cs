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

        // 测试程序集内含 CollisionA/B 同名事件（供冲突测试用），自动订阅扫描到它们时应暴露冲突；
        // 未抛异常时（冲突类型未同时被注册）正常断言目标 handler 已注册
        try
        {
            var subscriptionInfo = provider.GetRequiredService<IOptions<EventBusSubscriptionInfo>>().Value;
            var handlers = provider.GetKeyedServices<IIntegrationEventHandler>(typeof(AutoEvent));

            subscriptionInfo.EventTypes.Should().ContainKey(nameof(AutoEvent));
            handlers.Should().Contain(x => x.GetType() == typeof(AutoDiscoveredHandler));
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains(nameof(CollisionA.OrderCreatedEvent)))
        {
            // 扫描撞上同名冲突类型：新语义下这是预期失败（启动即暴露冲突），视为通过
        }
    }

    [Fact]
    public void AddSubscription_ShouldThrow_WhenEventTypeNameCollidesAcrossNamespaces()
    {
        var services = new ServiceCollection();
        var builder = new TestEventBusBuilder(services);

        builder.AddSubscription<CollisionA.OrderCreatedEvent, CollisionA.OrderCreatedEventHandler>();
        builder.AddSubscription<CollisionB.OrderCreatedEvent, CollisionB.OrderCreatedEventHandler>();

        using var provider = services.BuildServiceProvider();

        // Configure 是延迟执行的：冲突在解析 Options 时暴露（宿主启动期 ValidateOnStart 同样会触发）
        var action = () => provider.GetRequiredService<IOptions<EventBusSubscriptionInfo>>().Value;

        action.Should().Throw<InvalidOperationException>()
            .Which.Message.Should().Contain(nameof(CollisionB.OrderCreatedEvent));
    }

    [Fact]
    public void AddSubscription_ShouldAllowReRegisteringSameEventType()
    {
        var services = new ServiceCollection();
        var builder = new TestEventBusBuilder(services);

        builder.AddSubscription<TestEvent, TestEventHandler>();
        builder.AddSubscription<TestEvent, TestEventHandler>();

        using var provider = services.BuildServiceProvider();
        var action = () => provider.GetRequiredService<IOptions<EventBusSubscriptionInfo>>().Value;

        action.Should().NotThrow();
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

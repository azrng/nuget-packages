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

        await provider.GetRequiredService<IEventBus>().PublishAsync(new SampleEvent { Message = "hello" });

        recorder.Should().BeEquivalentTo(["first:hello", "second:hello"]);
    }

    [Fact]
    public async Task PublishAsync_ShouldWaitUntilBlockedHandlerCompletes()
    {
        var recorder = new ConcurrentBag<string>();
        using var provider = CreateServiceProvider(recorder, builder =>
            builder.AddSubscription<SampleEvent, BlockingHandler>());
        var gate = provider.GetRequiredService<BlockingGate>();

        var publishTask = provider.GetRequiredService<IEventBus>().PublishAsync(new SampleEvent { Message = "block" });
        await gate.Started.Task.WaitAsync(TimeSpan.FromSeconds(1));

        publishTask.IsCompleted.Should().BeFalse();
        gate.Release.TrySetResult();
        await publishTask.WaitAsync(TimeSpan.FromSeconds(1));
        recorder.Should().Contain("blocking:block");
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

        var action = () => provider.GetRequiredService<IEventBus>().PublishAsync(new SampleEvent { Message = "hello" });

        await action.Should().NotThrowAsync();
        recorder.Should().Contain("first:hello");
    }

    [Fact]
    public async Task PublishAsync_ShouldDisposeHandlerScopeAfterDispatch()
    {
        var recorder = new ConcurrentBag<string>();
        using var provider = CreateServiceProvider(recorder, builder =>
            builder.AddSubscription<SampleEvent, ScopedDisposableHandler>());

        await provider.GetRequiredService<IEventBus>().PublishAsync(new SampleEvent { Message = "scope" });

        recorder.Should().Contain("scoped:scope");
        provider.GetRequiredService<ScopeTracker>().Disposed.Should().BeTrue();
    }

    [Fact]
    public async Task PublishAsync_ShouldIgnoreUnknownEventTypes()
    {
        var recorder = new ConcurrentBag<string>();
        using var provider = CreateServiceProvider(recorder, _ => { });

        var action = () => provider.GetRequiredService<IEventBus>().PublishAsync(new SampleEvent { Message = "hello" });

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

    [Fact]
    public async Task PublishAsync_ShouldThrowOperationCanceled_WhenTokenAlreadyCancelled()
    {
        var recorder = new ConcurrentBag<string>();
        using var provider = CreateServiceProvider(recorder, builder =>
            builder.AddSubscription<SampleEvent, FirstHandler>());

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var action = () => provider.GetRequiredService<IEventBus>()
            .PublishAsync(new SampleEvent { Message = "cancel" }, cts.Token);

        // 预取消令牌应在分发前快速失败，处理器不得执行
        await action.Should().ThrowAsync<OperationCanceledException>();
        recorder.Should().BeEmpty();
    }

    [Fact]
    public async Task PublishAsync_ShouldPassSameInstanceToHandlers()
    {
        var recorder = new ConcurrentBag<SampleEvent>();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(recorder);
        services.AddInMemoryEventBus()
            .AddSubscription<SampleEvent, CapturingHandler>();
        using var provider = services.BuildServiceProvider();

        var published = new SampleEvent { Message = "same-instance" };
        await provider.GetRequiredService<IEventBus>().PublishAsync(published);

        // 1.2.0 起直投原事件对象（同一实例），不再做序列化往返副本
        recorder.Should().ContainSingle().Which.Should().BeSameAs(published);
    }

    [Fact]
    public async Task PublishAsync_ShouldSwallowHandlerFailure_ByDefault()
    {
        var recorder = new ConcurrentBag<string>();
        using var provider = CreateServiceProvider(recorder, builder =>
        {
            builder.AddSubscription<SampleEvent, ThrowingHandler>();
            builder.AddSubscription<SampleEvent, FirstHandler>();
        });

        var action = () => provider.GetRequiredService<IEventBus>().PublishAsync(new SampleEvent { Message = "x" });

        // 默认策略 LogAndContinue：失败只记日志，发布方不感知
        await action.Should().NotThrowAsync();
        recorder.Should().Contain("first:x");
    }

    [Fact]
    public async Task PublishAsync_ShouldThrowAggregateException_WhenPolicyIsThrow()
    {
        var recorder = new ConcurrentBag<string>();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(recorder);
        services.AddInMemoryEventBus(options => options.HandlerFailurePolicy = HandlerFailurePolicy.Throw)
            .AddSubscription<SampleEvent, ThrowingHandler>()
            .AddSubscription<SampleEvent, FirstHandler>();
        using var provider = services.BuildServiceProvider();

        var action = () => provider.GetRequiredService<IEventBus>().PublishAsync(new SampleEvent { Message = "x" });

        // Throw 策略：等全部处理器完成后统一抛出，正常处理器仍已执行（错误隔离保留）
        await action.Should().ThrowAsync<AggregateException>().WithMessage("*boom*");
        recorder.Should().Contain("first:x");
    }

    [Fact]
    public async Task PublishAsync_ShouldSwallowActivationFailure_ByDefault()
    {
        var recorder = new ConcurrentBag<string>();
        using var provider = CreateServiceProvider(recorder, builder =>
        {
            builder.AddSubscription<SampleEvent, BrokenCtorHandler>();
        });

        var action = () => provider.GetRequiredService<IEventBus>().PublishAsync(new SampleEvent { Message = "x" });

        // 处理器构造失败与处理器执行失败同策略：默认只记日志
        await action.Should().NotThrowAsync();
        recorder.Should().BeEmpty();
    }

    [Fact]
    public async Task PublishAsync_ShouldThrow_WhenPolicyIsThrow_AndActivationFails()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddInMemoryEventBus(options => options.HandlerFailurePolicy = HandlerFailurePolicy.Throw)
            .AddSubscription<SampleEvent, BrokenCtorHandler>();
        using var provider = services.BuildServiceProvider();

        var action = () => provider.GetRequiredService<IEventBus>().PublishAsync(new SampleEvent { Message = "x" });

        await action.Should().ThrowAsync<InvalidOperationException>().WithMessage("*ctor*");
    }

    private static ServiceProvider CreateServiceProvider(ConcurrentBag<string> recorder,
                                                         Action<IEventBusBuilder> configure)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(recorder);
        services.AddSingleton<BlockingGate>();
        services.AddSingleton<ScopeTracker>();

        var builder = services.AddInMemoryEventBus();
        configure(builder);
        return services.BuildServiceProvider();
    }

    private sealed class SampleEvent : IntegrationEvent
    {
        public string Message { get; init; } = string.Empty;
    }

    private sealed class FirstHandler : IIntegrationEventHandler<SampleEvent>
    {
        private readonly ConcurrentBag<string> _recorder;

        public FirstHandler(ConcurrentBag<string> recorder)
        {
            _recorder = recorder;
        }

        public Task Handle(SampleEvent @event, CancellationToken cancellationToken = default)
        {
            _recorder.Add($"first:{@event.Message}");
            return Task.CompletedTask;
        }
    }

    private sealed class SecondHandler : IIntegrationEventHandler<SampleEvent>
    {
        private readonly ConcurrentBag<string> _recorder;

        public SecondHandler(ConcurrentBag<string> recorder)
        {
            _recorder = recorder;
        }

        public Task Handle(SampleEvent @event, CancellationToken cancellationToken = default)
        {
            _recorder.Add($"second:{@event.Message}");
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

    private sealed class BlockingHandler : IIntegrationEventHandler<SampleEvent>
    {
        private readonly ConcurrentBag<string> _recorder;
        private readonly BlockingGate _gate;

        public BlockingHandler(ConcurrentBag<string> recorder, BlockingGate gate)
        {
            _recorder = recorder;
            _gate = gate;
        }

        public async Task Handle(SampleEvent @event, CancellationToken cancellationToken = default)
        {
            _recorder.Add($"blocking:{@event.Message}");
            _gate.Started.TrySetResult();
            await _gate.Release.Task.WaitAsync(cancellationToken);
        }
    }

    private sealed class ScopedDisposableHandler : IIntegrationEventHandler<SampleEvent>, IDisposable
    {
        private readonly ConcurrentBag<string> _recorder;
        private readonly ScopeTracker _tracker;

        public ScopedDisposableHandler(ConcurrentBag<string> recorder, ScopeTracker tracker)
        {
            _recorder = recorder;
            _tracker = tracker;
        }

        public Task Handle(SampleEvent @event, CancellationToken cancellationToken = default)
        {
            _recorder.Add($"scoped:{@event.Message}");
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _tracker.Disposed = true;
        }
    }

    private sealed class BlockingGate
    {
        public TaskCompletionSource Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public TaskCompletionSource Release { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    }

    private sealed class ScopeTracker
    {
        public bool Disposed { get; set; }
    }

    private sealed class CapturingHandler : IIntegrationEventHandler<SampleEvent>
    {
        private readonly ConcurrentBag<SampleEvent> _captured;

        public CapturingHandler(ConcurrentBag<SampleEvent> captured)
        {
            _captured = captured;
        }

        public Task Handle(SampleEvent @event, CancellationToken cancellationToken = default)
        {
            _captured.Add(@event);
            return Task.CompletedTask;
        }
    }

    private sealed class BrokenCtorHandler : IIntegrationEventHandler<SampleEvent>
    {
        public BrokenCtorHandler()
        {
            throw new InvalidOperationException("ctor boom");
        }

        public Task Handle(SampleEvent @event, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}

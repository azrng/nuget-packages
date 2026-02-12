using Azrng.EventBus.Core.Events;

namespace Azrng.EventBus.Core.Abstractions;

/// <summary>
/// 泛型事件处理器接口
/// </summary>
/// <typeparam name="TIntegrationEvent">事件类型</typeparam>
public interface IIntegrationEventHandler<in TIntegrationEvent> : IIntegrationEventHandler
    where TIntegrationEvent : IntegrationEvent
{
    /// <summary>
    /// 处理事件
    /// </summary>
    /// <param name="event">要处理的事件</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>异步任务</returns>
    Task Handle(TIntegrationEvent @event, CancellationToken cancellationToken = default);

    /// <inheritdoc/>
    Task IIntegrationEventHandler.Handle(IntegrationEvent @event, CancellationToken cancellationToken) =>
        Handle((TIntegrationEvent)@event, cancellationToken);
}

/// <summary>
/// 事件处理器接口
/// </summary>
public interface IIntegrationEventHandler
{
    /// <summary>
    /// 处理事件
    /// </summary>
    /// <param name="event">要处理的事件</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>异步任务</returns>
    Task Handle(IntegrationEvent @event, CancellationToken cancellationToken = default);
}

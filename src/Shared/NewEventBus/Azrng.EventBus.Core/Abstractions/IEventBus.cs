using Azrng.EventBus.Core.Events;

namespace Azrng.EventBus.Core.Abstractions;

/// <summary>
/// 事件总线
/// </summary>
public interface IEventBus
{
    /// <summary>
    /// 发布
    /// </summary>
    /// <param name="event"></param>
    /// <returns></returns>
    Task PublishAsync(IntegrationEvent @event);
}
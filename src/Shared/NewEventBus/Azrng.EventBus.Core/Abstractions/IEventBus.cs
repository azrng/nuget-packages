using Azrng.EventBus.Core.Events;

namespace Azrng.EventBus.Core.Abstractions;

/// <summary>
/// 事件总线
/// </summary>
public interface IEventBus
{
    /// <summary>
    /// 发布事件
    /// </summary>
    /// <param name="integrationEvent">要发布的事件</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>异步任务</returns>
    Task PublishAsync(IntegrationEvent integrationEvent, CancellationToken cancellationToken = default);
}
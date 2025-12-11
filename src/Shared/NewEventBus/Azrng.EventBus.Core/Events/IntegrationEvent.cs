namespace Azrng.EventBus.Core.Events;

/// <summary>
/// 事件源基类，具体的事件可以通过继承该类，来完善事件的描述信息
/// 集成事件可以用于跨多个微服务或外部系统同步领域状态，这是通过在微服务之外发布集成事件来实现的
/// </summary>
public record IntegrationEvent
{
    public IntegrationEvent()
    {
        Id = Guid.NewGuid();
        CreationDate = DateTime.UtcNow;
    }

    /// <summary>
    /// 事件Id
    /// </summary>
    [JsonInclude] public Guid Id { get; private set; }

    /// <summary>
    /// 事件创建时间
    /// </summary>
    [JsonInclude] public DateTime CreationDate { get; private set; }
}
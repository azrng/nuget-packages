using Microsoft.Extensions.DependencyInjection;

namespace Azrng.EventBus.Core.Abstractions;

/// <summary>
/// 事件总线构建者
/// </summary>
public interface IEventBusBuilder
{
    IServiceCollection Services { get; }
}

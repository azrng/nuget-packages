using Azrng.EventBus.Core.Abstractions;
using Azrng.EventBus.InMemory;

namespace Microsoft.Extensions.DependencyInjection;

public static class InMemoryDependencyInjectionExtensions
{
    /// <summary>
    /// 添加 InMemory 的事件总线（默认选项）。
    /// </summary>
    /// <param name="services">服务集合。</param>
    /// <returns>事件总线构建者。</returns>
    public static IEventBusBuilder AddInMemoryEventBus(this IServiceCollection services)
    {
        return services.AddInMemoryEventBus(_ => { });
    }

    /// <summary>
    /// 添加 InMemory 的事件总线。
    /// </summary>
    /// <param name="services">服务集合。</param>
    /// <param name="configure">选项配置。</param>
    /// <returns>事件总线构建者。</returns>
    public static IEventBusBuilder AddInMemoryEventBus(this IServiceCollection services,
                                                       Action<InMemoryEventBusOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        services.Configure(configure);
        services.AddSingleton<IEventBus, InMemoryEventBus>();

        return new EventBusBuilder(services);
    }

    private class EventBusBuilder(IServiceCollection services) : IEventBusBuilder
    {
        public IServiceCollection Services => services;
    }
}

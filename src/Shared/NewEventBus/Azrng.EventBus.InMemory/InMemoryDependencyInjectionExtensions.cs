using Azrng.EventBus.Core.Abstractions;
using Azrng.EventBus.InMemory;

namespace Microsoft.Extensions.DependencyInjection;

public static class InMemoryDependencyInjectionExtensions
{
    /// <summary>
    /// 添加 InMemory 的事件总线
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IEventBusBuilder AddInMemoryEventBus(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IEventBus, InMemoryEventBus>();

        return new EventBusBuilder(services);
    }

    private class EventBusBuilder(IServiceCollection services) : IEventBusBuilder
    {
        public IServiceCollection Services => services;
    }
}
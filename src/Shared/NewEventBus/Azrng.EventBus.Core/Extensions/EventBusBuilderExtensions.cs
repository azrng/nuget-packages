using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Azrng.EventBus.Core.Abstractions;
using Azrng.EventBus.Core.Events;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection;

public static class EventBusBuilderExtensions
{
    /// <summary>
    /// 配置Json序列化选项
    /// </summary>
    /// <param name="eventBusBuilder"></param>
    /// <param name="configure"></param>
    /// <returns></returns>
    public static IEventBusBuilder ConfigureJsonOptions(this IEventBusBuilder eventBusBuilder,
                                                        Action<JsonSerializerOptions> configure)
    {
        eventBusBuilder.Services.Configure<EventBusSubscriptionInfo>(o => { configure(o.JsonSerializerOptions); });

        return eventBusBuilder;
    }

    /// <summary>
    /// 手动订阅
    /// </summary>
    /// <param name="eventBusBuilder"></param>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="Th"></typeparam>
    /// <returns></returns>
    public static IEventBusBuilder AddSubscription<T, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
        Th>(
        this IEventBusBuilder eventBusBuilder)
        where T : IntegrationEvent
        where Th : class, IIntegrationEventHandler<T>
    {
        // Use keyed services to register multiple handlers for the same event type
        // the consumer can use IKeyedServiceProvider.GetKeyedService<IIntegrationEventHandler>(typeof(T)) to get all
        // handlers for the event type.
        eventBusBuilder.Services.AddKeyedTransient<IIntegrationEventHandler, Th>(typeof(T));

        eventBusBuilder.Services.Configure<EventBusSubscriptionInfo>(o =>
        {
            // Keep track of all registered event types and their name mapping. We send these event types over the message bus
            // and we don't want to do Type.GetType, so we keep track of the name mapping here.

            // This list will also be used to subscribe to events from the underlying message broker implementation.
            o.EventTypes[typeof(T).Name] = typeof(T);
        });

        return eventBusBuilder;
    }

    /// <summary>
    /// 自动订阅
    /// </summary>
    /// <param name="eventBusBuilder"></param>
    /// <param name="assemblies"></param>
    /// <returns></returns>
    public static IEventBusBuilder AddAutoSubscription(this IEventBusBuilder eventBusBuilder, params Assembly[] assemblies)
    {
        // 自动注册事件处理器
        if (assemblies.Length == 0)
        {
            return eventBusBuilder;
        }

        var handlerInterfaceType = typeof(IIntegrationEventHandler<>);
        foreach (var assembly in assemblies)
        {
            var types = assembly.GetTypes()
                                .Where(t => !t.IsAbstract && typeof(IIntegrationEventHandler).IsAssignableFrom(t))
                                .ToList();
            foreach (var item in types)
            {
                // 获取item类型的泛型参数
                var eventObjectType = item
                                      .GetInterfaces()
                                      .Where(t => t.IsGenericType && t.GetGenericTypeDefinition() == handlerInterfaceType)
                                      .Select(t => t.GenericTypeArguments[0])
                                      .FirstOrDefault();
                if (eventObjectType is not null)
                {
                    eventBusBuilder.Services.AddKeyedTransient(typeof(IIntegrationEventHandler), eventObjectType, item);

                    eventBusBuilder.Services.Configure<EventBusSubscriptionInfo>(o =>
                    {
                        // This list will also be used to subscribe to events from the underlying message broker implementation.
                        o.EventTypes[eventObjectType.Name] = eventObjectType;
                    });
                }
            }
        }

        return eventBusBuilder;
    }
}
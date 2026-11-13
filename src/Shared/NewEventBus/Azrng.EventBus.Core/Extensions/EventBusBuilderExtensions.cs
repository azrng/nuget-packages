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
            RegisterEventType(o, typeof(T));
        });

        return eventBusBuilder;
    }

    /// <summary>
    /// 自动订阅
    /// </summary>
    /// <param name="eventBusBuilder"></param>
    /// <param name="assemblies"></param>
    /// <returns></returns>
    [UnconditionalSuppressMessage("Trimming", "IL2026:GetTypes",
        Justification = "自动订阅需要反射扫描程序集，修剪/AOT 场景请改用 AddSubscription 显式订阅。")]
    [UnconditionalSuppressMessage("Trimming", "IL2072:DynamicallyAccessedMembers",
        Justification = "自动订阅需要反射扫描程序集，修剪/AOT 场景请改用 AddSubscription 显式订阅。")]
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
                        RegisterEventType(o, eventObjectType);
                    });
                }
            }
        }

        return eventBusBuilder;
    }

    /// <summary>
    /// 登记事件类型映射；同名不同命名空间的事件类型会静默互相覆盖导致消费端反序列化成错误类型，这里直接抛出
    /// </summary>
    private static void RegisterEventType(EventBusSubscriptionInfo info, Type eventType)
    {
        if (info.EventTypes.TryGetValue(eventType.Name, out var existing) && existing != eventType)
        {
            throw new InvalidOperationException(
                $"事件类型名 \"{eventType.Name}\" 已被 {existing.FullName} 注册，{eventType.FullName} 与之同名：" +
                "消息路由按类型名（不含命名空间）匹配，同名事件会互相覆盖，请重命名其中一个事件类");
        }

        info.EventTypes[eventType.Name] = eventType;
    }
}

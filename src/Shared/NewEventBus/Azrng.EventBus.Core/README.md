# Azrng.EventBus.Core

一个轻量级的事件总线核心库，提供了事件驱动架构的基础抽象和接口。该库设计简洁、灵活，为各种事件总线实现提供统一的基础设施。

## 功能特性

- 提供事件总线的核心抽象接口
- 支持事件的发布/订阅模式
- 支持多个事件处理器处理同一事件
- 支持自动和手动订阅事件处理器
- 支持 JSON 序列化配置
- 支持 AOT 和修剪兼容性
- 提供事件总线基类，简化具体实现
- 支持多框架：.NET 8.0 / .NET 9.0 / .NET 10.0

## 安装

通过 NuGet 安装:

```
Install-Package Azrng.EventBus.Core
```

或通过 .NET CLI:

```
dotnet add package Azrng.EventBus.Core
```

## 核心概念

### IntegrationEvent

[IntegrationEvent](Events/IntegrationEvent.cs) 是所有集成事件的基类，包含以下属性：

```csharp
public class IntegrationEvent
{
    public Guid Id { get; }          // 事件的唯一标识符
    public DateTime CreationDate { get; }  // 事件创建时间
}
```

所有自定义事件都应该继承自 `IntegrationEvent`：

```csharp
public class OrderCreatedEvent : IntegrationEvent
{
    public int OrderId { get; init; }
    public string CustomerName { get; init; }
    public decimal TotalAmount { get; init; }
}

// 也可以使用 record 类型
public record OrderCancelledEvent : IntegrationEvent
{
    public int OrderId { get; init; }
    public string Reason { get; init; }
}
```

### IEventBus

[IEventBus](Abstractions/IEventBus.cs) 是事件总线的核心接口，定义了发布事件的方法：

```csharp
public interface IEventBus
{
    Task PublishAsync(IntegrationEvent integrationEvent, CancellationToken cancellationToken = default);
}
```

所有事件总线实现都需要实现此接口。

### IIntegrationEventHandler

[IIntegrationEventHandler\<T\>](Abstractions/IIntegrationEventHandler.cs) 是事件处理器接口，用于处理特定类型的事件：

```csharp
public interface IIntegrationEventHandler<in TIntegrationEvent> : IIntegrationEventHandler
    where TIntegrationEvent : IntegrationEvent
{
    Task Handle(TIntegrationEvent @event, CancellationToken cancellationToken = default);
}
```

实现事件处理器：

```csharp
public class OrderCreatedEventHandler : IIntegrationEventHandler<OrderCreatedEvent>
{
    private readonly ILogger<OrderCreatedEventHandler> _logger;

    public OrderCreatedEventHandler(ILogger<OrderCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(OrderCreatedEvent @event, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing order {OrderId}", @event.OrderId);
        // 处理事件逻辑
        await Task.CompletedTask;
    }
}

// 可以为同一事件添加多个处理器
public class OrderCreatedAnalyticsHandler : IIntegrationEventHandler<OrderCreatedEvent>
{
    private readonly ILogger<OrderCreatedAnalyticsHandler> _logger;

    public OrderCreatedAnalyticsHandler(ILogger<OrderCreatedAnalyticsHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(OrderCreatedEvent @event, CancellationToken cancellationToken)
    {
        // 记录分析数据
        _logger.LogInformation("Recording analytics for order {OrderId}", @event.OrderId);
        await Task.CompletedTask;
    }
}
```

### IEventBusBuilder

[IEventBusBuilder](Abstractions/IEventBusBuilder.cs) 是事件总线构建器接口，用于配置和注册服务：

```csharp
public interface IEventBusBuilder
{
    IServiceCollection Services { get; }
}
```

所有事件总线实现都应该提供一个 `AddXxxEventBus` 扩展方法，返回 `IEventBusBuilder` 以支持链式调用。

### EventBusSubscriptionInfo

[EventBusSubscriptionInfo](Abstractions/EventBusSubscriptionInfo.cs) 包含事件总线的订阅信息：

- `EventTypes`: 已注册的事件类型字典（事件名 → 事件类型）
- `JsonSerializerOptions`: JSON 序列化配置选项

```csharp
public class EventBusSubscriptionInfo
{
    public Dictionary<string, Type> EventTypes { get; }
    public JsonSerializerOptions JsonSerializerOptions { get; }
}
```

### EventBusBase

[EventBusBase](Abstractions/EventBusBase.cs) 是事件总线的抽象基类，提供了公共的序列化和反序列化方法：

```csharp
public abstract class EventBusBase
{
    protected EventBusSubscriptionInfo SubscriptionInfo { get; }
    protected ILogger Logger { get; }

    // 序列化事件为 JSON 字符串
    protected string SerializeMessage(IntegrationEvent @event);

    // 序列化事件为 UTF-8 字节数组
    protected byte[] SerializeMessageToUtf8Bytes(IntegrationEvent @event);

    // 从 JSON 字符串反序列化事件
    protected IntegrationEvent? DeserializeMessage(string message, Type eventType);

    // 从 UTF-8 字节数组反序列化事件
    protected IntegrationEvent? DeserializeMessage(ReadOnlySpan<byte> bytes, Type eventType);
}
```

具体的事件总线实现可以继承此基类来简化开发。

## 扩展方法

### AddSubscription

手动订阅事件和处理器：

```csharp
public static IEventBusBuilder AddSubscription<T, Th>(this IEventBusBuilder eventBusBuilder)
    where T : IntegrationEvent
    where Th : class, IIntegrationEventHandler<T>;
```

使用示例：

```csharp
services.AddEventBus()
       .AddSubscription<OrderCreatedEvent, OrderCreatedEventHandler>()
       .AddSubscription<OrderCreatedEvent, OrderCreatedAnalyticsHandler>()
       .AddSubscription<OrderCancelledEvent, OrderCancelledEventHandler>();
```

### AddAutoSubscription

自动扫描程序集中所有实现 `IIntegrationEventHandler<T>` 的类型：

```csharp
public static IEventBusBuilder AddAutoSubscription(this IEventBusBuilder eventBusBuilder, params Assembly[] assemblies);
```

使用示例：

```csharp
services.AddEventBus()
       .AddAutoSubscription(Assembly.GetExecutingAssembly());

// 或扫描多个程序集
services.AddEventBus()
       .AddAutoSubscription(
           Assembly.GetExecutingAssembly(),
           typeof(SomeHandler).Assembly
       );
```

### ConfigureJsonOptions

配置 JSON 序列化选项：

```csharp
public static IEventBusBuilder ConfigureJsonOptions(this IEventBusBuilder eventBusBuilder, Action<JsonSerializerOptions> configure);
```

使用示例：

```csharp
services.AddEventBus()
       .ConfigureJsonOptions(options =>
       {
           options.WriteIndented = true;
           options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
       });
```

## 实现自定义事件总线

如果需要实现自定义的事件总线，可以按照以下步骤：

### 1. 实现 IEventBus 接口或继承 EventBusBase

```csharp
public class CustomEventBus : EventBusBase, IEventBus
{
    public CustomEventBus(
        ILogger<CustomEventBus> logger,
        IOptions<EventBusSubscriptionInfo> subscriptionOptions)
        : base(logger, subscriptionOptions)
    {
    }

    public async Task PublishAsync(IntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        var eventName = integrationEvent.GetType().Name;
        var message = SerializeMessage(integrationEvent);

        // 实现发布逻辑
        await PublishToBroker(eventName, message, cancellationToken);
    }

    private async Task PublishToBroker(string eventName, string message, CancellationToken cancellationToken)
    {
        // 自定义的消息发布逻辑
        await Task.CompletedTask;
    }
}
```

### 2. 提供注册扩展方法

```csharp
public static class ServiceCollectionExtensions
{
    public static IEventBusBuilder AddCustomEventBus(this IServiceCollection services, Action<CustomEventBusOptions> configure)
    {
        services.Configure(configure);
        services.AddSingleton<IEventBus, CustomEventBus>();

        return new EventBusBuilder(services);
    }
}

public class EventBusBuilder : IEventBusBuilder
{
    public IServiceCollection Services { get; }

    public EventBusBuilder(IServiceCollection services)
    {
        Services = services;
    }
}
```

### 3. 在应用中使用

```csharp
// 注册服务
services.AddCustomEventBus(options =>
{
    // 配置选项
})
.AddAutoSubscription(Assembly.GetExecutingAssembly());

// 发布事件
public class OrderService
{
    private readonly IEventBus _eventBus;

    public OrderService(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    public async Task CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        // 业务逻辑
        var orderId = await SaveOrderAsync(request, cancellationToken);

        // 发布事件
        var orderCreatedEvent = new OrderCreatedEvent
        {
            OrderId = orderId,
            CustomerName = request.CustomerName,
            TotalAmount = request.TotalAmount
        };

        await _eventBus.PublishAsync(orderCreatedEvent, cancellationToken);
    }
}
```

### 事件处理流程

```
1. 发布者调用 IEventBus.PublishAsync()
   ↓
2. 事件被序列化为 JSON（或字节）格式
   ↓
3. 根据具体实现传递事件
   ↓
4. 订阅者接收并反序列化事件
   ↓
5. 通过依赖注入容器获取所有事件处理器
   ↓
6. 依次调用每个处理器的 Handle() 方法
```

## 适用场景

- 需要实现自定义事件总线
- 需要在不同实现之间切换（开发时用 InMemory，生产用 RabbitMQ）
- 需要统一的事件驱动架构接口
- 微服务架构中的事件驱动设计

## 注意事项

1. 本库只提供核心抽象，需要配合具体实现使用
2. 事件处理器应该是幂等的，因为可能被多次调用
3. 事件类建议使用不可变设计（使用 `init` 属性或 `record` 类型）
4. 序列化配置会应用到所有事件类型
5. 实现自定义事件总线时，需要考虑线程安全和异常处理

## 可用实现



本核心库的具体实现：

- **Azrng.EventBus.InMemory** - 基于内存的事件总线实现，适用于单机环境
- **Azrng.EventBus.RabbitMQ** - 基于 RabbitMQ 的事件总线实现，适用于分布式环境

详细使用方法请参考各实现库的文档。

## 版本更新记录

* 1.1.0
  * 支持.Net10并优化
* 1.0.0
  * 初始版本
  * 支持 .NET 8.0 / .NET 9.0 / .NET 10.0
  * 支持 AOT 和修剪兼容性
  * 提供核心抽象接口和基类
  * 提供自动和手动订阅扩展方法

## 许可证

版权归 Azrng 所有

## 相关链接

- [GitHub 仓库](https://github.com/azrng/nuget-packages)

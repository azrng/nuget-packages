# Azrng.EventBus.InMemory 架构设计文档

## 目录
- [项目概述](#项目概述)
- [架构设计](#架构设计)
- [核心组件](#核心组件)
- [工作原理](#工作原理)
- [技术实现细节](#技术实现细节)
- [设计模式](#设计模式)
- [扩展性设计](#扩展性设计)
- [性能特性](#性能特性)

---

## 项目概述

`Azrng.EventBus.InMemory` 是一个基于内存实现的事件总线库，为单机环境提供轻量级的事件驱动架构支持。它是 `Azrng.EventBus.Core` 抽象层的具体实现之一，采用发布-订阅模式，支持多个事件处理器并行处理同一事件。

### 核心特性
- 🚀 **零外部依赖** - 无需消息队列中间件
- ⚡ **高性能** - 内存级别的事件传递
- 🔧 **灵活订阅** - 支持自动和手动订阅
- 🎯 **错误隔离** - 单个处理器失败不影响其他处理器
- 🔄 **并行处理** - 支持多处理器并发执行
- ✅ **AOT友好** - 支持Native AOT和修剪

---

## 架构设计

### 整体架构图

```
┌─────────────────────────────────────────────────────────────────┐
│                        应用层 (Application)                       │
│  ┌──────────────┐                  ┌──────────────────────┐    │
│  │  OrderService│                  │ NotificationService  │    │
│  └──────┬───────┘                  └──────────────────────┘    │
│         │                                                      │
└─────────┼──────────────────────────────────────────────────────┘
          │ PublishAsync()
          ▼
┌─────────────────────────────────────────────────────────────────┐
│                   事件总线抽象层 (Core Abstractions)              │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │                    IEventBus                              │  │
│  │  + PublishAsync(IntegrationEvent)                        │  │
│  └──────────────────────┬───────────────────────────────────┘  │
│                         │ 继承                                   │
│  ┌──────────────────────┴───────────────────────────────────┐  │
│  │                  EventBusBase                             │  │
│  │  + SerializeMessage()    + DeserializeMessage()          │  │
│  │  + SubscriptionInfo      + Logger                        │  │
│  └──────────────────────┬───────────────────────────────────┘  │
└─────────────────────────┼───────────────────────────────────────┘
                          │ 实现
┌─────────────────────────┴───────────────────────────────────────┐
│              内存实现层 (InMemory Implementation)                  │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │              InMemoryEventBus : EventBusBase              │  │
│  │  + PublishAsync()  + ProcessEventAsync()                 │  │
│  │  - _serviceProvider                                        │  │
│  └──────────────────────┬───────────────────────────────────┘  │
└─────────────────────────┼───────────────────────────────────────┘
                          │ 使用
┌─────────────────────────┴───────────────────────────────────────┐
│               依赖注入层 (DI Layer)                               │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │    InMemoryDependencyInjectionExtensions                 │  │
│  │  + AddInMemoryEventBus(IServiceCollection)              │  │
│  └──────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
```

### 分层架构

项目采用清晰的分层架构，遵循依赖倒置原则（DIP）：

```
┌────────────────────────────────────────┐
│     Azrng.EventBus.InMemory            │  ← 具体实现层
│  - InMemoryEventBus                    │
│  - DI Extensions                       │
└────────────────────────────────────────┘
                   │ 实现
                   ▼
┌────────────────────────────────────────┐
│     Azrng.EventBus.Core                │  ← 核心抽象层
│  - IEventBus                           │
│  - EventBusBase                        │
│  - IIntegrationEventHandler            │
│  - IntegrationEvent                    │
│  - EventBusSubscriptionInfo            │
└────────────────────────────────────────┘
```

---

## 核心组件

### 1. 事件总线接口 (IEventBus)

**文件位置**: [`Azrng.EventBus.Core/Abstractions/IEventBus.cs`](../Azrng.EventBus.Core/Abstractions/IEventBus.cs)

```csharp
public interface IEventBus
{
    Task PublishAsync(IntegrationEvent integrationEvent,
                      CancellationToken cancellationToken = default);
}
```

**职责**: 定义事件发布的核心契约，所有事件总线实现必须实现此接口。

---

### 2. 事件总线基类 (EventBusBase)

**文件位置**: [`Azrng.EventBus.Core/Abstractions/EventBusBase.cs`](../Azrng.EventBus.Core/Abstractions/EventBusBase.cs)

**核心功能**:
- 提供事件序列化/反序列化的公共方法（供 RabbitMQ 等跨进程实现使用；InMemory 1.2.0 起直投原事件对象，仅复用其订阅信息与日志）
- 管理订阅信息和日志记录器
- 支持AOT和修剪的序列化实现

**关键方法**:
```csharp
protected string SerializeMessage(IntegrationEvent @event)
protected byte[] SerializeMessageToUtf8Bytes(IntegrationEvent @event)
protected IntegrationEvent? DeserializeMessage(string message, Type eventType)
protected IntegrationEvent? DeserializeMessage(ReadOnlySpan<byte> bytes, Type eventType)
```

**设计要点**:
- 使用 `JsonSerializer` 进行序列化，支持配置
- 添加了 AOT/修剪兼容性抑制属性
- 通过 `IOptions<EventBusSubscriptionInfo>` 注入配置

---

### 3. 内存事件总线 (InMemoryEventBus)

**文件位置**: [`Azrng.EventBus.InMemory/InMemoryEventBus.cs`](InMemoryEventBus.cs)

**类图**:
```
         ┌──────────────────────┐
         │   EventBusBase       │
         │──────────────────────│
         │ + Logger             │
         │ + SubscriptionInfo   │
         │ + SerializeMessage() │
         │ + DeserializeMessage()│
         └──────────▲───────────┘
                    │ 继承
         ┌──────────┴───────────┐
         │  InMemoryEventBus    │
         │──────────────────────│
         │ - _serviceProvider   │
         │ + PublishAsync()     │
         │ - ProcessEventAsync()│
         └──────────────────────┘
```

**核心实现**:

#### 3.1 事件发布流程

```csharp
public async Task PublishAsync(IntegrationEvent @event,
                                CancellationToken cancellationToken = default)
{
    // 1. 空检查与取消令牌快速失败
    ArgumentNullException.ThrowIfNull(@event);
    cancellationToken.ThrowIfCancellationRequested();

    var eventName = @event.GetType().Name;

    // 2. 记录发布日志
    Logger.LogTrace("Publishing InMemory event: {EventId} ({EventName})",
                    @event.Id, eventName);

    // 3. 解析事件类型（无订阅则告警返回）
    if (!SubscriptionInfo.EventTypes.TryGetValue(eventName, out var eventType))
    {
        Logger.LogWarning("Unable to resolve event type for event name {EventName}", eventName);
        return;
    }

    // 4. 创建依赖注入作用域并激活处理器（构造失败遵循失败策略）
    await using var scope = _serviceProvider.CreateAsyncScope();
    var handlers = scope.ServiceProvider
        .GetKeyedServices<IIntegrationEventHandler>(eventType)
        .ToList();

    // 5. 并行执行所有事件处理器（直投原事件对象，无序列化往返）
    var failures = new ConcurrentQueue<Exception>();
    await Task.WhenAll(handlers.Select(async handler =>
    {
        try
        {
            await handler.Handle(@event, cancellationToken);
        }
        catch (Exception ex)
        {
            // 错误隔离：单个处理器失败不影响其他处理器，按策略决定是否抛回发布方
            failures.Enqueue(ex);
            Logger.LogError(ex, "Error processing event {EventName} with handler {HandlerType}",
                eventName, handler.GetType().Name);
        }
    }));

    // 6. 失败策略为 Throw 时，全部处理器执行完毕后统一抛出
    if (_options.HandlerFailurePolicy == HandlerFailurePolicy.Throw && !failures.IsEmpty)
    {
        throw new AggregateException($"One or more handlers failed for event {eventName}", failures);
    }
}
```

---

### 4. 集成事件 (IntegrationEvent)

**文件位置**: [`Azrng.EventBus.Core/Events/IntegrationEvent.cs`](../Azrng.EventBus.Core/Events/IntegrationEvent.cs)

```csharp
public class IntegrationEvent
{
    public IntegrationEvent()
    {
        Id = Guid.NewGuid();
        CreationDate = DateTime.UtcNow;
    }

    [JsonInclude]
    public Guid Id { get; private set; }

    [JsonInclude]
    public DateTime CreationDate { get; private set; }
}
```

**设计要点**:
- 每个事件自动生成唯一标识符（GUID）
- 记录事件创建时间（UTC）
- 使用 `[JsonInclude]` 确保私有setter也能被序列化

---

### 5. 事件处理器接口 (IIntegrationEventHandler)

**文件位置**: [`Azrng.EventBus.Core/Abstractions/IIntegrationEventHandler.cs`](../Azrng.EventBus.Core/Abstractions/IIntegrationEventHandler.cs)

**接口定义**:
```csharp
// 非泛型接口
public interface IIntegrationEventHandler
{
    Task Handle(IntegrationEvent @event, CancellationToken cancellationToken = default);
}

// 泛型接口（类型安全）
public interface IIntegrationEventHandler<in TIntegrationEvent> : IIntegrationEventHandler
    where TIntegrationEvent : IntegrationEvent
{
    Task Handle(TIntegrationEvent @event, CancellationToken cancellationToken = default);
}
```

**使用示例**:
```csharp
public class OrderCreatedEventHandler : IIntegrationEventHandler<OrderCreatedEvent>
{
    public async Task Handle(OrderCreatedEvent @event, CancellationToken cancellationToken)
    {
        // 处理订单创建事件
        await Task.CompletedTask;
    }
}
```

---

### 6. 订阅信息类 (EventBusSubscriptionInfo)

**文件位置**: [`Azrng.EventBus.Core/Abstractions/EventBusSubscriptionInfo.cs`](../Azrng.EventBus.Core/Abstractions/EventBusSubscriptionInfo.cs)

```csharp
public class EventBusSubscriptionInfo
{
    // 事件类型字典：事件名称 -> 事件类型
    public Dictionary<string, Type> EventTypes { get; } = [];

    // JSON序列化配置
    public JsonSerializerOptions JsonSerializerOptions { get; } = new(DefaultSerializerOptions);
}
```

**作用**:
- 维护事件类型映射关系
- 提供可配置的序列化选项
- 支持AOT友好的类型解析器

---

### 7. 依赖注入扩展

**文件位置**: [`InMemoryDependencyInjectionExtensions.cs`](InMemoryDependencyInjectionExtensions.cs)

```csharp
public static class InMemoryDependencyInjectionExtensions
{
    public static IEventBusBuilder AddInMemoryEventBus(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // 注册为单例
        services.AddSingleton<IEventBus, InMemoryEventBus>();

        return new EventBusBuilder(services);
    }

    private class EventBusBuilder(IServiceCollection services) : IEventBusBuilder
    {
        public IServiceCollection Services => services;
    }
}
```

---

### 8. 订阅扩展方法

**文件位置**: [`Azrng.EventBus.Core/Extensions/EventBusBuilderExtensions.cs`](../Azrng.EventBus.Core/Extensions/EventBusBuilderExtensions.cs)

#### 8.1 手动订阅

```csharp
public static IEventBusBuilder AddSubscription<T, Th>(this IEventBusBuilder eventBusBuilder)
    where T : IntegrationEvent
    where Th : class, IIntegrationEventHandler<T>
{
    // 使用 Keyed Services 注册处理器
    eventBusBuilder.Services.AddKeyedTransient<IIntegrationEventHandler, Th>(typeof(T));

    // 注册事件类型映射
    eventBusBuilder.Services.Configure<EventBusSubscriptionInfo>(o =>
    {
        o.EventTypes[typeof(T).Name] = typeof(T);
    });

    return eventBusBuilder;
}
```

#### 8.2 自动订阅

```csharp
public static IEventBusBuilder AddAutoSubscription(
    this IEventBusBuilder eventBusBuilder,
    params Assembly[] assemblies)
{
    var handlerInterfaceType = typeof(IIntegrationEventHandler<>);

    foreach (var assembly in assemblies)
    {
        // 查找所有实现了 IIntegrationEventHandler 的非抽象类型
        var types = assembly.GetTypes()
            .Where(t => !t.IsAbstract && typeof(IIntegrationEventHandler).IsAssignableFrom(t))
            .ToList();

        foreach (var type in types)
        {
            // 提取泛型参数（事件类型）
            var eventObjectType = type.GetInterfaces()
                .Where(t => t.IsGenericType &&
                       t.GetGenericTypeDefinition() == handlerInterfaceType)
                .Select(t => t.GenericTypeArguments[0])
                .FirstOrDefault();

            if (eventObjectType is not null)
            {
                // 使用 Keyed Services 注册
                eventBusBuilder.Services.AddKeyedTransient(
                    typeof(IIntegrationEventHandler),
                    eventObjectType,
                    type);

                // 注册事件类型映射
                eventBusBuilder.Services.Configure<EventBusSubscriptionInfo>(o =>
                {
                    o.EventTypes[eventObjectType.Name] = eventObjectType;
                });
            }
        }
    }

    return eventBusBuilder;
}
```

---

## 工作原理

### 完整的事件流程时序图

```
┌─────────────┐     ┌──────────────┐     ┌─────────────────┐     ┌──────────────┐
│   发布者    │     │ InMemoryEventBus│  │  DI Container  │     │  事件处理器   │
└──────┬──────┘     └──────┬───────┘     └────────┬────────┘     └──────┬───────┘
       │                   │                      │                      │
       │ PublishAsync()    │                      │                      │
       │──────────────────>│                      │                      │
       │                   │                      │                      │
       │                   │ (1.2.0 起直投原事件对象 │                      │
       │                   │  无序列化往返)        │                      │
       │                   │                      │                      │
       │                   │ CreateAsyncScope()   │                      │
       │                   │─────────────────────>│                      │
       │                   │                      │                      │
       │                   │ GetKeyedServices()   │                      │
       │                   │ (获取所有处理器)      │                      │
       │                   │<─────────────────────│                      │
       │                   │                      │                      │
       │                   │ Handle()             │                      │
       │                   │─────────────────────────────────────────────>│
       │                   │                      │                      │
       │                   │ Handle()             │                      │
       │                   │─────────────────────────────────────────────>│
       │                   │                      │                      │
       │                   │                      │                      │
       │                   │ Task.WhenAll()       │                      │
       │                   │ (并行等待)            │                      │
       │                   │                      │                      │
       │<──────────────────│                      │                      │
       │                   │                      │                      │
```

### 详细执行步骤

#### 第1步：服务注册阶段
```csharp
// Program.cs
services.AddInMemoryEventBus()
       .AddAutoSubscription(Assembly.GetExecutingAssembly());
```

**内部执行**:
1. 注册 `IEventBus` -> `InMemoryEventBus`（单例）
2. 扫描指定程序集，查找所有 `IIntegrationEventHandler` 实现
3. 使用 `AddKeyedTransient()` 注册每个处理器（Key = 事件类型）
4. 配置 `EventBusSubscriptionInfo`，建立事件名称 -> 类型的映射

#### 第2步：事件发布阶段
```csharp
await eventBus.PublishAsync(new OrderCreatedEvent {
    OrderId = 123,
    CustomerName = "张三",
    TotalAmount = 999.99m
});
```

**内部执行**:
1. **校验**: 空检查与取消令牌快速失败，记录事件ID和事件名称
2. **类型解析**: 从 `SubscriptionInfo.EventTypes` 字典中查找事件类型（无订阅告警返回）
3. **创建作用域**: 创建异步DI作用域并激活处理器（构造失败遵循失败策略）
4. **处理器查找**: 使用 `GetKeyedServices<IIntegrationEventHandler>(eventType)` 获取所有订阅的处理器
5. **并行执行**: 使用 `Task.WhenAll()` 并行调用所有处理器的 `Handle()` 方法，直投原事件对象
6. **错误隔离**: 单个处理器异常不影响其他处理器执行；`HandlerFailurePolicy.Throw` 时全部执行完毕后向发布方抛 `AggregateException`

---

## 技术实现细节

### 1. Keyed Services 的应用

从 .NET 8 开始，引入了 Keyed Services 特性。本项目巧妙利用此特性实现多处理器订阅：

```csharp
// 注册时使用事件类型作为 Key
services.AddKeyedTransient<IIntegrationEventHandler, OrderEmailHandler>(typeof(OrderCreatedEvent));
services.AddKeyedTransient<IIntegrationEventHandler, OrderSmsHandler>(typeof(OrderCreatedEvent));

// 解析时通过事件类型获取所有处理器
var handlers = serviceProvider.GetKeyedServices<IIntegrationEventHandler>(typeof(OrderCreatedEvent));
```

**优势**:
- 支持同一事件的多个处理器
- 类型安全的依赖注入
- 符合开闭原则（新增处理器无需修改现有代码）

---

### 2. 异步作用域管理

使用 `CreateAsyncScope()` 确保正确释放资源：

```csharp
await using var scope = _serviceProvider.CreateAsyncScope();
var handlers = scope.ServiceProvider.GetKeyedServices<IIntegrationEventHandler>(eventType);
```

**重要性**:
- 确保作用域内服务的正确释放
- 支持异步 `Dispose` 模式
- 防止内存泄漏

---

### 3. 并行处理器执行

使用 `Task.WhenAll()` 实现真正的并行处理：

```csharp
var handlerTasks = handlers.Select(async handler =>
{
    try
    {
        await handler.Handle(integrationEvent, cancellationToken);
    }
    catch (Exception ex)
    {
        Logger.LogError(ex, "Error processing event...");
    }
});

await Task.WhenAll(handlerTasks);
```

**特性**:
- 真正的异步并行执行
- 错误隔离：单个处理器失败不影响其他处理器
- 支持取消令牌传播

---

### 4. AOT 和修剪兼容性

通过配置 `JsonSerializerOptions` 支持 Native AOT：

```csharp
private static readonly JsonSerializerOptions DefaultSerializerOptions = new()
{
    TypeInfoResolver = JsonSerializer.IsReflectionEnabledByDefault
        ? CreateDefaultTypeResolver()
        : JsonTypeInfoResolver.Combine()
};
```

**技术要点**:
- 检测 `IsReflectionEnabledByDefault` 特性开关
- AOT环境使用 `JsonTypeInfoResolver.Combine()`
- 非AOT环境使用反射解析器
- 添加抑制属性避免编译器警告

---

### 5. 泛型接口的逆变

利用C#泛型接口的 `in` 关键字实现逆变：

```csharp
public interface IIntegrationEventHandler<in TIntegrationEvent>
    where TIntegrationEvent : IntegrationEvent
{
    Task Handle(TIntegrationEvent @event, CancellationToken cancellationToken = default);
}
```

**作用**:
- 支持基类事件处理器的多态
- 提高类型系统的灵活性

---

## 设计模式

### 1. 发布-订阅模式 (Publish-Subscribe Pattern)

**实现方式**:
- **发布者**: 通过 `IEventBus.PublishAsync()` 发布事件
- **订阅者**: 实现 `IIntegrationEventHandler<T>` 接口
- **事件总线**: InMemoryEventBus 作为中介，连接发布者和订阅者

**优势**:
- 松耦合：发布者无需知道订阅者的存在
- 可扩展：轻松添加新的订阅者

---

### 2. 策略模式 (Strategy Pattern)

**应用场景**: 不同的EventBus实现

```
         ┌─────────────────┐
         │    IEventBus    │
         │─────────────────│
         │ + PublishAsync()│
         └────────┬────────┘
                  │
       ┌──────────┼──────────┐
       │          │          │
┌──────▼──────┐ ┌▼──────────▼┐ ┌──────────────┐
│ InMemory    │ │ RabbitMQ   │ │ Redis        │
│ EventBus    │ │ EventBus   │ │ EventBus     │
└─────────────┘ └────────────┘ └──────────────┘
```

**优势**: 运行时可替换不同的实现策略

---

### 3. 依赖注入模式 (Dependency Injection)

**应用场景**:
- 通过构造函数注入 `IServiceProvider`
- 通过 `IOptions<T>` 注入配置
- 通过 DI 容器管理处理器生命周期

---

### 4. 模板方法模式 (Template Method Pattern)

**应用场景**: `EventBusBase` 为消息型实现提供序列化/反序列化能力（InMemory 1.2.0 起直投原事件对象，不再调用这些方法，仅复用其 Logger 与 SubscriptionInfo）

```csharp
public abstract class EventBusBase
{
    // 供 RabbitMQ 等跨进程实现使用：发布前序列化、消费后反序列化
    protected string SerializeMessage(IntegrationEvent @event)
    {
        return JsonSerializer.Serialize(@event, @event.GetType(),
                                         SubscriptionInfo.JsonSerializerOptions);
    }
}
```

---

### 5. 工厂模式 (Factory Pattern)

**应用场景**: DI容器作为处理器工厂

```csharp
// 通过 DI 容器创建处理器实例
var handlers = scope.ServiceProvider.GetKeyedServices<IIntegrationEventHandler>(eventType);
```

---

## 扩展性设计

### 1. 可替换的序列化器

**注意**: InMemory 1.2.0 起直投原事件对象，`ConfigureJsonOptions()` 对 InMemory 链路**不再生效**；该配置供 RabbitMQ 等跨进程实现使用：

```csharp
// 仅影响走消息序列化的实现（如 RabbitMQ）
services.AddRabbitMqEventBus(options => { /* ... */ })
       .ConfigureJsonOptions(options =>
       {
           options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
           options.WriteIndented = true;
       });
```

---

### 2. 多种订阅方式

#### 方式1：自动订阅（推荐）
```csharp
services.AddInMemoryEventBus()
       .AddAutoSubscription(Assembly.GetExecutingAssembly());
```

#### 方式2：手动订阅
```csharp
services.AddInMemoryEventBus()
       .AddSubscription<OrderCreatedEvent, OrderEmailHandler>()
       .AddSubscription<OrderCreatedEvent, OrderSmsHandler>();
```

---

### 3. 扩展到分布式环境

通过继承 `EventBusBase` 可轻松扩展到分布式环境：

```csharp
public class RabbitMQEventBus : EventBusBase, IEventBus
{
    public async Task PublishAsync(IntegrationEvent @event,
                                    CancellationToken cancellationToken = default)
    {
        var message = SerializeMessageToUtf8Bytes(@event);
        // 发送到 RabbitMQ...
    }
}
```

---

## 性能特性

### 1. 内存级别性能

- **零网络开销**: 进程内直接调用，无消息队列
- **零序列化开销**: 1.2.0 起直投原事件对象（1.1.1 每次发布做一次 JSON 序列化+反序列化往返）
- **极低延迟**: 直接内存调用，延迟在微秒级
- **高吞吐**: 单机可支持每秒数万次事件处理

### 2. 并行处理优势

使用 `Task.WhenAll()` 实现真正的并行：

```csharp
// 3个处理器，每个耗时100ms，并行只需100ms
await Task.WhenAll(handlerTasks); // 而不是300ms
```

### 3. 资源管理

- 使用 `CreateAsyncScope()` 确保资源及时释放
- 处理器注册为 Transient，避免单例状态问题

### 4. 内存优化

- 事件处理完成后立即释放作用域
- 无需持久化事件，避免内存堆积

---

## 最佳实践建议

### 1. 处理器设计原则

```csharp
public class OrderCreatedEventHandler : IIntegrationEventHandler<OrderCreatedEvent>
{
    // ✅ DO: 通过构造函数注入依赖
    private readonly ILogger<OrderCreatedEventHandler> _logger;
    private readonly IEmailService _emailService;

    public OrderCreatedEventHandler(ILogger<OrderCreatedEventHandler> logger,
                                     IEmailService emailService)
    {
        _logger = logger;
        _emailService = emailService;
    }

    // ✅ DO: 保持处理幂等性
    public async Task Handle(OrderCreatedEvent @event, CancellationToken cancellationToken)
    {
        // 检查是否已处理
        if (await IsProcessedAsync(@event.Id))
        {
            _logger.LogInformation("Event {EventId} already processed", @event.Id);
            return;
        }

        // 处理业务逻辑
        await _emailService.SendOrderConfirmationAsync(@event);

        // 标记为已处理
        await MarkAsProcessedAsync(@event.Id);
    }

    // ❌ DON'T: 在处理器中执行耗时操作
    public async Task Handle(OrderCreatedEvent @event, CancellationToken cancellationToken)
    {
        // ❌ 不要发送HTTP请求到外部API（应该使用后台服务）
        // ❌ 不要执行长时间的数据库操作（应该异步化）
    }
}
```

### 2. 错误处理策略

```csharp
// ✅ 框架已实现错误隔离，无需额外处理
// 单个处理器失败不会影响其他处理器

// ❌ 不要在处理器中吞掉异常
public async Task Handle(OrderCreatedEvent @event, CancellationToken cancellationToken)
{
    try
    {
        await Process(@event);
    }
    catch
    {
        // ❌ 不要这样做 - 框架已经记录了日志
    }
}
```

### 3. 事件设计原则

```csharp
// ✅ DO: 事件使用不可变设计
public class OrderCreatedEvent : IntegrationEvent
{
    public int OrderId { get; init; }  // init-only属性
    public string CustomerName { get; init; }
    public decimal TotalAmount { get; init; }
}

// ❌ DON'T: 事件包含可变状态
public class OrderCreatedEvent : IntegrationEvent
{
    public int OrderId { get; set; }  // ❌ 可变状态
}
```

### 4. 服务注册建议

```csharp
// ✅ 推荐：自动订阅（开发/测试环境）
services.AddInMemoryEventBus()
       .AddAutoSubscription(Assembly.GetExecutingAssembly());

// ✅ 推荐：手动订阅（需要精细控制时）
services.AddInMemoryEventBus()
       .AddSubscription<OrderCreatedEvent, OrderEmailHandler>()
       .AddSubscription<OrderCreatedEvent, OrderSmsHandler>();

// ❌ 避免：混合使用自动和手动订阅（可能导致重复注册）
```

---

## 局限性与适用场景

### 局限性

1. **进程隔离**: 事件只能在同一进程内传递
2. **无持久化**: 应用重启后未处理的事件会丢失
3. **无重试机制**: 处理失败后不会自动重试
4. **无顺序保证**: 并行处理可能导致事件乱序
5. **无死信队列**: 失败事件无法进入死信队列

### 适用场景

✅ **适合**:
- 单体应用程序
- 开发和测试环境
- 不需要可靠保证的简单场景
- 性能要求极高的本地事件处理

❌ **不适合**:
- 分布式微服务架构（使用 RabbitMQ/Redis 实现）
- 需要事件持久化的场景
- 需要可靠交付保证的生产环境
- 需要跨进程通信的场景

---

## 总结

`Azrng.EventBus.InMemory` 通过精巧的设计实现了：

1. **清晰的分层架构** - 核心抽象层与具体实现层分离
2. **灵活的扩展机制** - 支持多种订阅方式和自定义配置
3. **优秀的性能表现** - 内存级别的事件传递和并行处理
4. **现代.NET特性** - 支持AOT、修剪、Keyed Services等
5. **良好的开发体验** - 自动订阅、类型安全、错误隔离

该项目为单机环境的事件驱动架构提供了一个轻量级、高性能、易用的解决方案。

---

**文档版本**: 1.0
**最后更新**: 2026-02-17
**维护者**: Azrng Team

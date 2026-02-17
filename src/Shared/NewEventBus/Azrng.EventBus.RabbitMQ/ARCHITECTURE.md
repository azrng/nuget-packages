# Azrng.EventBus.RabbitMQ 架构与原理说明

## 目录

- [项目概述](#项目概述)
- [核心架构](#核心架构)
- [工作原理](#工作原理)
- [组件设计](#组件设计)
- [技术实现](#技术实现)
- [依赖关系](#依赖关系)
- [关键设计决策](#关键设计决策)

---

## 项目概述

`Azrng.EventBus.RabbitMQ` 是一个基于 RabbitMQ 的分布式事件总线实现，为微服务架构提供事件驱动通信能力。该项目遵循以下设计原则：

- **发布-订阅模式**：实现解耦的事件发布和订阅机制
- **异步处理**：基于消息队列的异步事件处理
- **高可靠性**：支持消息持久化和重试机制
- **可扩展性**：支持多个事件处理器并行处理同一事件
- **AOT 兼容**：支持 .NET Native AOT 和修剪

---

## 核心架构

### 架构层次图

```
┌─────────────────────────────────────────────────────────────┐
│                     应用层 (Application Layer)                │
│  ┌──────────────┐                    ┌──────────────┐        │
│  │ 事件发布者   │                    │ 事件处理器   │        │
│  │ Event Publisher                  │ Event Handler │        │
│  └──────────────┘                    └──────────────┘        │
└─────────────────────────────────────────────────────────────┘
                            ▲                         │
                            │                         │
┌───────────────────────────┼─────────────────────────┼───────┐
│                    事件总线抽象层                      │       │
│               (Azrng.EventBus.Core)                   │       │
│  ┌────────────────────────────────────────────────┐  │       │
│  │ IEventBus, IntegrationEvent,                    │  │       │
│  │ IIntegrationEventHandler<T>, EventBusBase      │  │       │
│  └────────────────────────────────────────────────┘  │       │
└───────────────────────────┼─────────────────────────┼───────┘
                            │                         ▼
┌───────────────────────────┼───────────────────────────────────┐
│                 RabbitMQ 实现层 (Implementation Layer)          │
│                   (Azrng.EventBus.RabbitMQ)                    │
│  ┌────────────────────────────────────────────────────────┐   │
│  │  RabbitMQEventBus - IHostedService                     │   │
│  │  ├─ PublishAsync()    → 发布消息到 Exchange             │   │
│  │  ├─ StartAsync()      → 启动消费者监听                  │   │
│  │  └─ ProcessEventAsync() → 处理接收的消息                │   │
│  └────────────────────────────────────────────────────────┘   │
└───────────────────────────┼───────────────────────────────────┘
                            │
                            ▼
┌───────────────────────────────────────────────────────────────┐
│                    RabbitMQ 消息中间件                         │
│  ┌──────────┐    ┌─────────────┐    ┌──────────────┐         │
│  │ Exchange │───▶│ Binding Key │───▶│    Queue     │         │
│  │ (direct) │    │ (EventName) │    │ (持久化)      │         │
│  └──────────┘    └─────────────┘    └──────────────┘         │
└───────────────────────────────────────────────────────────────┘
```

### 类图关系

```
┌───────────────────────────┐      ┌───────────────────────────┐
│    IntegrationEvent       │      │ IIntegrationEventHandler │
│   ─────────────────────   │      │   ─────────────────────   │
│ + Id: Guid                │      │ + Handle(event): Task     │
│ + CreationDate: DateTime  │      │                           │
└─────────────┬─────────────┘      └─────────────┬─────────────┘
              │                                  │
              │ 继承                              │ 实现
              ▼                                  ▼
    ┌─────────────────────┐          ┌─────────────────────┐
    │   具体事件类         │          │   具体处理器类       │
    │   e.g.              │          │   e.g.              │
    │   OrderCreatedEvent │          │   OrderCreatedEvent │
    └─────────────────────┘          │   Handler           │
                                    └─────────────────────┘
    ┌──────────────────────────────────────────────┐
    │          IEventBus                            │
    │   ────────────────────────────────           │
    │ + PublishAsync(event): Task                  │
    └─────────────┬────────────────────────────────┘
                  │ 实现
                  ▼
    ┌──────────────────────────────────────────────┐
    │       EventBusBase (抽象基类)                 │
    │   ────────────────────────────────           │
    │ # SerializeMessage()                         │
    │ # DeserializeMessage()                       │
    │ # SubscriptionInfo                           │
    │ # Logger                                     │
    └─────────────┬────────────────────────────────┘
                  │ 继承
                  ▼
    ┌──────────────────────────────────────────────┐
    │     RabbitMQEventBus                         │
    │   ────────────────────────────────           │
    │ + PublishAsync(event)                        │
    │ + StartAsync()        [IHostedService]       │
    │ + StopAsync()         [IHostedService]       │
    │ - OnMessageReceived()                        │
    │ - ProcessEventAsync()                        │
    └──────────────────────────────────────────────┘
```

---

## 工作原理

### 1. 应用启动流程

```
┌─────────────────────────────────────────────────────────────┐
│                     应用程序启动                              │
└─────────────────────────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────┐
│   services.AddRabbitMqEventBus(options)                      │
│   ├─ 配置 RabbitMQ 连接参数                                  │
│   ├─ 注册 IConnection (单例)                                 │
│   ├─ 注册 IEventBus → RabbitMQEventBus (单例)                │
│   └─ 注册 IHostedService → RabbitMQEventBus                  │
└─────────────────────────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────┐
│   IHostedService.StartAsync() 被自动调用                      │
│   ├─ 创建 RabbitMQ 连接                                      │
│   ├─ 声明 Exchange (Direct 类型)                             │
│   ├─ 声明 Queue (持久化)                                     │
│   ├─ 绑定 Queue 与 Exchange (通过事件名)                     │
│   └─ 启动消费者监听 (AsyncEventingBasicConsumer)              │
└─────────────────────────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────┐
│              后台线程持续监听消息                             │
│     (异步长连接，应用程序运行期间保持活跃)                     │
└─────────────────────────────────────────────────────────────┘
```

### 2. 事件发布流程

```
┌──────────────────┐
│ 发布者代码       │
│ _eventBus        │
│  .PublishAsync(  │
│   event)         │
└────────┬─────────┘
         │
         ▼
┌─────────────────────────────────────────────────────────────┐
│  RabbitMQEventBus.PublishAsync(event)                        │
│  ┌─────────────────────────────────────────────────────┐    │
│  │ 1. 获取事件类型名作为 RoutingKey                      │    │
│  │    routingKey = event.GetType().Name                 │    │
│  │    例: "OrderCreatedEvent"                           │    │
│  └─────────────────────────────────────────────────────┘    │
│  ┌─────────────────────────────────────────────────────┐    │
│  │ 2. 创建 RabbitMQ Channel                              │    │
│  │    channel = _connection.CreateModel()               │    │
│  └─────────────────────────────────────────────────────┘    │
│  ┌─────────────────────────────────────────────────────┐    │
│  │ 3. 声明 Exchange (如果不存在)                         │    │
│  │    channel.ExchangeDeclare(                         │    │
│  │        exchange: "direct_exchange",                  │    │
│  │        type: "direct")                              │    │
│  └─────────────────────────────────────────────────────┘    │
│  ┌─────────────────────────────────────────────────────┐    │
│  │ 4. 序列化事件为 UTF-8 字节数组                        │    │
│  │    body = JsonSerializer.SerializeToUtf8Bytes(event) │   │
│  └─────────────────────────────────────────────────────┘    │
│  ┌─────────────────────────────────────────────────────┐    │
│  │ 5. 设置消息属性（持久化）                             │    │
│  │    properties.DeliveryMode = 2                       │    │
│  └─────────────────────────────────────────────────────┘    │
│  ┌─────────────────────────────────────────────────────┐    │
│  │ 6. 通过 Polly 重试管道发布消息                        │    │
│  │    _pipeline.Execute(() => {                         │    │
│  │        channel.BasicPublish(                         │    │
│  │            exchange: "direct_exchange",              │    │
│  │            routingKey: "OrderCreatedEvent",          │    │
│  │            body: body)                               │    │
│  │    })                                                │    │
│  └─────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────┐
│                    RabbitMQ 服务器                           │
│   Exchange 接收消息 → 根据 RoutingKey 路由到绑定队列          │
└─────────────────────────────────────────────────────────────┘
```

### 3. 事件消费流程

```
┌─────────────────────────────────────────────────────────────┐
│              RabbitMQ 推送消息到消费者                        │
│   (AsyncEventingBasicConsumer.Received 事件触发)              │
└─────────────────────────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────┐
│  OnMessageReceived(eventArgs)                                │
│  ┌─────────────────────────────────────────────────────┐    │
│  │ 1. 提取 RoutingKey (事件名)                           │    │
│  │    eventName = eventArgs.RoutingKey                  │    │
│  └─────────────────────────────────────────────────────┘    │
│  ┌─────────────────────────────────────────────────────┐    │
│  │ 2. 反序列化消息体                                     │    │
│  │    message = Encoding.UTF8.GetString(body)           │    │
│  └─────────────────────────────────────────────────────┘    │
│  ┌─────────────────────────────────────────────────────┐    │
│  │ 3. 调用 ProcessEventAsync 处理事件                   │    │
│  └─────────────────────────────────────────────────────┘    │
│  ┌─────────────────────────────────────────────────────┐    │
│  │ 4. 手动确认消息（无论成功或失败）                     │    │
│  │    _channel.BasicAck(deliveryTag, multiple: false)   │    │
│  └─────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────┐
│  ProcessEventAsync(eventName, message)                        │
│  ┌─────────────────────────────────────────────────────┐    │
│  │ 1. 创建异步依赖注入作用域                             │    │
│  │    await using var scope =                           │    │
│  │        _serviceProvider.CreateAsyncScope()           │    │
│  └─────────────────────────────────────────────────────┘    │
│  ┌─────────────────────────────────────────────────────┐    │
│  │ 2. 查找事件类型                                       │    │
│  │    if (!EventTypes.TryGetValue(eventName,            │    │
│  │         out var eventType)) return;                  │    │
│  └─────────────────────────────────────────────────────┘    │
│  ┌─────────────────────────────────────────────────────┐    │
│  │ 3. 反序列化为 IntegrationEvent 对象                   │    │
│  │    integrationEvent =                                │    │
│  │        DeserializeMessage(message, eventType)        │    │
│  └─────────────────────────────────────────────────────┘    │
│  ┌─────────────────────────────────────────────────────┐    │
│  │ 4. 获取所有事件处理器                                 │    │
│  │    handlers = scope.ServiceProvider                  │    │
│  │        .GetKeyedServices<IIntegrationEventHandler>   │    │
│  │        (eventType)                                   │    │
│  └─────────────────────────────────────────────────────┘    │
│  ┌─────────────────────────────────────────────────────┐    │
│  │ 5. 并行执行所有处理器                                 │    │
│  │    await Task.WhenAll(handlers.Select(h =>           │    │
│  │        h.Handle(integrationEvent)))                  │    │
│  └─────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────┘
```

### 4. 重试机制流程

```
┌─────────────────────────────────────────────────────────────┐
│              Polly 重试管道 (ResiliencePipeline)              │
└─────────────────────────────────────────────────────────────┘
                           │
                           ▼
        ┌──────────────────────────────────┐
        │      尝试发布消息                 │
        └─────────────┬────────────────────┘
                      │
                      ▼
              ┌───────────────┐
              │   发布成功?   │
              └───────┬───────┘
                      │
          ┌───────────┴───────────┐
          │ Yes                   │ No
          ▼                       ▼
    ┌──────────┐         ┌──────────────────┐
    │   完成    │         │ 检查异常类型      │
    └──────────┘         │ - BrokerUnreachableException
                         │ - SocketException
                         └─────────┬────────┘
                                   │
                                   ▼
                         ┌──────────────────┐
                         │ 已达重试上限?      │
                         │ (默认 10 次)      │
                         └─────────┬────────┘
                                   │
                      ┌────────────┴────────────┐
                      │ No                      │ Yes
                      ▼                         ▼
            ┌──────────────────┐      ┌──────────────┐
            │ 等待后重试         │      │ 抛出异常     │
            │ 延迟: 2^attempt 秒│      └──────────────┘
            │ (指数退避)         │
            └───────────────────┘
                 │
                 └────────► 回到 "尝试发布消息"
```

---

## 组件设计

### 1. RabbitMQEventBus (核心类)

**文件**: [RabbitMQEventBus.cs](RabbitMQEventBus.cs)

**职责**:
- 实现 `IEventBus` 接口提供事件发布能力
- 实现 `IHostedService` 作为后台服务持续监听消息
- 管理 RabbitMQ 连接和消费者通道

**关键方法**:

| 方法 | 说明 |
|------|------|
| `PublishAsync()` | 发布事件到 RabbitMQ Exchange |
| `StartAsync()` | 启动后台消费者服务 |
| `OnMessageReceived()` | RabbitMQ 消息到达时的回调 |
| `ProcessEventAsync()` | 处理事件并调用处理器 |

**状态管理**:
```csharp
private readonly ResiliencePipeline _pipeline;     // 重试管道
private IConnection _rabbitMqConnection;            // RabbitMQ 连接
private IModel _consumerChannel;                    // 消费者通道
private readonly IServiceProvider _serviceProvider; // DI 容器
private readonly RabbitmqEventBusOptions _options;  // 配置选项
```

### 2. RabbitMqDependencyInjectionExtensions

**文件**: [RabbitMqDependencyInjectionExtensions.cs](RabbitMqDependencyInjectionExtensions.cs)

**职责**: 提供依赖注入扩展方法

**服务注册**:
```csharp
services.AddSingleton<IConnectionFactory>()      // 连接工厂
services.AddSingleton<IConnection>()               // RabbitMQ 连接
services.AddSingleton<IEventBus, RabbitMQEventBus>() // 事件总线
services.AddSingleton<IHostedService>()            // 后台服务
```

### 3. RabbitmqEventBusOptions

**文件**: [RabbitmqEventBusOptions.cs](RabbitmqEventBusOptions.cs)

**配置选项**:

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `HostName` | string | - | RabbitMQ 服务器地址 |
| `VirtualHost` | string | - | 虚拟主机 |
| `UserName` | string | - | 用户名 |
| `Password` | string | - | 密码 |
| `Port` | int | 5672 | AMQP 端口 |
| `SubscriptionClientName` | string | "defaultQueue" | 队列名称 |
| `RetryCount` | int | 10 | 发布失败重试次数 |
| `ExchangeName` | string | "direct_exchange" | 交换机名称 |

### 4. EventBusBase (抽象基类)

**文件**: [../Azrng.EventBus.Core/Abstractions/EventBusBase.cs](../Azrng.EventBus.Core/Abstractions/EventBusBase.cs)

**职责**: 提供序列化/反序列化的公共方法，支持 AOT

**关键方法**:
- `SerializeMessage()`: 序列化事件为 JSON 字符串
- `SerializeMessageToUtf8Bytes()`: 序列化为 UTF-8 字节数组
- `DeserializeMessage()`: 反序列化消息为事件对象

**AOT 兼容性**:
使用特性抑制编译器警告，支持 Native AOT：
```csharp
[UnconditionalSuppressMessage("Trimming", "IL2026:RequiresUnreferencedCode", ...)]
[UnconditionalSuppressMessage("AOT", "IL3050:RequiresDynamicCode", ...)]
```

---

## 技术实现

### 1. RabbitMQ 消息模式

本项目使用 **Direct Exchange** 模式：

```
                    ┌─────────────────────────┐
                    │   direct_exchange       │
                    │   (ExchangeDeclare)     │
                    └───────────┬─────────────┘
                                │
          ┌─────────────────────┼─────────────────────┐
          │                     │                     │
          ▼                     ▼                     ▼
    ┌──────────┐         ┌──────────┐          ┌──────────┐
    │ Queue 1  │         │ Queue 2  │          │ Queue 3  │
    │ Key: A   │         │ Key: B   │          │ Key: C   │
    └──────────┘         └──────────┘          └──────────┘
         │                    │                     │
         ▼                    ▼                     ▼
    Consumer 1            Consumer 2            Consumer 3
```

**关键代码**:
```csharp
// 声明 Direct Exchange
channel.ExchangeDeclare(
    exchange: "direct_exchange",
    type: "direct"
);

// 绑定队列到 Exchange（通过事件名作为 RoutingKey）
channel.QueueBind(
    queue: subscriptionClientName,
    exchange: "direct_exchange",
    routingKey: eventName  // 例如: "OrderCreatedEvent"
);
```

### 2. 消息持久化

确保消息在 RabbitMQ 服务器重启后不丢失：

```csharp
// 1. 队列持久化
channel.QueueDeclare(
    queue: queueName,
    durable: true,      // ← 持久化队列
    exclusive: false,
    autoDelete: false,
    arguments: null
);

// 2. 消息持久化
var properties = channel.CreateBasicProperties();
properties.DeliveryMode = 2;  // ← 2 = 持久化消息
```

### 3. 手动确认机制

使用手动确认 (Manual Ack) 确保消息处理完成：

```csharp
// 创建消费者时设置 autoAck: false
channel.BasicConsume(
    queue: queueName,
    autoAck: false,  // ← 手动确认
    consumer: consumer
);

// 消息处理完成后确认
_consumerChannel.BasicAck(
    eventArgs.DeliveryTag,
    multiple: false
);
```

**注意**: 当前实现在异常时也会确认消息。生产环境建议使用 **死信队列 (DLX)** 处理失败消息。

### 4. 错误隔离机制

多个处理器处理同一事件时，一个处理器失败不影响其他处理器：

```csharp
await Task.WhenAll(handlers.Select(async handler =>
{
    try
    {
        await handler.Handle(integrationEvent);
    }
    catch (Exception ex)
    {
        // 仅记录错误，不影响其他处理器
        Logger.LogError(ex, "Error processing event...");
    }
}));
```

### 5. Polly 重试策略

使用 Polly v8 实现指数退避重试：

```csharp
private static ResiliencePipeline CreateResiliencePipeline(int retryCount)
{
    var retryOptions = new RetryStrategyOptions
    {
        ShouldHandle = new PredicateBuilder()
            .Handle<BrokerUnreachableException>()  // 处理 RabbitMQ 不可达
            .Handle<SocketException>(),            // 处理网络异常
        MaxRetryAttempts = retryCount,
        DelayGenerator = (context) =>
            ValueTask.FromResult(GenerateDelay(context.AttemptNumber))
    };

    return new ResiliencePipelineBuilder()
        .AddRetry(retryOptions)
        .Build();

    // 指数退避: 2^attempt 秒
    // attempt 0: 1秒
    // attempt 1: 2秒
    // attempt 2: 4秒
    // attempt 3: 8秒
    static TimeSpan? GenerateDelay(int attempt)
    {
        return TimeSpan.FromSeconds(Math.Pow(2, attempt));
    }
}
```

### 6. 依赖注入作用域管理

为每个消息创建独立的 DI 作用域，确保服务正确释放：

```csharp
private async Task ProcessEventAsync(string eventName, string message)
{
    // 创建异步作用域
    await using var scope = _serviceProvider.CreateAsyncScope();

    // 从作用域获取服务
    var handlers = scope.ServiceProvider
        .GetKeyedServices<IIntegrationEventHandler>(eventType);

    // 作用域在 using 块结束时自动释放
}
```

### 7. 后台服务实现

使用 `IHostedService` 在后台线程运行消息监听：

```csharp
public Task StartAsync(CancellationToken cancellationToken)
{
    _ = Task.Factory.StartNew(() =>
    {
        // 在后台线程中执行
        _rabbitMqConnection = _serviceProvider.GetRequiredService<IConnection>();
        _consumerChannel = _rabbitMqConnection.CreateModel();

        // 启动消费者
        var consumer = new AsyncEventingBasicConsumer(_consumerChannel);
        consumer.Received += OnMessageReceived;

        _consumerChannel.BasicConsume(...);
    },
    cancellationToken,
    TaskCreationOptions.LongRunning,  // 标记为长运行任务
    TaskScheduler.Default);

    return Task.CompletedTask;
}
```

---

## 依赖关系

### 项目依赖

```
Azrng.EventBus.RabbitMQ
│
├── Azrng.EventBus.Core (核心抽象)
│   ├── Abstractions/
│   │   ├── IEventBus
│   │   ├── IIntegrationEventHandler<T>
│   │   ├── EventBusBase
│   │   └── EventBusSubscriptionInfo
│   └── Events/
│       └── IntegrationEvent
│
├── RabbitMQ.Client (RabbitMQ .NET 客户端)
│
├── Microsoft.Extensions.Hosting (后台服务支持)
│
└── Polly (弹性与重试库)
```

### NuGet 包依赖

| 包名 | 版本 | 用途 |
|------|------|------|
| `RabbitMQ.Client` | 6.x | RabbitMQ AMQP 客户端 |
| `Microsoft.Extensions.Hosting` | 8.x/9.x | IHostedService 支持 |
| `Polly` | 8.x | 重试和弹性策略 |
| `System.Text.Json` | 内置 | JSON 序列化 |

---

## 关键设计决策

### 1. 为什么选择 Direct Exchange？

**决策原因**:
- **精确路由**: 每个事件类型有唯一的 RoutingKey，确保消息只路由到订阅该事件的队列
- **性能优势**: Direct Exchange 是最简单的交换机类型，路由开销最小
- **类型安全**: 通过事件类型名作为 RoutingKey，在编译期就能确定路由关系

**对比其他 Exchange 类型**:
- **Fanout**: 广播所有队列，不适合事件总线场景
- **Topic**: 支持通配符，但增加了复杂度，对于事件总线是过度设计
- **Headers**: 基于消息头路由，性能较差且复杂

### 2. 为什么使用手动确认 (Manual Ack)？

**决策原因**:
- **可靠性保证**: 确保消息处理完成后才从队列删除
- **错误处理**: 可以选择在失败时重新入队 (NACK + requeue=true)
- **控制粒度**: 可以批量确认 (multiple:true) 提高性能

**权衡**:
- 需要手动调用 `BasicAck()`，代码复杂度增加
- 性能略低于自动确认，但差异可忽略

### 3. 为什么使用 Polly 而不是手动重试？

**决策原因**:
- **声明式**: 通过配置定义重试策略，代码更清晰
- **灵活性**: 支持多种重试模式 (固定间隔、指数退避、抖动等)
- **可组合**: 可以与其他策略 (超时、熔断器) 组合使用
- **成熟稳定**: Polly 是 .NET 社区标准的弹性库

### 4. 为什么每个消息创建独立的 DI 作用域？

**决策原因**:
- **资源管理**: Scoped 服务 (如 DbContext) 在处理完成后正确释放
- **隔离性**: 不同消息的处理互不影响
- **测试友好**: 便于单元测试中模拟依赖

**代码示例**:
```csharp
await using var scope = _serviceProvider.CreateAsyncScope();
var handler = scope.ServiceProvider.GetRequiredService<MyHandler>();
// scope 释放时，所有 Scoped 服务也会释放
```

### 5. 为什么使用后台线程而不是 HostedService 的标准模式？

**决策原因**:
- **阻塞 API**: RabbitMQ.Client 的 `BasicConsume` 是阻塞调用
- **持续监听**: 消息监听是长时间运行的操作，需要专用线程
- **避免阻塞**: 不阻塞主线程，应用可以正常启动

**标准 HostedService 模式问题**:
```csharp
// ❌ 错误：阻塞了 HostedService 的执行循环
public Task StartAsync(CancellationToken token)
{
    _consumerChannel.BasicConsume(...); // 阻塞调用
    return Task.CompletedTask;
}

// ✅ 正确：在后台线程运行
public Task StartAsync(CancellationToken token)
{
    _ = Task.Factory.StartNew(() => {
        _consumerChannel.BasicConsume(...);
    }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
    return Task.CompletedTask;
}
```

### 6. 为什么支持多个处理器并行处理同一事件？

**决策原因**:
- **解耦业务逻辑**: 一个事件可能触发多个独立的业务流程
- **提高并发度**: 并行执行可以提高吞吐量
- **可扩展性**: 新增处理器不影响现有处理器

**示例场景**:
```
OrderCreatedEvent 触发:
├── EmailHandler (发送确认邮件)
├── InventoryHandler (扣减库存)
├── AnalyticsHandler (更新销售统计)
└── NotificationHandler (推送通知)
```

### 7. 为什么使用 Keyed Services 注册处理器？

**决策原因**:
- **类型安全**: 通过事件类型作为 Key，避免字符串拼写错误
- **多实例支持**: 同一事件可以有多个处理器
- **查询高效**: `GetKeyedServices<T>(key)` 直接获取特定类型的处理器

**注册方式**:
```csharp
services.AddKeyedSingleton<IIntegrationEventHandler, OrderCreatedEmailHandler>
    (typeof(OrderCreatedEvent));
services.AddKeyedSingleton<IIntegrationEventHandler, OrderCreatedSMSHandler>
    (typeof(OrderCreatedEvent));
```

### 8. 为什么异常时仍然确认消息？

**当前实现**:
```csharp
try
{
    await ProcessEventAsync(eventName, message);
}
catch (Exception ex)
{
    Logger.LogWarning(ex, "Error Processing message");
}
finally
{
    _consumerChannel.BasicAck(eventArgs.DeliveryTag, multiple: false);
}
```

**设计考虑**:
- **避免消息堆积**: 处理失败的消息不会无限重试导致队列堵塞
- **错误可见**: 通过日志记录错误，便于监控和排查
- **简化实现**: 避免复杂的重试逻辑

**生产环境建议**:
使用 **死信队列 (DLX)** 存储失败消息:
```csharp
var args = new Dictionary<string, object>
{
    { "x-dead-letter-exchange", "dlx_exchange" }
};
channel.QueueDeclare(queue: "main_queue", durable: true, arguments: args);
```

---

## 性能考虑

### 1. 消息序列化

- 使用 `System.Text.Json` 而不是 `Newtonsoft.Json`，性能更好
- 直接序列化为 UTF-8 字节数组，避免中间字符串分配
- 支持 AOT 友好的序列化配置

### 2. 并行处理

- 多个处理器使用 `Task.WhenAll` 并行执行
- 错误隔离确保单个处理器失败不影响其他处理器

### 3. 连接管理

- `IConnection` 作为单例，整个应用共享一个连接
- 每个发布操作创建临时 Channel，用完即释放
- 消费者 Channel 长期存在，避免重复创建

### 4. 重试策略

- 指数退避避免雪崩效应
- 仅处理网络相关异常，避免无效重试

---

## 安全建议

1. **连接安全**:
   - 使用 TLS/SSL 加密连接 (`ConnectionFactory.Ssl = true`)
   - 不要在代码中硬编码凭据，使用配置管理

2. **访问控制**:
   - 为不同应用创建独立的 RabbitMQ 用户
   - 使用虚拟主机 (Virtual Host) 隔离环境

3. **消息验证**:
   - 在处理器中验证事件数据
   - 考虑添加消息签名或加密敏感数据

---

## 监控建议

1. **关键指标**:
   - 消息发布速率
   - 消息消费延迟
   - 队列深度
   - 处理器错误率

2. **日志级别**:
   - Trace: 发布/消费详细流程
   - Information: 连接状态、关键操作
   - Warning: 处理器异常
   - Error: 连接失败、严重错误

3. **RabbitMQ 管理界面**:
   - 监控队列状态
   - 查看消息速率
   - 分析连接和通道

---

## 扩展方向

1. **事务支持**:
   - 实现 `PublishAsync` 的事务性发布
   - 与数据库事务集成 (Outbox 模式)

2. **消息追踪**:
   - 添加 CorrelationId
   - 集成分布式追踪 (OpenTelemetry)

3. **高级消费模式**:
   - 支持批量消费
   - 支持延迟重试
   - 支持消息优先级

4. **高可用性**:
   - 连接断开自动重连
   - 主备队列切换
   - 集群支持

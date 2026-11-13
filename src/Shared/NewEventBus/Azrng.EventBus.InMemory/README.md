# Azrng.EventBus.InMemory

这是一个基于内存实现的事件总线库，适用于单机环境的事件驱动架构。它是 Azrng.EventBus.Core 的具体实现之一。

## 功能特性

- 基于内存的事件总线实现
- 适用于单机环境或开发测试环境
- 支持事件的发布/订阅模式
- 支持多个事件处理器处理同一事件
- 直投原事件对象：零序列化开销，处理器与发布方共享同一实例
- 处理器失败策略可配置：默认记日志继续，可选抛回发布方
- 支持自动和手动订阅事件处理器
- 支持 AOT 和修剪兼容性
- 支持多框架：.NET 8.0 / 9.0 / 10.0

## 安装

通过 NuGet 安装:

```
Install-Package Azrng.EventBus.InMemory
```

或通过 .NET CLI:

```
dotnet add package Azrng.EventBus.InMemory
```

## 使用方法

### 注册服务

在 Program.cs 中注册服务：

```csharp
services.AddInMemoryEventBus()
       .AddAutoSubscription(Assembly.GetExecutingAssembly());
```

配置处理器失败策略（可选）：

```csharp
services.AddInMemoryEventBus(options =>
    {
        // 默认 LogAndContinue：处理器失败只记日志，发布方不感知
        // Throw：等全部处理器执行完毕后向发布方抛 AggregateException
        options.HandlerFailurePolicy = HandlerFailurePolicy.Throw;
    })
    .AddAutoSubscription(Assembly.GetExecutingAssembly());
```

### 定义事件

创建集成事件类，继承自 [IntegrationEvent]()：

```csharp
public class OrderCreatedEvent : IntegrationEvent
{
    public int OrderId { get; init; }
    public string CustomerName { get; init; }
    public decimal TotalAmount { get; init; }
}
```

### 创建事件处理器

实现 [IIntegrationEventHandler<T>]() 接口来处理事件：

```csharp
public class OrderCreatedEventHandler : IIntegrationEventHandler<OrderCreatedEvent>
{
    private readonly ILogger<OrderCreatedEventHandler> _logger;

    public OrderCreatedEventHandler(ILogger<OrderCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(OrderCreatedEvent @event, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Order created: ID={OrderId}, Customer={CustomerName}, Amount={TotalAmount}",
            @event.OrderId, @event.CustomerName, @event.TotalAmount);

        // 处理订单创建后的业务逻辑
        await ProcessOrderCreation(@event);
    }

    private async Task ProcessOrderCreation(OrderCreatedEvent @event)
    {
        // 实际的业务处理逻辑
        await Task.Delay(100); // 模拟异步操作
    }
}
```

### 发布事件

注入 [IEventBus]() 接口并使用 [PublishAsync]() 方法发布事件：

```csharp
public class OrderService
{
    private readonly IEventBus _eventBus;

    public OrderService(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    public async Task CreateOrderAsync(CreateOrderRequest request)
    {
        // 创建订单的业务逻辑
        var orderId = await SaveOrderAsync(request);

        // 发布订单创建事件
        var orderCreatedEvent = new OrderCreatedEvent
        {
            OrderId = orderId,
            CustomerName = request.CustomerName,
            TotalAmount = request.TotalAmount
        };

        await _eventBus.PublishAsync(orderCreatedEvent);
    }

    private async Task<int> SaveOrderAsync(CreateOrderRequest request)
    {
        // 保存订单到数据库的逻辑
        await Task.Delay(100); // 模拟异步操作
        return new Random().Next(1000, 9999);
    }
}
```

## 核心概念

### IEventBus

[IEventBus]() 是事件总线的核心接口，提供了 [PublishAsync]() 方法用于发布事件。

### IntegrationEvent

[IntegrationEvent]() 是事件的基类，所有具体的事件都应该继承自此类。它包含：
- `Id`: 事件的唯一标识符
- `CreationDate`: 事件创建时间

### IIntegrationEventHandler

[IIntegrationEventHandler<T>]() 是事件处理器接口，用于处理特定类型的事件。

### EventBusSubscriptionInfo

[EventBusSubscriptionInfo]() 包含事件总线的订阅信息，包括：
- `EventTypes`: 已注册的事件类型字典
- `JsonSerializerOptions`: JSON 序列化配置选项

## 适用场景

- 单体应用程序中的事件驱动架构
- 开发和测试环境
- 不需要跨进程通信的简单场景
- 性能要求较高的本地事件处理

## 注意事项

1. 该实现在进程内工作，不适用于分布式环境
2. `PublishAsync` 在当前调用链中等待全部事件处理器完成
3. 处理器直接收到发布的事件对象（**同一实例，无序列化**）：处理器不应修改事件属性，否则会同时影响发布方与其他处理器
4. 处理器失败默认只记日志（`LogAndContinue`），发布方不感知；需要感知失败时配置 `HandlerFailurePolicy.Throw`（单个失败仍不阻断其他处理器，全部执行完后抛 `AggregateException`）
5. 事件不会持久化，应用程序重启后事件会丢失；需要可靠投递时使用 RabbitMQ 实现

## 与其他实现的区别

相比于基于 RabbitMQ 的实现，内存事件总线：
- 更轻量级，无需外部依赖
- 零序列化开销：处理器直投原事件对象
- 不支持跨进程/跨机器通信
- 不提供事件持久化和可靠性保证
- 适用于开发测试环境而非生产环境

## 版本更新记录

* 1.2.0
    * 处理器直投原事件对象（1.1.1 为 JSON 序列化往返副本：不可序列化属性会静默丢失）
    * 新增 `HandlerFailurePolicy` 失败策略（默认 `LogAndContinue` 保持兼容，可配 `Throw`）
    * 处理器激活（构造）失败遵循与执行失败相同的策略
    * 补 Nullable 支持，移除无用的配置绑定生成器声明
* 1.1.1
    * 明确 `PublishAsync` 在当前调用链中等待全部处理器完成
* 1.1.0
    * 支持.Net10并优化
* 1.0.0
    * 初始版本

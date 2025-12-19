# Azrng.EventBus.RabbitMQ

这是一个基于 RabbitMQ 实现的事件总线库，适用于分布式环境的事件驱动架构。它是 Azrng.EventBus.Core 的具体实现之一。

## 功能特性

- 基于 RabbitMQ 的事件总线实现
- 适用于分布式环境
- 支持事件的发布/订阅模式
- 支持多个事件处理器处理同一事件
- 支持 JSON 序列化配置
- 支持自动和手动订阅事件处理器
- 支持 AOT 和修剪兼容性
- 支持多框架：.NET 8.0 / 9.0

## 安装

通过 NuGet 安装:

```
Install-Package Azrng.EventBus.RabbitMQ
```

或通过 .NET CLI:

```
dotnet add package Azrng.EventBus.RabbitMQ
```

## 使用方法

### 注册服务

在 Program.cs 中注册服务：

```csharp
services.AddRabbitMqEventBus(options =>
{
    options.HostName = "localhost";
    options.VirtualHost = "/";
    options.UserName = "guest";
    options.Password = "guest";
    options.Port = 5672;
    options.SubscriptionClientName = "default_queue";
    options.RetryCount = 10;
    options.ExchangeName = "direct_exchange";
})
.AddAutoSubscription(Assembly.GetExecutingAssembly());
```

### 定义事件

创建集成事件类，继承自 [IntegrationEvent]()：

```csharp
public record OrderCreatedEvent : IntegrationEvent
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

    public async Task Handle(OrderCreatedEvent @event)
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

## 配置选项

[RabbitmqEventBusOptions]() 类提供了以下配置选项：

- `HostName`: RabbitMQ 主机地址
- `VirtualHost`: RabbitMQ 虚拟主机
- `UserName`: 用户名
- `Password`: 密码
- `Port`: 端口号，默认为 5672
- `SubscriptionClientName`: 订阅客户端名称（队列名称）
- `RetryCount`: 重试次数，默认为 10
- `ExchangeName`: 交换机名称，默认为 "direct_exchange"

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

## 实现原理

RabbitMQ 事件总线基于 RabbitMQ.Client 库实现，使用了直接交换机（Direct Exchange）模式：

1. 使用 `AddKeyedTransient` 注册事件处理器，支持同一事件类型的多个处理器
2. 通过 `IKeyedServiceProvider.GetKeyedService` 获取事件处理器实例
3. 使用 JSON 序列化/反序列化事件对象
4. 通过 RabbitMQ 实现事件的发布和订阅
5. 支持消息确认机制，确保消息被正确处理
6. 使用 Polly 实现重试策略，提高系统的容错能力
7. 通过后台服务（IHostedService）实现消息的持续监听和处理

## 适用场景

- 微服务架构中的服务间通信
- 分布式系统中的事件驱动架构
- 需要跨进程、跨机器通信的场景
- 需要消息持久化和可靠传递的生产环境
- 需要削峰填谷、异步处理的业务场景

## 注意事项

1. 需要预先安装和配置 RabbitMQ 服务器
2. 需要正确配置连接参数（主机名、端口、用户名、密码等）
3. 事件处理是异步执行的，通过后台服务持续监听消息
4. 事件会持久化到 RabbitMQ，即使应用程序重启也不会丢失
5. 支持事件的重试机制，默认重试10次
6. 需要合理设置队列和交换机的参数以满足业务需求
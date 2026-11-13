# Azrng.EventBus.RabbitMQ

这是一个基于 RabbitMQ 实现的事件总线库，适用于分布式环境的事件驱动架构。它是 Azrng.EventBus.Core 的具体实现之一。

## 功能特性

- 基于 RabbitMQ 的事件总线实现
- 适用于分布式环境
- 支持事件的发布/订阅模式
- 支持多个事件处理器处理同一事件
- 支持 JSON 序列化配置
- 支持自动和手动订阅事件处理器
- 消费失败的消息转入死信队列，不再随 ACK 丢弃
- 发布重试 + 断线自动重连（指数退避）
- 启动期配置校验（队列名必填等）
- 支持多框架：.NET 8.0 / 9.0 / 10.0

> 注：依赖的 RabbitMQ.Client 6.x 不支持 Native AOT，本包未声明 AOT 兼容；Core 与 InMemory 包保持 AOT 兼容。

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
    // 必填：本服务的消费队列名，每个服务必须唯一（共用同名队列会互相抢消息）
    options.SubscriptionClientName = "order_service_queue";
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

- `HostName`: RabbitMQ 主机地址，默认 `localhost`
- `VirtualHost`: RabbitMQ 虚拟主机，默认 `/`
- `UserName`: 用户名，默认 `guest`
- `Password`: 密码，默认 `guest`
- `Port`: 端口号，默认为 5672
- `SubscriptionClientName`: 订阅队列名称，必填（启动期校验）；每个服务必须唯一
- `RetryCount`: 发布重试次数，默认为 10
- `ExchangeName`: 交换机名称，默认为 "direct_exchange"
- `DeadLetterExchangeName`: 死信交换机名称，默认为 "eventbus_dlx"
- `DeadLetterQueueName`: 死信队列名称，默认为 "eventbus_dlx_queue"

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

- 微服务架构中的服务间通信
- 分布式系统中的事件驱动架构
- 需要跨进程、跨机器通信的场景
- 需要消息持久化和可靠传递的生产环境
- 需要削峰填谷、异步处理的业务场景

## 注意事项

1. 需要预先安装和配置 RabbitMQ 服务器
2. 需要正确配置连接参数（主机名、端口、用户名、密码等）
3. 事件处理是异步执行的，通过后台服务持续监听消息
4. 消息与队列均为持久化，broker 重启不丢；应用消费失败（处理器异常、反序列化失败、无注册处理器）的消息转入死信队列（默认 `eventbus_dlx_queue`），需人工或重试消费处理，不自动重回队列
5. 发布侧带指数退避重试（默认 10 次）；broker 不可用时消费侧自动指数退避重连
6. `SubscriptionClientName` 是本服务的消费队列名，每个服务必须唯一：多个服务共用同名队列时，同一条消息只会投递给其中一个服务
7. 发布未订阅的事件类型时，消息会被 broker 退回并记录 Warning 日志（`mandatory: true`）
8. 事件类改名后 routingKey 随之变化，旧队列绑定收不到新事件名，需通过运维手段迁移绑定

## 版本更新记录

* 1.2.0
    * 消费失败的消息转入死信队列（原实现无条件 ACK 会丢消息）
    * 发布复用单一 channel，修复每次发布新建 channel 的泄漏
    * broker 不可用时消费端指数退避自动重连（原实现只尝试一次，失败后消费端永久失效）
    * 发布重试管道覆盖 channel 级异常（AlreadyClosedException / OperationInterruptedException）
    * 处理器收到 CancellationToken；应用停机时优雅退出消费循环
    * 启动期配置校验：SubscriptionClientName 必填
    * 处理 mandatory 消息的 BasicReturn，未路由消息记录 Warning 而非静默丢弃
    * 未声明 AOT 兼容（RabbitMQ.Client 6.x 限制），并补 Nullable 支持
* 1.1.0
    * 支持.Net10并优化
* 1.0.0
    * 初始版本
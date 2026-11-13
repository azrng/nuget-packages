using Azrng.EventBus.Core.Abstractions;
using Azrng.EventBus.Core.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Polly.Retry;

namespace Azrng.EventBus.RabbitMQ;

/// <summary>
/// RabbitMQ 事件总线实现
/// </summary>
public sealed class RabbitMQEventBus : EventBusBase, IEventBus, IDisposable, IHostedService
{
    private readonly ResiliencePipeline _pipeline;
    private readonly IServiceProvider _serviceProvider;
    private readonly RabbitmqEventBusOptions _rabbitmqEventBus;

    // 消费端：后台消费循环持有，停机时受 _stoppingCts 控制退出
    private IModel? _consumerChannel;
    private IConnection? _rabbitMqConnection;

    // 发布端：复用单个 channel（channel 非线程安全，所有发布经 _publishLock 串行化）
    private IModel? _publisherChannel;
    private readonly object _publishLock = new();

    // 停机信号：停止消费循环、发布重试与后台重连
    private readonly CancellationTokenSource _stoppingCts = new();
    private readonly TaskCompletionSource _consumerStarted = new(TaskCreationOptions.RunContinuationsAsynchronously);


    public RabbitMQEventBus(
        IServiceProvider serviceProvider,
        IOptions<RabbitmqEventBusOptions> options,
        IOptions<EventBusSubscriptionInfo> subscriptionOptions)
        : base(serviceProvider.GetRequiredService<ILogger<RabbitMQEventBus>>(), subscriptionOptions)
    {
        _serviceProvider = serviceProvider;
        _rabbitmqEventBus = options.Value;
        _pipeline = CreateResiliencePipeline(_rabbitmqEventBus.RetryCount);
    }

    /// <summary>
    /// 发布事件到 RabbitMQ
    /// </summary>
    /// <param name="event">要发布的事件</param>
    /// <param name="cancellationToken">取消令牌</param>
    public Task PublishAsync(IntegrationEvent @event, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(@event);
        var routingKey = @event.GetType().Name;

        if (Logger.IsEnabled(LogLevel.Trace))
        {
            Logger.LogTrace("Publishing RabbitMQ event: {EventId} ({EventName})", @event.Id, routingKey);
        }

        var body = SerializeMessageToUtf8Bytes(@event);

        // channel 创建、Exchange 声明与 BasicPublish 一起进重试管道：
        // 连接闪断时 CreateModel / ExchangeDeclare / BasicPublish 抛出的异常都需要重试
        return _pipeline.ExecuteAsync(ct =>
        {
            // 复用发布 channel，避免每次发布新建导致 channel 泄漏
            var channel = GetOrCreatePublisherChannel();

            var properties = channel.CreateBasicProperties();
            properties.DeliveryMode = 2; // 持久化
            properties.MessageId = @event.Id.ToString();

            channel.BasicPublish(
                exchange: _rabbitmqEventBus.ExchangeName,
                routingKey: routingKey,
                mandatory: true,
                basicProperties: properties,
                body: body);

            // BasicReturn 事件回调携带原始 deliveryTag 与 ReplyText，足够定位被退回的消息

            return ValueTask.CompletedTask;
        }, cancellationToken).AsTask();
    }

    /// <summary>
    /// 获取或创建发布专用 channel；连接未建立时抛 BrokerUnreachableException，由重试管道按退避重试
    /// （消费端后台循环负责持续建连，这里不做第二次阻塞等待）
    /// </summary>
    private IModel GetOrCreatePublisherChannel()
    {
        lock (_publishLock)
        {
            if (_publisherChannel is { IsOpen: true })
            {
                return _publisherChannel;
            }

            _publisherChannel?.Dispose();
            _publisherChannel = null;

            var connection = _rabbitMqConnection;
            if (connection is not { IsOpen: true })
            {
                // 消费端后台循环正在指数退避重连，这里抛可重试异常交给 Polly 管道
                throw new BrokerUnreachableException(new InvalidOperationException("RabbitMQ connection is not open"));
            }

            _publisherChannel = connection.CreateModel();
            _publisherChannel.ExchangeDeclare(exchange: _rabbitmqEventBus.ExchangeName, type: "direct");

            // mandatory: true 的消息在无队列绑定时会被 broker 退回；
            // 不处理 BasicReturn 消息会被静默丢弃且发布方误以为成功
            _publisherChannel.BasicReturn += (_, ea) =>
            {
                Logger.LogWarning(
                    "RabbitMQ message returned (no queue bound): Exchange={Exchange} RoutingKey={RoutingKey} ReplyCode={ReplyCode} ReplyText={ReplyText}",
                    ea.Exchange, ea.RoutingKey, ea.ReplyCode, ea.ReplyText);
            };

            return _publisherChannel;
        }
    }


    public void Dispose()
    {
        _stoppingCts.Cancel();
        _stoppingCts.Dispose();
        _publisherChannel?.Dispose();
        _consumerChannel?.Dispose();
    }

    /// <summary>
    /// 后台任务接收消息
    /// </summary>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        // Messaging is async so we don't need to wait for it to complete. On top of this
        // the APIs are blocking, so we need to run this on a background thread.
        _ = Task.Factory.StartNew(() => RunConsumerLoop(cancellationToken),
            CancellationToken.None,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default);

        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _stoppingCts.Cancel();

        try
        {
            // 等待消费循环退出，避免停机时正在处理的消息被硬终止
            await _consumerStarted.Task.ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // 后台任务尚未启动即停机，无需处理
        }
    }

    /// <summary>
    /// 消费主循环：broker 暂不可用（应用先于 broker 启动、网络闪断）时指数退避重试，
    /// 直到连接成功或收到停机信号；避免只尝试一次导致消费端永久失效
    /// </summary>
    private void RunConsumerLoop(CancellationToken startupToken)
    {
        var attempt = 0;
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(startupToken, _stoppingCts.Token);

        while (!linked.IsCancellationRequested)
        {
            try
            {
                if (Logger.IsEnabled(LogLevel.Information) && attempt > 0)
                {
                    Logger.LogInformation("Retrying RabbitMQ connection (attempt {Attempt})", attempt + 1);
                }

                // 每轮重连通过工厂新建连接（容器缓存的 IConnection 单例可能是断线旧实例）
                var oldConnection = _rabbitMqConnection;
                oldConnection?.Dispose();
                _rabbitMqConnection = _serviceProvider.GetRequiredService<IConnectionFactory>().CreateConnection();
                if (!_rabbitMqConnection.IsOpen)
                {
                    throw new InvalidOperationException("RabbitMQ connection is not open");
                }

                if (Logger.IsEnabled(LogLevel.Trace))
                {
                    Logger.LogTrace("Creating RabbitMQ consumer channel");
                }

                StartConsuming(_rabbitMqConnection);

                // 连接建立并开始消费后保持阻塞，直到连接被外部关闭（broker 重启、网络断开）
                // 或收到停机信号；ConnectionShutdown 事件负责唤醒
                var shutdownTcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
                _rabbitMqConnection.ConnectionShutdown += OnShutdown;

                try
                {
                    linked.Token.Register(() => shutdownTcs.TrySetResult());
                    shutdownTcs.Task.Wait(linked.Token);
                }
                finally
                {
                    _rabbitMqConnection.ConnectionShutdown -= OnShutdown;
                }

                if (linked.IsCancellationRequested)
                {
                    break;
                }

                // 连接被关闭但未停机 → 断线重连
                attempt = 0;

                void OnShutdown(object? sender, ShutdownEventArgs e)
                {
                    shutdownTcs.TrySetResult();
                }
            }
            catch (OperationCanceledException) when (linked.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                attempt++;
                var delay = TimeSpan.FromSeconds(Math.Min(Math.Pow(2, Math.Min(attempt, 6)), 60));
                if (Logger.IsEnabled(LogLevel.Error))
                {
                    Logger.LogError(ex, "RabbitMQ connection failed, retrying in {Delay}s (attempt {Attempt})",
                        delay.TotalSeconds, attempt);
                }

                try
                {
                    Task.Delay(delay, linked.Token).Wait(linked.Token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }

        _consumerChannel?.Dispose();
        _consumerChannel = null;
        _consumerStarted.TrySetResult();
    }

    /// <summary>
    /// 建立消费 channel、声明队列（含死信交换机）并绑定全部事件类型
    /// </summary>
    private void StartConsuming(IConnection connection)
    {
        _consumerChannel = connection.CreateModel();

        _consumerChannel.CallbackException += (_, ea) =>
        {
            Logger.LogWarning(ea.Exception, "Error with RabbitMQ consumer channel");
        };

        _consumerChannel.ExchangeDeclare(exchange: _rabbitmqEventBus.ExchangeName, type: "direct");

        // 消费失败的消息进入死信交换机，由死信队列留存，而不是随 ACK 一起被丢弃
        _consumerChannel.ExchangeDeclare(exchange: _rabbitmqEventBus.DeadLetterExchangeName, type: "fanout");
        _consumerChannel.QueueDeclare(
            queue: _rabbitmqEventBus.DeadLetterQueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);
        _consumerChannel.QueueBind(
            queue: _rabbitmqEventBus.DeadLetterQueueName,
            exchange: _rabbitmqEventBus.DeadLetterExchangeName,
            routingKey: string.Empty);

        _consumerChannel.QueueDeclare(
            queue: _rabbitmqEventBus.SubscriptionClientName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object>
            {
                // x-dead-letter-exchange：消费端 nack(requeue:false) 的消息转入死信交换机
                ["x-dead-letter-exchange"] = _rabbitmqEventBus.DeadLetterExchangeName
            });

        if (Logger.IsEnabled(LogLevel.Trace))
        {
            Logger.LogTrace("Starting RabbitMQ basic consume");
        }

        // ConnectionFactory 开启 DispatchConsumersAsync 时必须使用异步消费者，否则收不到消息
        var consumer = new AsyncEventingBasicConsumer(_consumerChannel);
        consumer.Received += OnMessageReceived;
        _consumerChannel.BasicConsume(
            queue: _rabbitmqEventBus.SubscriptionClientName,
            autoAck: false,
            consumer: consumer);

        foreach (var (eventName, _) in SubscriptionInfo.EventTypes)
        {
            _consumerChannel.QueueBind(
                queue: _rabbitmqEventBus.SubscriptionClientName,
                exchange: _rabbitmqEventBus.ExchangeName,
                routingKey: eventName);
        }
    }

    private async Task OnMessageReceived(object? sender, BasicDeliverEventArgs eventArgs)
    {
        var eventName = eventArgs.RoutingKey;
        var message = Encoding.UTF8.GetString(eventArgs.Body.Span);
        var channel = (sender as AsyncEventingBasicConsumer)?.Model ?? _consumerChannel;
        if (channel is null)
        {
            return;
        }

        try
        {
            // 全部处理器成功才 ack；任一失败走 catch nack 进死信队列
            if (await ProcessEventAsync(eventName, message, _stoppingCts.Token).ConfigureAwait(false))
            {
                channel.BasicAck(eventArgs.DeliveryTag, multiple: false);
            }
            else
            {
                // 无法解析事件类型 / 无注册处理器：消息无法在本地处理，留痕进死信队列
                Logger.LogWarning("Nacking message \"{EventName}\" (deliveryTag {DeliveryTag}) to dead letter queue",
                    eventName, eventArgs.DeliveryTag);
                channel.BasicNack(eventArgs.DeliveryTag, multiple: false, requeue: false);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error processing message \"{EventName}\" (deliveryTag {DeliveryTag}), routing to dead letter queue",
                eventName, eventArgs.DeliveryTag);

            // 处理失败不再无条件 ACK 丢弃消息，而是 nack(requeue:false) 送入死信交换机；
            // 毒消息（反序列化失败等）反复 requeue 会造成消费死循环，因此不重回原队列
            try
            {
                channel.BasicNack(eventArgs.DeliveryTag, multiple: false, requeue: false);
            }
            catch (ObjectDisposedException)
            {
                // 停机时 channel 已关闭，消息由 broker 重新投递
            }
        }
    }

    /// <summary>
    /// 处理接收到的事件；返回是否成功（失败由调用方决定 ack/nack）
    /// </summary>
    private async Task<bool> ProcessEventAsync(string eventName, string message, CancellationToken cancellationToken)
    {
        if (Logger.IsEnabled(LogLevel.Trace))
        {
            Logger.LogTrace("Processing RabbitMQ event: {EventName}", eventName);
        }

        await using var scope = _serviceProvider.CreateAsyncScope();

        if (!SubscriptionInfo.EventTypes.TryGetValue(eventName, out var eventType))
        {
            // 未知事件类型：无法处理，nack 进死信队列留痕（比静默丢弃可观测）
            Logger.LogWarning("Unable to resolve event type for event name {EventName}", eventName);
            return false;
        }

        // 反序列化失败（毒消息）通过抛出 JsonException 交给调用方 nack 进死信队列
        var integrationEvent = DeserializeMessage(message, eventType)
                               ?? throw new JsonException($"Failed to deserialize event {eventName}");

        // 获取所有事件处理器并执行
        var handlers = scope.ServiceProvider.GetKeyedServices<IIntegrationEventHandler>(eventType).ToList();

        if (handlers.Count == 0)
        {
            Logger.LogWarning("No handlers registered for event {EventName}", eventName);
            return false;
        }

        var failures = 0;

        // 并行执行所有事件处理器：单个处理器失败不阻断其他处理器，但计入失败
        await Task.WhenAll(handlers.Select(async handler =>
        {
            try
            {
                await handler.Handle(integrationEvent, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Interlocked.Increment(ref failures);
                Logger.LogError(ex, "Error processing event {EventName} with handler {HandlerType}",
                    eventName, handler.GetType().Name);
            }
        })).ConfigureAwait(false);

        // 任一处理器失败即视为整体失败：消息进死信队列等待人工或重试消费
        return failures == 0;
    }

    /// <summary>
    /// 创建重试管道
    /// </summary>
    private static ResiliencePipeline CreateResiliencePipeline(int retryCount)
    {
        // See https://pollydocs.org/strategies/retry.html
        var retryOptions = new RetryStrategyOptions
        {
            ShouldHandle = new PredicateBuilder()
                .Handle<BrokerUnreachableException>()
                .Handle<SocketException>()
                // 连接闪断后 channel 上的操作抛出这两类异常，同样需要重试
                .Handle<AlreadyClosedException>()
                .Handle<OperationInterruptedException>(),
            MaxRetryAttempts = retryCount,
            DelayGenerator = (context) => ValueTask.FromResult(GenerateDelay(context.AttemptNumber))
        };

        return new ResiliencePipelineBuilder()
            .AddRetry(retryOptions)
            .Build();

        static TimeSpan? GenerateDelay(int attempt)
        {
            return TimeSpan.FromSeconds(Math.Pow(2, attempt));
        }
    }
}

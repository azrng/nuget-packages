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
    private IConnection _rabbitMqConnection;
    private IModel _consumerChannel;
    private readonly IServiceProvider _serviceProvider;
    private readonly RabbitmqEventBusOptions _rabbitmqEventBus;

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
        var routingKey = @event.GetType().Name;

        if (Logger.IsEnabled(LogLevel.Trace))
        {
            Logger.LogTrace("Creating RabbitMQ channel to publish event: {EventId} ({EventName})", @event.Id, routingKey);
        }

        var channel = _rabbitMqConnection?.CreateModel() ??
                     throw new InvalidOperationException("RabbitMQ connection is not open");

        if (Logger.IsEnabled(LogLevel.Trace))
        {
            Logger.LogTrace("Declaring RabbitMQ exchange to publish event: {EventId}", @event.Id);
        }

        channel.ExchangeDeclare(exchange: _rabbitmqEventBus.ExchangeName, type: "direct");

        var body = SerializeMessageToUtf8Bytes(@event);

        return _pipeline.Execute(async (ct) =>
        {
            var properties = channel.CreateBasicProperties();
            properties.DeliveryMode = 2; // 持久化

            if (Logger.IsEnabled(LogLevel.Trace))
            {
                Logger.LogTrace("Publishing event to RabbitMQ: {EventId}", @event.Id);
            }

            await Task.CompletedTask;
            channel.BasicPublish(
                exchange: _rabbitmqEventBus.ExchangeName,
                routingKey: routingKey,
                mandatory: true,
                basicProperties: properties,
                body: body);
        }, cancellationToken);
    }

    public void Dispose()
    {
        _consumerChannel?.Dispose();
    }

    private async Task OnMessageReceived(object sender, BasicDeliverEventArgs eventArgs)
    {
        var eventName = eventArgs.RoutingKey;
        var message = Encoding.UTF8.GetString(eventArgs.Body.Span);

        try
        {
            if (message.Contains("throw-fake-exception", StringComparison.InvariantCultureIgnoreCase))
            {
                throw new InvalidOperationException($"Fake exception requested: \"{message}\"");
            }

            await ProcessEventAsync(eventName, message);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Error Processing message \"{Message}\"", message);
        }
        finally
        {
            // Even on exception we take the message off the queue.
            // in a REAL WORLD app this should be handled with a Dead Letter Exchange (DLX).
            // For more information see: https://www.rabbitmq.com/dlx.html
            _consumerChannel.BasicAck(eventArgs.DeliveryTag, multiple: false);
        }
    }

    /// <summary>
    /// 处理接收到的事件
    /// </summary>
    private async Task ProcessEventAsync(string eventName, string message)
    {
        if (Logger.IsEnabled(LogLevel.Trace))
        {
            Logger.LogTrace("Processing RabbitMQ event: {EventName}", eventName);
        }

        await using var scope = _serviceProvider.CreateAsyncScope();

        if (!SubscriptionInfo.EventTypes.TryGetValue(eventName, out var eventType))
        {
            Logger.LogWarning("Unable to resolve event type for event name {EventName}", eventName);
            return;
        }

        // 反序列化事件
        var integrationEvent = DeserializeMessage(message, eventType);
        if (integrationEvent == null)
        {
            Logger.LogError("Failed to deserialize event {EventName}", eventName);
            return;
        }

        // 获取所有事件处理器并执行
        var handlers = scope.ServiceProvider.GetKeyedServices<IIntegrationEventHandler>(eventType).ToList();

        if (handlers.Count == 0)
        {
            Logger.LogWarning("No handlers registered for event {EventName}", eventName);
            return;
        }

        // 并行执行所有事件处理器
        var handlerTasks = handlers.Select(async handler =>
        {
            try
            {
                await handler.Handle(integrationEvent);
            }
            catch (Exception ex)
            {
                // 错误隔离：一个处理器失败不影响其他处理器
                Logger.LogError(ex, "Error processing event {EventName} with handler {HandlerType}",
                    eventName, handler.GetType().Name);
            }
        });

        await Task.WhenAll(handlerTasks);
    }

    /// <summary>
    /// 后台任务接收消息
    /// </summary>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        // Messaging is async so we don't need to wait for it to complete. On top of this
        // the APIs are blocking, so we need to run this on a background thread.
        _ = Task.Factory.StartNew(() =>
            {
                try
                {
                    Logger.LogInformation("Starting RabbitMQ connection on a background thread");

                    _rabbitMqConnection = _serviceProvider.GetRequiredService<IConnection>();
                    if (!_rabbitMqConnection.IsOpen)
                    {
                        return;
                    }

                    if (Logger.IsEnabled(LogLevel.Trace))
                    {
                        Logger.LogTrace("Creating RabbitMQ consumer channel");
                    }

                    _consumerChannel = _rabbitMqConnection.CreateModel();

                    _consumerChannel.CallbackException += (sender, ea) =>
                    {
                        Logger.LogWarning(ea.Exception, "Error with RabbitMQ consumer channel");
                    };

                    _consumerChannel.ExchangeDeclare(exchange: _rabbitmqEventBus.ExchangeName, type: "direct");

                    _consumerChannel.QueueDeclare(
                        queue: _rabbitmqEventBus.SubscriptionClientName,
                        durable: true,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null);

                    if (Logger.IsEnabled(LogLevel.Trace))
                    {
                        Logger.LogTrace("Starting RabbitMQ basic consume");
                    }

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
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Error starting RabbitMQ connection");
                }
            },
            cancellationToken,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// 创建重试管道
    /// </summary>
    private static ResiliencePipeline CreateResiliencePipeline(int retryCount)
    {
        // See https://www.pollydocs.org/strategies/retry.html
        var retryOptions = new RetryStrategyOptions
        {
            ShouldHandle = new PredicateBuilder()
                .Handle<BrokerUnreachableException>()
                .Handle<SocketException>(),
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

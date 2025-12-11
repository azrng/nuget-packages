using System.Diagnostics.CodeAnalysis;
using Azrng.EventBus.Core.Abstractions;
using Azrng.EventBus.Core.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly.Retry;

namespace Azrng.EventBus.RabbitMQ;

/// <summary>
/// rabbitmq 事件总线实现
/// </summary>
public sealed class RabbitMQEventBus : IEventBus, IDisposable, IHostedService
{
    private readonly ResiliencePipeline _pipeline;
    private IConnection _rabbitMqConnection;
    private IModel _consumerChannel;
    private readonly ILogger<RabbitMQEventBus> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly EventBusSubscriptionInfo _subscriptionInfo;
    private readonly RabbitmqEventBusOptions _rabbitmqEventBus;

    public RabbitMQEventBus(ILogger<RabbitMQEventBus> logger, IServiceProvider serviceProvider,
                            IOptions<RabbitmqEventBusOptions> options, IOptions<EventBusSubscriptionInfo> subscriptionOptions)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _subscriptionInfo = subscriptionOptions.Value;
        _rabbitmqEventBus = options.Value;
        _pipeline = CreateResiliencePipeline(_rabbitmqEventBus.RetryCount);
    }

    public Task PublishAsync(IntegrationEvent @event)
    {
        var routingKey = @event.GetType().Name;

        if (_logger.IsEnabled(LogLevel.Trace))
        {
            _logger.LogTrace("Creating RabbitMQ channel to publish event: {EventId} ({EventName})", @event.Id,
                routingKey);
        }

        using var channel = _rabbitMqConnection?.CreateModel() ??
                            throw new InvalidOperationException("RabbitMQ connection is not open");

        if (_logger.IsEnabled(LogLevel.Trace))
        {
            _logger.LogTrace("Declaring RabbitMQ exchange to publish event: {EventId}", @event.Id);
        }

        channel.ExchangeDeclare(exchange: _rabbitmqEventBus.ExchangeName, type: "direct");

        var body = SerializeMessage(@event);

        return _pipeline.Execute(() =>
        {
            // Depending on Sampling (and whether a listener is registered or not), the activity above may not be created.
            // If it is created, then propagate its context. If it is not created, the propagate the Current context, if any.

            var properties = channel.CreateBasicProperties();

            // persistent
            properties.DeliveryMode = 2;

            if (_logger.IsEnabled(LogLevel.Trace))
            {
                _logger.LogTrace("Publishing event to RabbitMQ: {EventId}", @event.Id);
            }

            channel.BasicPublish(exchange: _rabbitmqEventBus.ExchangeName,
                routingKey: routingKey,
                mandatory: true,
                basicProperties: properties,
                body: body);

            return Task.CompletedTask;
        });
    }

    public void Dispose()
    {
        _consumerChannel?.Dispose();
    }

    private async Task OnMessageReceived(object sender, BasicDeliverEventArgs eventArgs)
    {
        // Extract the PropagationContext of the upstream parent from the message headers.
        var eventName = eventArgs.RoutingKey;
        var message = Encoding.UTF8.GetString(eventArgs.Body.Span);

        try
        {
            if (message.Contains("throw-fake-exception", StringComparison.InvariantCultureIgnoreCase))
            {
                throw new InvalidOperationException($"Fake exception requested: \"{message}\"");
            }

            await ProcessEvent(eventName, message);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error Processing message \"{Message}\"", message);

            //activity.SetExceptionTags(ex);
        }

        // Even on exception we take the message off the queue.
        // in a REAL WORLD app this should be handled with a Dead Letter Exchange (DLX).
        // For more information see: https://www.rabbitmq.com/dlx.html
        _consumerChannel.BasicAck(eventArgs.DeliveryTag, multiple: false);
    }

    private async Task ProcessEvent(string eventName, string message)
    {
        if (_logger.IsEnabled(LogLevel.Trace))
        {
            _logger.LogTrace("Processing RabbitMQ event: {EventName}", eventName);
        }

        await using var scope = _serviceProvider.CreateAsyncScope();

        if (!_subscriptionInfo.EventTypes.TryGetValue(eventName, out var eventType))
        {
            _logger.LogWarning("Unable to resolve event type for event name {EventName}", eventName);
            return;
        }

        // Deserialize the event
        var integrationEvent = DeserializeMessage(message, eventType);

        // REVIEW: This could be done in parallel

        // Get all the handlers using the event type as the key
        foreach (var handler in scope.ServiceProvider.GetKeyedServices<IIntegrationEventHandler>(eventType))
        {
            await handler.Handle(integrationEvent);
        }
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026:RequiresUnreferencedCode",
        Justification =
            "The 'JsonSerializer.IsReflectionEnabledByDefault' feature switch, which is set to false by default for trimmed .NET apps, ensures the JsonSerializer doesn't use Reflection.")]
    [UnconditionalSuppressMessage("AOT", "IL3050:RequiresDynamicCode", Justification = "See above.")]
    private IntegrationEvent DeserializeMessage(string message, Type eventType)
    {
        return JsonSerializer.Deserialize(message, eventType, _subscriptionInfo.JsonSerializerOptions) as
            IntegrationEvent;
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026:RequiresUnreferencedCode",
        Justification =
            "The 'JsonSerializer.IsReflectionEnabledByDefault' feature switch, which is set to false by default for trimmed .NET apps, ensures the JsonSerializer doesn't use Reflection.")]
    [UnconditionalSuppressMessage("AOT", "IL3050:RequiresDynamicCode", Justification = "See above.")]
    private byte[] SerializeMessage(IntegrationEvent @event)
    {
        return JsonSerializer.SerializeToUtf8Bytes(@event, @event.GetType(), _subscriptionInfo.JsonSerializerOptions);
    }

    /// <summary>
    /// 后台任务接收消息
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        // Messaging is async so we don't need to wait for it to complete. On top of this
        // the APIs are blocking, so we need to run this on a background thread.
        _ = Task.Factory.StartNew(() =>
            {
                try
                {
                    _logger.LogInformation("Starting RabbitMQ connection on a background thread");

                    _rabbitMqConnection = _serviceProvider.GetRequiredService<IConnection>();
                    if (!_rabbitMqConnection.IsOpen)
                    {
                        return;
                    }

                    if (_logger.IsEnabled(LogLevel.Trace))
                    {
                        _logger.LogTrace("Creating RabbitMQ consumer channel");
                    }

                    _consumerChannel = _rabbitMqConnection.CreateModel();

                    _consumerChannel.CallbackException += (sender, ea) =>
                    {
                        _logger.LogWarning(ea.Exception, "Error with RabbitMQ consumer channel");
                    };

                    _consumerChannel.ExchangeDeclare(exchange: _rabbitmqEventBus.ExchangeName,
                        type: "direct");

                    _consumerChannel.QueueDeclare(queue: _rabbitmqEventBus.SubscriptionClientName,
                        durable: true,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null);

                    if (_logger.IsEnabled(LogLevel.Trace))
                    {
                        _logger.LogTrace("Starting RabbitMQ basic consume");
                    }

                    var consumer = new AsyncEventingBasicConsumer(_consumerChannel);

                    consumer.Received += OnMessageReceived;

                    _consumerChannel.BasicConsume(queue: _rabbitmqEventBus.SubscriptionClientName,
                        autoAck: false,
                        consumer: consumer);

                    foreach (var (eventName, _) in _subscriptionInfo.EventTypes)
                    {
                        _consumerChannel.QueueBind(queue: _rabbitmqEventBus.SubscriptionClientName,
                            exchange: _rabbitmqEventBus.ExchangeName,
                            routingKey: eventName);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error starting RabbitMQ connection");
                }
            },
            TaskCreationOptions.LongRunning);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// 创建重试管道
    /// </summary>
    /// <param name="retryCount"></param>
    /// <returns></returns>
    private static ResiliencePipeline CreateResiliencePipeline(int retryCount)
    {
        // See https://www.pollydocs.org/strategies/retry.html
        var retryOptions = new RetryStrategyOptions
                           {
                               ShouldHandle = new PredicateBuilder().Handle<BrokerUnreachableException>().Handle<SocketException>(),
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
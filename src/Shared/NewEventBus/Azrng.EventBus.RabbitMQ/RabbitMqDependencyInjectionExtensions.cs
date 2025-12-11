using Azrng.EventBus.Core.Abstractions;
using Azrng.EventBus.RabbitMQ;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Extensions.DependencyInjection;

public static class RabbitMqDependencyInjectionExtensions
{
    /// <summary>
    /// 添加 RabbitMq 的事件总线
    /// </summary>
    /// <param name="services"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public static IEventBusBuilder AddRabbitMqEventBus(this IServiceCollection services,
                                                       Action<RabbitmqEventBusOptions> action)
    {
        ArgumentNullException.ThrowIfNull(services);

        var config = new RabbitmqEventBusOptions();
        action.Invoke(config);

        // Options support
        services.Configure(action);

        var factory = new ConnectionFactory
                      {
                          HostName = config.HostName,
                          DispatchConsumersAsync = true,
                          VirtualHost = config.VirtualHost,
                          UserName = config.UserName,
                          Password = config.Password,
                          Port = config.Port
                      };

        services.AddSingleton<IConnectionFactory>(factory);
        services.AddSingleton<IConnection>((Func<IServiceProvider, IConnection>)(sp =>
            sp.GetRequiredService<IConnectionFactory>().CreateConnection()));

        services.AddSingleton<IEventBus, RabbitMQEventBus>();

        // Start consuming messages as soon as the application starts
        services.AddSingleton<IHostedService>(sp => (RabbitMQEventBus)sp.GetRequiredService<IEventBus>());

        return new EventBusBuilder(services);
    }

    private class EventBusBuilder(IServiceCollection services) : IEventBusBuilder
    {
        public IServiceCollection Services => services;
    }
}
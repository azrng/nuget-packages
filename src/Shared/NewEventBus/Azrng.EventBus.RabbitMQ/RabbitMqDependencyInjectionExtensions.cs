using Azrng.EventBus.Core.Abstractions;
using Azrng.EventBus.RabbitMQ;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

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
        ArgumentNullException.ThrowIfNull(action);

        // Options 支持与启动期校验：连接参数缺失或队列名未设置时启动即失败，
        // 而不是等到运行期 NRE 或多服务共用默认队列互相抢消息
        services.AddOptions<RabbitmqEventBusOptions>()
            .Configure(action)
            .Validate(o => !string.IsNullOrWhiteSpace(o.SubscriptionClientName),
                "SubscriptionClientName 必须设置：每个服务的消费队列名必须唯一，共用会互相抢消息")
            .ValidateOnStart();

        // IConnection 注册保留兼容（供用户直接注入）；事件总线自身通过工厂在每次重连时新建连接。
        // 不启用 AutomaticRecoveryEnabled：断线恢复由 RabbitMQEventBus 消费循环统一负责，
        // 避免客户端自动重放消费者与总线重建消费 channel 叠加造成重复消费
        services.AddSingleton<IConnectionFactory>(sp =>
        {
            var config = sp.GetRequiredService<IOptions<RabbitmqEventBusOptions>>().Value;
            return new ConnectionFactory
                   {
                       HostName = config.HostName,
                       DispatchConsumersAsync = true,
                       VirtualHost = config.VirtualHost,
                       UserName = config.UserName,
                       Password = config.Password,
                       Port = config.Port,
                       AutomaticRecoveryEnabled = false
                   };
        });
        services.AddSingleton<IConnection>(sp =>
            sp.GetRequiredService<IConnectionFactory>().CreateConnection());

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

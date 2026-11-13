namespace Azrng.EventBus.RabbitMQ;

/// <summary>
/// RabbitMQ 事件总线配置选项
/// </summary>
public class RabbitmqEventBusOptions
{
    /// <summary>
    /// host 地址
    /// </summary>
    public string HostName { get; set; } = "localhost";

    /// <summary>
    /// 虚拟 host
    /// </summary>
    public string VirtualHost { get; set; } = "/";

    /// <summary>
    /// 用户名
    /// </summary>
    public string UserName { get; set; } = "guest";

    /// <summary>
    /// 密码
    /// </summary>
    public string Password { get; set; } = "guest";

    /// <summary>
    /// 端口
    /// </summary>
    public int Port { get; set; } = 5672;

    /// <summary>
    /// 订阅客户端名字（消费队列名）。每个服务必须唯一：多个服务共用同名队列会互相抢消息
    /// </summary>
    public string SubscriptionClientName { get; set; } = string.Empty;

    /// <summary>
    /// 发布重试次数
    /// </summary>
    public int RetryCount { get; set; } = 10;

    /// <summary>
    /// 交换机名称（事件发布到该交换机）
    /// </summary>
    public string ExchangeName { get; set; } = "direct_exchange";

    /// <summary>
    /// 死信交换机名称（消费失败的消息转入该交换机，避免随 ACK 丢失）
    /// </summary>
    public string DeadLetterExchangeName { get; set; } = "eventbus_dlx";

    /// <summary>
    /// 死信队列名称（留存消费失败的消息，供人工排查或重试消费）
    /// </summary>
    public string DeadLetterQueueName { get; set; } = "eventbus_dlx_queue";
}

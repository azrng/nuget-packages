namespace Azrng.EventBus.RabbitMQ;

public class RabbitmqEventBusOptions
{
    /// <summary>
    /// host地址
    /// </summary>
    public string HostName { get; set; }

    /// <summary>
    /// 虚拟 host
    /// </summary>
    public string VirtualHost { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// 密码
    /// </summary>
    public string Password { get; set; }

    /// <summary>
    /// 端口
    /// </summary>
    public int Port { get; set; } = 5672;

    /// <summary>
    /// 订阅客户端名字
    /// </summary>
    public string SubscriptionClientName { get; set; } = "defaultQueue";

    /// <summary>
    /// 重试次数
    /// </summary>
    public int RetryCount { get; set; } = 10;

    /// <summary>
    /// 交换机名称(发送事件总线给给交换机)
    /// </summary>
    public string ExchangeName { get; set; } = "direct_exchange";
}
using StackExchange.Redis;
using System.Threading.Tasks;

namespace Common.Cache.Redis
{
    /// <summary>
    /// Redis 连接工厂抽象，便于在测试中替换为 Fake 实现。
    /// </summary>
    internal interface IRedisConnectionFactory
    {
        Task<IRedisConnection> ConnectAsync(ConfigurationOptions configurationOptions);
    }
}

using System;

namespace Common.Cache.Redis
{
    /// <summary>
    /// Redis 连接抽象，提供获取 Database 与 Subscriber 的能力。
    /// </summary>
    internal interface IRedisConnection : IDisposable
    {
        IRedisDatabase GetDatabase();

        IRedisSubscriber GetSubscriber();
    }
}

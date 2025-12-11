namespace Azrng.DistributeLock.Redis
{
    /// <summary>
    /// redis锁配置类
    /// </summary>
    public class RedisLockOptions
    {
        /// <summary>
        /// Redis连接字符串
        /// 示例：127.0.0.1:6379,defaultDatabase=1,connectTimeout=100000,syncTimeout=100000,connectRetry=50
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// 默认的锁超时时间
        /// </summary>
        public TimeSpan DefaultExpireTime { get; set; }
    }
}
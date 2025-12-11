namespace Common.Cache.Redis
{
    /// <summary>
    /// redis配置信息
    /// </summary>
    public class RedisConfig
    {
        /// <summary>
        /// 连接字符串
        /// </summary>
        public string ConnectionString { get; set; } = "localhost:6379,DefaultDatabase=0";

        /// <summary>
        /// 实例名
        /// </summary>
        public string KeyPrefix { get; set; } = "default";

        /// <summary>
        /// 距离上一次初始化错误的间隔时间(默认10秒)
        /// </summary>
        public int InitErrorIntervalSecond { get; set; } = 10;

        // /// <summary>
        // /// 获取缓存超时时间(默认为5s)
        // /// </summary>
        // public TimeSpan TimeoutTime { get; set; } = TimeSpan.FromSeconds(5);

        /// <summary>
        /// 是否缓存空集合和空字符串数据（默认为true，即缓存空集合和空字符串）
        /// </summary>
        public bool CacheEmptyCollections { get; set; } = true;
    }
}
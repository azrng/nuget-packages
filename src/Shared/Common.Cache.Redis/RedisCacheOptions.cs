namespace Common.Cache.Redis
{
    /// <summary>
    /// Redis 缓存配置选项
    /// </summary>
    public class RedisCacheOptions
    {
        /// <summary>
        /// 连接字符串
        /// </summary>
        public string ConnectionString { get; set; } = "localhost:6379,DefaultDatabase=0";

        /// <summary>
        /// Key 前缀
        /// </summary>
        public string KeyPrefix { get; set; } = "default";

        /// <summary>
        /// 距离上一次初始化错误的间隔时间(默认10秒)
        /// </summary>
        public int InitErrorIntervalSecond { get; set; } = 10;

        /// <summary>
        /// 是否缓存空集合和空字符串数据（默认为true，即缓存空集合和空字符串）
        /// </summary>
        public bool CacheEmptyCollections { get; set; } = true;

        /// <summary>
        /// 缓存操作失败时是否抛出异常（默认为true）
        /// </summary>
        public bool FailThrowException { get; set; } = true;
    }
}

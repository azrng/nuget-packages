using System;

namespace Azrng.Cache.MemoryCache
{
    /// <summary>
    /// 内存缓存配置
    /// </summary>
    public class MemoryConfig
    {
        /// <summary>
        /// 默认缓存时间(默认5s)
        /// </summary>
        public TimeSpan DefaultExpiry { get; set; } = TimeSpan.FromSeconds(5);

        /// <summary>
        /// 是否缓存空集合和空字符串数据（默认为true，即缓存空集合和空字符串）
        /// </summary>
        public bool CacheEmptyCollections { get; set; } = true;
    }
}
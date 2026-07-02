using System;

namespace Azrng.Cache.MemoryCache
{
    /// <summary>
    /// 内存缓存配置选项
    /// </summary>
    public class MemoryCacheOptions
    {
        /// <summary>
        /// 默认缓存过期时间，默认值为 5 秒。
        /// 当 <c>SetAsync</c> / <c>GetOrCreateAsync</c> 未显式传入 expiry 时使用。
        /// </summary>
        public TimeSpan DefaultExpiry { get; set; } = TimeSpan.FromSeconds(5);

        /// <summary>
        /// 是否缓存空集合与空字符串。
        /// 默认值为 <c>true</c>，即缓存空集合、空字符串；<c>null</c> 始终不缓存。
        /// 合法的值类型默认值（如 <c>0</c>、<c>false</c>）不受此项影响，始终缓存。
        /// </summary>
        public bool CacheEmptyCollections { get; set; } = true;

        /// <summary>
        /// 缓存操作失败时是否抛出异常。
        /// 默认值为 <c>true</c>：记录日志后重新抛出，避免把真实故障误判为缓存未命中。
        /// 设为 <c>false</c>：记录日志后返回默认值，不抛出异常。
        /// </summary>
        public bool FailThrowException { get; set; } = true;
    }
}

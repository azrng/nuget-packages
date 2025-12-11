using Azrng.Cache.Core;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Azrng.Cache.MemoryCache
{
    /// <summary>
    /// 封装的memory cache
    /// </summary>
    public interface IMemoryCacheProvider : ICacheProvider
    {
        /// <summary>
        /// 获取缓存集合
        /// </summary>
        /// <param name="keys">缓存Key集合</param>
        /// <returns></returns>
        Task<Dictionary<string, object>> GetAllAsync(IEnumerable<string> keys);

        /// <summary>
        /// 获取所有缓存键
        /// </summary>
        /// <returns></returns>
        List<string> GetAllKeys();

        /// <summary>
        /// 删除所有缓存
        /// </summary>
        Task RemoveAllKeyAsync();
    }
}
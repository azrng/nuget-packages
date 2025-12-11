using System.Threading.Tasks;

namespace Azrng.Cache.FreeRedis
{
    public interface IRedisCache
    {
        /// <summary>
        /// 判断是否存在
        /// </summary>
        /// <param name="key">值</param>
        /// <returns></returns>
        Task<bool> ExistAsync(string key);

        /// <summary>
        /// 获取 Reids 缓存值
        /// </summary>
        /// <param name="key">键</param>
        /// <returns></returns>
        Task<string> GetStringAsync(string key);

        /// <summary>
        /// 获取值，并序列化
        /// </summary>
        /// <typeparam name="TEntity">实体</typeparam>
        /// <param name="key">键</param>
        /// <returns></returns>
        Task<TEntity> GetAsync<TEntity>(string key);

        /// <summary>
        /// 移除某一个缓存值
        /// </summary>
        /// <param name="key">移除</param>
        Task RemoveAsync(string key);

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <param name="timeoutSeconds">过期时间(秒)</param>
        Task SetAsync<T>(string key, T value, int timeoutSeconds);

        /// <summary>
        /// 保存hash
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="timeoutSeconds"></param>
        /// <returns></returns>
        Task SetHashAsync<T>(string key, T value, int timeoutSeconds);
    }
}
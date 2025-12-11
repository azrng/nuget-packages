using System.Collections.Generic;
using System.Threading.Tasks;

namespace Common.Cache.CSRedis
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
        /// 移除某一个缓存值
        /// </summary>
        /// <param name="key">移除</param>
        Task RemoveAsync(string key);

        #region string

        /// <summary>
        /// 获取 Reids 缓存值
        /// </summary>
        /// <typeparam name="T">string/int</typeparam>
        /// <param name="key">键</param>
        /// <returns></returns>
        Task<T> GetAsync<T>(string key);

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">字符串/数值</param>
        /// <param name="second">过期时间(秒)</param>
        /// <returns></returns>
        Task SetAsync(string key, object value, int second = 600);

        #endregion

        #region Hash

        /// <summary>
        /// 获取值，并序列化
        /// </summary>
        /// <typeparam name="T">实体</typeparam>
        /// <param name="key">键</param>
        /// <param name="field"></param>
        /// <returns></returns>
        Task<T> HGetAsync<T>(string key, string field = "data");

        /// <summary>
        /// 获取所有的hash的key和值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<Dictionary<string, string>> HGetALLAsync(string key);

        /// <summary>
        /// 获取所有的hash的key和值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<Dictionary<string, T>> HGetALLAsync<T>(string key) where T : class;

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="field"></param>
        /// <param name="value">实体</param>
        /// <param name="second">过期时间(秒)</param>
        Task HSetAsync<T>(string key, string field, T value, double second = 600.0);

        #endregion
    }
}
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Azrng.Cache.Core
{
    /// <summary>
    /// 缓存
    /// </summary>
    public interface ICacheProvider
    {
        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <param name="key">缓存Key</param>
        /// <returns></returns>
        Task<string> GetAsync(string key);

        /// <summary>
        /// 获取缓存,并序列化
        /// </summary>
        /// <param name="key">缓存Key</param>
        /// <returns></returns>
        Task<T> GetAsync<T>(string key);

        /// <summary>
        /// 查询数据,如果不存在就添加
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">缓存的key</param>
        /// <param name="getData">提供数据的委托</param>
        /// <param name="expiry">缓存过期时间</param>
        /// <returns></returns>
        Task<T> GetOrCreateAsync<T>(string key, Func<T> getData, TimeSpan? expiry = null);

        /// <summary>
        /// 查询数据,如果不存在就添加
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">缓存的key</param>
        /// <param name="getData">提供数据的委托</param>
        /// <param name="expiry">缓存过期时间</param>
        /// <returns></returns>
        Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> getData, TimeSpan? expiry = null);

        /// <summary>
        /// 保存字符串
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <param name="expiry">过期时间</param>
        Task<bool> SetAsync(string key, string value, TimeSpan? expiry = null);

        /// <summary>
        /// 保存内容
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <param name="expiry">过期时间</param>
        Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiry = null);

        /// <summary>
        /// 移除某一个缓存值
        /// </summary>
        /// <param name="key">缓存Key</param>
        /// <returns></returns>
        Task<bool> RemoveAsync(string key);

        /// <summary>
        /// 批量删除缓存
        /// </summary>
        /// <returns></returns>
        Task<int> RemoveAsync(IEnumerable<string> keys);

        /// <summary>
        /// 根据前缀匹配符,批量删除Key
        /// * 表示可以匹配多个任意字符
        /// ? 表示可以匹配单个任意字符
        /// [] 表示可以匹配指定范围内的字符
        /// </summary>
        /// <param name="prefixMatchStr"></param>
        /// <returns></returns>
        Task<bool> RemoveMatchKeyAsync(string prefixMatchStr);

        /// <summary>
        /// 设置key过期时间
        /// </summary>
        /// <param name="key"></param>
        /// <param name="expire"></param>
        /// <returns></returns>
        Task<bool> ExpireAsync(string key, TimeSpan expire);

        /// <summary>
        /// 验证key是否存在
        /// </summary>
        /// <param name="key">缓存Key</param>
        /// <returns></returns>
        Task<bool> ExistAsync(string key);
    }
}
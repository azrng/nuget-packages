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
        /// <returns>命中的字符串；未命中返回 <c>null</c></returns>
        Task<string?> GetAsync(string key);

        /// <summary>
        /// 获取缓存,并序列化
        /// </summary>
        /// <param name="key">缓存Key</param>
        /// <returns>命中的反序列化结果；未命中返回 <c>null</c>（值类型为 <c>default</c>）</returns>
        Task<T?> GetAsync<T>(string key);

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

        /// <summary>
        /// 原子自增。key 不存在时从 0 开始累加；已有值不是整数时抛出异常。
        /// Redis 实现基于服务端 INCRBY，多实例分布式安全；内存实现基于进程内原子累加，仅单节点安全。
        /// 新建的计数器不设置过期时间，如需过期请配合 <see cref="ExpireAsync"/> 使用。
        /// 计数错误不可降级：本方法失败时始终抛出异常，不受 FailThrowException 配置影响。
        /// </summary>
        /// <param name="key">缓存Key</param>
        /// <param name="value">增量，默认为 1</param>
        /// <returns>自增后的最新值</returns>
        Task<long> IncrementAsync(string key, long value = 1);

        /// <summary>
        /// 原子自减，语义与 <see cref="IncrementAsync"/> 一致（等价于增量为负数的自增）。
        /// </summary>
        /// <param name="key">缓存Key</param>
        /// <param name="value">减量，默认为 1</param>
        /// <returns>自减后的最新值</returns>
        Task<long> DecrementAsync(string key, long value = 1);
    }
}
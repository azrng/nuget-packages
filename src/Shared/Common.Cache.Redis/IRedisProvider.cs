using System;
using System.Threading;
using System.Threading.Tasks;
using Azrng.Cache.Core;

namespace Common.Cache.Redis
{
    /// <summary>
    /// redis缓存提供者
    /// </summary>
    public interface IRedisProvider : ICacheProvider
    {
        #region 发布/订阅

        /// <summary>
        /// 发布消息到指定频道
        /// </summary>
        /// <param name="channel">频道名称</param>
        /// <param name="message">消息内容</param>
        /// <returns>接收到消息的订阅者数量</returns>
        Task<long> PublishAsync<T>(string channel, T message);

        /// <summary>
        /// 订阅频道，当有消息发布时触发回调
        /// </summary>
        /// <typeparam name="T">消息类型</typeparam>
        /// <param name="channel">频道名称</param>
        /// <param name="handler">消息处理回调</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>订阅任务，可用于取消订阅</returns>
        Task SubscribeAsync<T>(string channel, Action<T> handler, CancellationToken cancellationToken = default);

        /// <summary>
        /// 取消订阅频道
        /// </summary>
        /// <param name="channel">频道名称</param>
        /// <returns></returns>
        Task UnsubscribeAsync(string channel);

        /// <summary>
        /// 订阅匹配模式的频道
        /// </summary>
        /// <typeparam name="T">消息类型</typeparam>
        /// <param name="pattern">频道匹配模式，支持通配符：? 表示任意单个字符，* 表示任意多个字符</param>
        /// <param name="handler">消息处理回调，参数为频道名和消息内容</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns></returns>
        Task SubscribePatternAsync<T>(string pattern, Action<string, T> handler, CancellationToken cancellationToken = default);

        /// <summary>
        /// 取消模式订阅
        /// </summary>
        /// <param name="pattern">匹配模式</param>
        /// <returns></returns>
        Task UnsubscribePatternAsync(string pattern);

        #endregion
    }
}
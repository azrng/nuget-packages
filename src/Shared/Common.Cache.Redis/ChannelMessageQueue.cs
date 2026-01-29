using System.Threading;

namespace Common.Cache.Redis
{
    /// <summary>
    /// 频道消息队列，用于管理订阅的生命周期
    /// </summary>
    internal class ChannelMessageQueue
    {
        public string Channel { get; }

        public CancellationTokenSource CancellationTokenSource { get; }

        public ChannelMessageQueue(string channel, CancellationTokenSource cancellationTokenSource)
        {
            Channel = channel;
            CancellationTokenSource = cancellationTokenSource;
        }
    }
}
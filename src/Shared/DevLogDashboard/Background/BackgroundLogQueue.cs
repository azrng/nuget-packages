using Azrng.DevLogDashboard.Models;
using System.Threading.Channels;

namespace Azrng.DevLogDashboard.Background;

/// <summary>
/// 后台日志队列，用于异步处理日志写入
/// </summary>
public interface IBackgroundLogQueue
{
    /// <summary>
    /// 将日志条目加入队列
    /// </summary>
    ValueTask QueueLogEntryAsync(LogEntry entry);

    /// <summary>
    /// 尝试从队列中读取日志条目（带超时和取消支持）
    /// </summary>
    ValueTask<LogEntry?> DequeueAsync(CancellationToken cancellationToken, TimeSpan timeout);

    /// <summary>
    /// 批量从队列中读取日志条目（带取消支持）
    /// </summary>
    ValueTask<List<LogEntry>> DequeueBatchAsync(int maxCount, CancellationToken cancellationToken);

    /// <summary>
    /// 获取队列中待处理的日志数量
    /// </summary>
    int GetQueuedCount();
}

/// <summary>
/// 基于 System.Threading.Channels 的高性能后台日志队列实现
/// </summary>
public class BackgroundLogQueue : IBackgroundLogQueue
{
    private readonly Channel<LogEntry> _queue;

    public BackgroundLogQueue(int capacity = 10000)
    {
        // 创建有界队列，防止内存无限增长
        var options = new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait
        };
        _queue = Channel.CreateBounded<LogEntry>(options);
    }

    public ValueTask QueueLogEntryAsync(LogEntry entry)
    {
        if (entry == null)
        {
            return ValueTask.CompletedTask;
        }

        return _queue.Writer.WriteAsync(entry);
    }

    public async ValueTask<LogEntry?> DequeueAsync(CancellationToken cancellationToken, TimeSpan timeout)
    {
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(timeout);

            return await _queue.Reader.ReadAsync(cts.Token);
        }
        catch (OperationCanceledException)
        {
            return null;
        }
    }

    public async ValueTask<List<LogEntry>> DequeueBatchAsync(int maxCount, CancellationToken cancellationToken)
    {
        var batch = new List<LogEntry>(maxCount);

        try
        {
            // 尝试读取第一条（会等待）
            var first = await _queue.Reader.ReadAsync(cancellationToken);
            batch.Add(first);

            // 尝试读取更多（非等待模式）
            while (batch.Count < maxCount)
            {
                if (_queue.Reader.TryRead(out var entry))
                {
                    batch.Add(entry);
                }
                else
                {
                    break;
                }
            }
        }
        catch (OperationCanceledException)
        {
            // 取消操作，返回已读取的批次
        }

        return batch;
    }

    public int GetQueuedCount()
    {
        return _queue.Reader.Count;
    }
}

using Azrng.DevLogDashboard.Models;
using System.Threading.Channels;

namespace Azrng.DevLogDashboard.Background;

/// <summary>
/// 鍚庡彴鏃ュ織闃熷垪锛岀敤浜庡紓姝ュ鐞嗘棩蹇楀啓鍏?
/// </summary>
public interface IBackgroundLogQueue
{
    /// <summary>
    /// 灏嗘棩蹇楁潯鐩姞鍏ラ槦鍒?
    /// </summary>
    ValueTask QueueLogEntryAsync(LogEntry entry);

    /// <summary>
    /// 灏濊瘯浠庨槦鍒椾腑璇诲彇鏃ュ織鏉＄洰锛堝甫瓒呮椂鍜屽彇娑堟敮鎸侊級
    /// </summary>
    ValueTask<LogEntry?> DequeueAsync(CancellationToken cancellationToken, TimeSpan timeout);

    /// <summary>
    /// 鎵归噺浠庨槦鍒椾腑璇诲彇鏃ュ織鏉＄洰锛堝甫鍙栨秷鏀寔锛?
    /// </summary>
    ValueTask<List<LogEntry>> DequeueBatchAsync(int maxCount, CancellationToken cancellationToken);

    /// <summary>
    /// 鑾峰彇闃熷垪涓緟澶勭悊鐨勬棩蹇楁暟閲?
    /// </summary>
    int GetQueuedCount();
}

/// <summary>
/// 鍩轰簬 System.Threading.Channels 鐨勯珮鎬ц兘鍚庡彴鏃ュ織闃熷垪瀹炵幇
/// </summary>
public class BackgroundLogQueue : IBackgroundLogQueue
{
    private readonly Channel<LogEntry> _queue;
    private readonly int _capacity;
    private long _droppedCount;

    public BackgroundLogQueue(int capacity = 10000)
    {
        _capacity = capacity > 0 ? capacity : 10000;
        // 鍒涘缓鏈夌晫闃熷垪锛岄槻姝㈠唴瀛樻棤闄愬闀?
        var options = new BoundedChannelOptions(_capacity)
        {
            FullMode = BoundedChannelFullMode.DropOldest
        };
        _queue = Channel.CreateBounded<LogEntry>(options);
    }

    public ValueTask QueueLogEntryAsync(LogEntry entry)
    {
        if (entry == null)
        {
            return ValueTask.CompletedTask;
        }

        if (_queue.Reader.Count >= _capacity)
        {
            Interlocked.Increment(ref _droppedCount);
        }

        if (_queue.Writer.TryWrite(entry))
        {
            return ValueTask.CompletedTask;
        }

        Interlocked.Increment(ref _droppedCount);
        return ValueTask.CompletedTask;
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
            // 灏濊瘯璇诲彇绗竴鏉★紙浼氱瓑寰咃級
            var first = await _queue.Reader.ReadAsync(cancellationToken);
            batch.Add(first);

            // 灏濊瘯璇诲彇鏇村锛堥潪绛夊緟妯″紡锛?
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
            // 鍙栨秷鎿嶄綔锛岃繑鍥炲凡璇诲彇鐨勬壒娆?
        }

        return batch;
    }

    public int GetQueuedCount()
    {
        return _queue.Reader.Count;
    }

    /// <summary>
    /// 鑾峰彇琚涪寮冪殑鏃ュ織鏁伴噺锛堜粎鐢ㄤ簬璇婃柇锛?    /// </summary>
    public long GetDroppedCount()
    {
        return Interlocked.Read(ref _droppedCount);
    }
}



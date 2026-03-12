using Azrng.DevLogDashboard.Storage;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Azrng.DevLogDashboard.Background;

/// <summary>
/// 后台日志写入服务，负责从队列中批量读取日志并写入存储
/// </summary>
public class BackgroundLogWriter : BackgroundService
{
    private readonly IBackgroundLogQueue _queue;
    private readonly ILogStore _logStore;
    private readonly ILogger<BackgroundLogWriter> _logger;
    private readonly BackgroundLogWriterOptions _options;

    public BackgroundLogWriter(
        IBackgroundLogQueue queue,
        ILogStore logStore,
        ILogger<BackgroundLogWriter> logger,
        BackgroundLogWriterOptions options)
    {
        _queue = queue ?? throw new ArgumentNullException(nameof(queue));
        _logStore = logStore ?? throw new ArgumentNullException(nameof(logStore));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("后台日志写入服务已启动");

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // 批量从队列中读取日志
                    var batch = await _queue.DequeueBatchAsync(_options.BatchSize, stoppingToken);

                    if (batch.Count > 0)
                    {
                        // 批量写入存储
                        await _logStore.AddBatchAsync(batch, stoppingToken);

                        _logger.LogDebug("已写入 {Count} 条日志", batch.Count);

                        // 如果队列中还有日志，立即处理下一批
                        if (_queue.GetQueuedCount() >= _options.BatchSize)
                        {
                            continue;
                        }
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    // 正常关闭
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "后台日志写入失败");
                }

                // 等待一段时间或直到有新日志
                await Task.Delay(_options.PollInterval, stoppingToken);
            }
        }
        finally
        {
            // 确保在关闭前处理剩余的日志
            await FlushRemainingLogsAsync(stoppingToken);
            _logger.LogInformation("后台日志写入服务已停止");
        }
    }

    private async Task FlushRemainingLogsAsync(CancellationToken cancellationToken)
    {
        try
        {
            var remainingCount = _queue.GetQueuedCount();
            if (remainingCount == 0)
            {
                return;
            }

            _logger.LogInformation("正在处理剩余的 {Count} 条日志...", remainingCount);

            while (_queue.GetQueuedCount() > 0 && !cancellationToken.IsCancellationRequested)
            {
                var batch = await _queue.DequeueBatchAsync(_options.BatchSize, cancellationToken);
                if (batch.Count == 0)
                {
                    break;
                }

                await _logStore.AddBatchAsync(batch, cancellationToken);
                _logger.LogInformation("已处理 {Count} 条日志，剩余 {Remaining} 条",
                    batch.Count, _queue.GetQueuedCount());
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "处理剩余日志时发生错误");
        }
    }
}

/// <summary>
/// 后台日志写入器配置选项
/// </summary>
public class BackgroundLogWriterOptions
{
    /// <summary>
    /// 批量写入大小，默认为 100
    /// </summary>
    public int BatchSize { get; set; } = 100;

    /// <summary>
    /// 轮询间隔，默认为 1 秒
    /// </summary>
    public TimeSpan PollInterval { get; set; } = TimeSpan.FromSeconds(1);
}

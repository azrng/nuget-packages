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
    private readonly SemaphoreSlim _initializeLock = new(1, 1);

    private volatile bool _initialized;

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
        _logger.LogInformation("Background log writer started.");

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (!await EnsureInitializedAsync(stoppingToken))
                    {
                        await Task.Delay(_options.PollInterval, stoppingToken);
                        continue;
                    }

                    var batch = await _queue.DequeueBatchAsync(_options.BatchSize, stoppingToken);

                    if (batch.Count > 0)
                    {
                        await _logStore.AddBatchAsync(batch, stoppingToken);
                        _logger.LogDebug("Persisted {Count} log entries.", batch.Count);

                        if (_queue.GetQueuedCount() >= _options.BatchSize)
                        {
                            continue;
                        }
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to persist queued log entries.");
                }

                await Task.Delay(_options.PollInterval, stoppingToken);
            }
        }
        finally
        {
            using var shutdownCts = new CancellationTokenSource(_options.ShutdownFlushTimeout);
            await FlushRemainingLogsAsync(shutdownCts.Token);
            _logger.LogInformation("Background log writer stopped.");
        }
    }

    private async Task FlushRemainingLogsAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (!await EnsureInitializedAsync(cancellationToken))
            {
                return;
            }

            var remainingCount = _queue.GetQueuedCount();
            if (remainingCount == 0)
            {
                return;
            }

            _logger.LogInformation("Flushing {Count} remaining log entries before shutdown.", remainingCount);

            while (_queue.GetQueuedCount() > 0 && !cancellationToken.IsCancellationRequested)
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(TimeSpan.FromMilliseconds(250));

                var batch = await _queue.DequeueBatchAsync(_options.BatchSize, cts.Token);
                if (batch.Count == 0)
                {
                    break;
                }

                await _logStore.AddBatchAsync(batch, cancellationToken);
                _logger.LogInformation(
                    "Flushed {Count} log entries, {Remaining} remaining.",
                    batch.Count,
                    _queue.GetQueuedCount());
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed while flushing remaining log entries.");
        }
    }

    private async Task<bool> EnsureInitializedAsync(CancellationToken cancellationToken)
    {
        if (_initialized)
        {
            return true;
        }

        try
        {
            await _initializeLock.WaitAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return false;
        }

        try
        {
            if (_initialized)
            {
                return true;
            }

            await _logStore.InitializeAsync(cancellationToken);
            _initialized = true;
            return true;
        }
        catch (OperationCanceledException)
        {
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize the log store.");
            return false;
        }
        finally
        {
            _initializeLock.Release();
        }
    }
}

/// <summary>
/// Options for background batch persistence.
/// </summary>
public class BackgroundLogWriterOptions
{
    /// <summary>
    /// Maximum number of entries written per batch.
    /// </summary>
    public int BatchSize { get; set; } = 100;

    /// <summary>
    /// Delay between polling attempts when the queue is idle.
    /// </summary>
    public TimeSpan PollInterval { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Maximum time allowed for the shutdown flush before giving up.
    /// </summary>
    public TimeSpan ShutdownFlushTimeout { get; set; } = TimeSpan.FromSeconds(5);
}

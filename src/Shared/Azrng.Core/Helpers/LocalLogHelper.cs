using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Azrng.Core.Enums;

namespace Azrng.Core.Helpers
{
    /// <summary>
    /// 记录本地日志
    /// </summary>
    public static class LocalLogHelper
    {
        /// <summary>
        /// 队列
        /// </summary>
        private static readonly ConcurrentQueue<LogEntry> _logQueue = new ConcurrentQueue<LogEntry>();

        /// <summary>
        /// 最大队列大小
        /// </summary>
        private static readonly int _maxQueueSize = 1000;

        /// <summary>
        /// 刷新间隔
        /// </summary>
        private static readonly TimeSpan _flushInterval = TimeSpan.FromSeconds(5);

        /// <summary>
        /// 是否正在处理日志
        /// </summary>
        private static volatile bool _isProcessing;

        /// <summary>
        /// 日志保留天数
        /// </summary>
        private static readonly int _logRetentionDays = 7;

        /// <summary>
        /// 清理日志的时间间隔（默认7天）
        /// </summary>
        private static readonly TimeSpan _cleanupInterval =
            TimeSpan.FromDays(CoreGlobalConfig.CleanupInterval <= 0 ? 7 : CoreGlobalConfig.CleanupInterval);

        // 使用静态构造函数启动后台处理任务
        static LocalLogHelper()
        {
            StartBackgroundProcessor();

            if (CoreGlobalConfig.IsClearLocalLog)
                StartCleanupProcessor();
        }

        /// <summary>
        /// 启动后台处理任务
        /// </summary>
        private static void StartBackgroundProcessor()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    await ProcessLogQueueAsync();
                    await Task.Delay(_flushInterval);
                }
            });
        }

        /// <summary>
        /// 启动日志清理任务
        /// </summary>
        private static void StartCleanupProcessor()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        await CleanupLogsAsync();

                        // 等待到下一天的凌晨2点执行清理
                        var now = DateTime.Now;
                        var nextRun = now.Date.AddDays(1).AddHours(2);
                        var delay = nextRun - now;
                        await Task.Delay(delay);
                    }
                    catch (Exception ex)
                    {
                        await LogErrorAsync(ex);
                        await Task.Delay(_cleanupInterval);
                    }
                }
            });
        }

        /// <summary>
        /// 记录Trace日志
        /// </summary>
        /// <param name="logInfo"></param>
        public static void LogTrace(string logInfo)
        {
            if (CoreGlobalConfig.MinimumLevel <= LogLevel.Trace)
                WriteMyLogs(nameof(LogLevel.Trace), logInfo);
        }

        /// <summary>
        /// 记录Debug日志
        /// </summary>
        /// <param name="logInfo"></param>
        public static void LogDebug(string logInfo)
        {
            if (CoreGlobalConfig.MinimumLevel <= LogLevel.Debug)
                WriteMyLogs(nameof(LogLevel.Debug), logInfo);
        }

        /// <summary>
        /// 记录Info日志
        /// </summary>
        /// <param name="logInfo"></param>
        public static void LogInformation(string logInfo)
        {
            if (CoreGlobalConfig.MinimumLevel <= LogLevel.Information)
                WriteMyLogs(nameof(LogLevel.Information), logInfo);
        }

        /// <summary>
        /// 记录Warning日志
        /// </summary>
        /// <param name="logInfo"></param>
        public static void LogWarning(string logInfo)
        {
            if (CoreGlobalConfig.MinimumLevel <= LogLevel.Warning)
                WriteMyLogs(nameof(LogLevel.Warning), logInfo);
        }

        /// <summary>
        /// 记录Error日志
        /// </summary>
        /// <param name="logInfo"></param>
        public static void LogError(string logInfo)
        {
            if (CoreGlobalConfig.MinimumLevel <= LogLevel.Error)
                WriteMyLogs(nameof(LogLevel.Error), logInfo);
        }

        /// <summary>
        /// 记录Critical日志
        /// </summary>
        /// <param name="logInfo"></param>
        public static void LogCritical(string logInfo)
        {
            if (CoreGlobalConfig.MinimumLevel <= LogLevel.Critical)
                WriteMyLogs(nameof(LogLevel.Critical), logInfo);
        }

        /// <summary>
        /// 记录Trace日志
        /// </summary>
        /// <param name="logInfo"></param>
        public static async Task LogTraceAsync(string logInfo)
        {
            if (CoreGlobalConfig.MinimumLevel <= LogLevel.Trace)
                await WriteMyLogsAsync(nameof(LogLevel.Trace), logInfo);
        }

        /// <summary>
        /// 记录Debug日志
        /// </summary>
        /// <param name="logInfo"></param>
        public static async Task LogDebugAsync(string logInfo)
        {
            if (CoreGlobalConfig.MinimumLevel <= LogLevel.Debug)
                await WriteMyLogsAsync(nameof(LogLevel.Debug), logInfo);
        }

        /// <summary>
        /// 记录Info日志
        /// </summary>
        /// <param name="logInfo"></param>
        public static async Task LogInformationAsync(string logInfo)
        {
            if (CoreGlobalConfig.MinimumLevel <= LogLevel.Information)
                await WriteMyLogsAsync(nameof(LogLevel.Information), logInfo);
        }

        /// <summary>
        /// 记录Warning日志
        /// </summary>
        /// <param name="logInfo"></param>
        public static async Task LogWarningAsync(string logInfo)
        {
            if (CoreGlobalConfig.MinimumLevel <= LogLevel.Warning)
                await WriteMyLogsAsync(nameof(LogLevel.Warning), logInfo);
        }

        /// <summary>
        /// 记录Error日志
        /// </summary>
        /// <param name="logInfo"></param>
        public static async Task LogErrorAsync(string logInfo)
        {
            if (CoreGlobalConfig.MinimumLevel <= LogLevel.Error)
                await WriteMyLogsAsync(nameof(LogLevel.Error), logInfo);
        }

        /// <summary>
        /// 记录Critical日志
        /// </summary>
        /// <param name="logInfo"></param>
        public static async Task LogCriticalAsync(string logInfo)
        {
            if (CoreGlobalConfig.MinimumLevel <= LogLevel.Critical)
                await WriteMyLogsAsync(nameof(LogLevel.Critical), logInfo);
        }

        /// <summary>
        /// 记录日志
        /// </summary>
        /// <param name="type">日志归类</param>
        /// <param name="logInfo">日志内容</param>
        public static void WriteMyLogs(string type, string logInfo)
        {
            WriteMyLogsAsync(type, logInfo).GetAwaiter().GetResult();
        }

        /// <summary>
        /// 记录日志
        /// </summary>
        public static async Task WriteMyLogsAsync(string type, string logInfo)
        {
            var logEntry = new LogEntry { Timestamp = DateTime.Now, Type = type, Message = logInfo };

            _logQueue.Enqueue(logEntry);

            // 如果队列超过最大大小，触发立即处理
            if (_logQueue.Count >= _maxQueueSize)
            {
                await ProcessLogQueueAsync();
            }
        }

        /// <summary>
        /// 获取日志文件地址
        /// </summary>
        /// <param name="logPath"></param>
        /// <param name="dateNow"></param>
        /// <returns></returns>
        private static string GetLogFilePath(string logPath, DateTime dateNow)
        {
            return Path.Combine(logPath, dateNow.ToString("yyyyMMdd") + ".log");
        }

        /// <summary>
        /// 处理日志队列
        /// </summary>
        private static async Task ProcessLogQueueAsync()
        {
            if (_isProcessing) return;

            try
            {
                _isProcessing = true;
                var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
                Directory.CreateDirectory(logPath);

                var currentDate = DateTime.Now;
                var logFilePath = GetLogFilePath(logPath, currentDate);

                await using var sw = new StreamWriter(logFilePath, true, Encoding.UTF8, 8192);
                while (_logQueue.TryDequeue(out var logEntry))
                {
                    var logMessage = FormatLogMessage(logEntry);
                    await sw.WriteLineAsync(logMessage);
                }

                await sw.FlushAsync();
            }
            catch (Exception ex)
            {
                await LogErrorAsync(ex);
            }
            finally
            {
                _isProcessing = false;
            }
        }

        private static string FormatLogMessage(LogEntry entry)
        {
            return $"{entry.Timestamp:yyyy-MM-dd HH:mm:ss} ==> {entry.Type}\t{entry.Message}";
        }

        /// <summary>
        /// 异步记录错误日志
        /// </summary>
        private static async Task LogErrorAsync(Exception ex)
        {
            try
            {
                var errorLogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "error.log");
                await using var sw = new StreamWriter(errorLogPath, true, Encoding.UTF8);
                await sw.WriteLineAsync($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} ==> Error: {ex.Message}");
                await sw.WriteLineAsync($"StackTrace: {ex.StackTrace}");
            }
            catch
            {
                // 最后的防线，如果连错误日志都无法写入，则忽略
            }
        }

        /// <summary>
        /// 清理过期日志
        /// </summary>
        private static async Task CleanupLogsAsync()
        {
            try
            {
                var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
                if (!Directory.Exists(logPath))
                {
                    return;
                }

                var cutoffDate = DateTime.Now.AddDays(-_logRetentionDays);
                var directory = new DirectoryInfo(logPath);
                var logFiles = directory.GetFiles("*.log");

                foreach (var file in logFiles)
                {
                    try
                    {
                        if (file.LastWriteTime < cutoffDate)
                        {
                            // 如果文件正在被使用，等待文件释放
                            if (IsFileInUse(file.FullName))
                            {
                                continue;
                            }

                            file.Delete();
                            await LogInformationAsync($"已删除过期日志文件: {file.Name}");
                        }
                    }
                    catch (Exception ex)
                    {
                        await LogErrorAsync(new Exception($"清理日志文件 {file.Name} 失败", ex));
                    }
                }

                // 压缩旧日志文件
                //await CompressOldLogsAsync(logPath, cutoffDate);
            }
            catch (Exception ex)
            {
                await LogErrorAsync(new Exception("执行日志清理任务时发生错误", ex));
            }
        }

        /// <summary>
        /// 检查文件是否被占用
        /// </summary>
        private static bool IsFileInUse(string filePath)
        {
            try
            {
                using var stream = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                return false;
            }
            catch (IOException)
            {
                return true;
            }
        }

        /// <summary>
        /// 记录日志实体类
        /// </summary>
        private class LogEntry
        {
            /// <summary>
            /// 时间戳
            /// </summary>
            public DateTime Timestamp { get; set; }

            /// <summary>
            /// 类型
            /// </summary>
            public string Type { get; set; } = string.Empty;

            /// <summary>
            /// 消息
            /// </summary>
            public string Message { get; set; } = string.Empty;
        }
    }
}
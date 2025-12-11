using System.Collections.Concurrent;
using System.Text;

namespace CommonCollect
{
    /// <summary>
    /// 记录本地日志 多线程下入方法：https://mp.weixin.qq.com/s/XLcIwtfosyC0cYpFWlXsOg
    /// </summary>
    public class LocalThreadLogHelper : IDisposable
    {
        private readonly BlockingCollection<(string log, DateTimeOffset timestamp)> _messageQueue =
            new BlockingCollection<(string log, DateTimeOffset timestamp)>();

        private FileStream _fileStream;
        private string _logFileName;

        public LocalThreadLogHelper()
        {
            var logPath = AppDomain.CurrentDomain.BaseDirectory + "logs\\";
            if (!Directory.Exists(logPath))
            {
                try
                {
                    Directory.CreateDirectory(logPath);
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException("Failed to create log directory", e);
                }
            }

            var outputThread = new Thread(ProcessLogLogQueue)
                               {
                                   IsBackground = true, Priority = ThreadPriority.BelowNormal, Name = "FileLoggingProcesser"
                               };
            outputThread.Start();
        }

        /// <summary>
        /// 记录日志
        /// </summary>
        /// <param name="logInfo">日志内容</param>
        public void WriteMyLogs(string logInfo)
        {
            if (_messageQueue.IsAddingCompleted)
                return;
            try
            {
                var logPath = AppDomain.CurrentDomain.BaseDirectory + "logs\\";
                var dateNow = DateTime.Now;

                // 清理7天前的日志文件
                CleanOldLogs(logPath, 7);

                _messageQueue.Add((logInfo, dateNow));
            }
            catch (Exception ex)
            {
                // ignored
                LogError(ex);
            }
        }

        private void ProcessLogLogQueue()
        {
            try
            {
                foreach (var message in _messageQueue.GetConsumingEnumerable())
                {
                    WriteLoggerEvent(message.log, message.timestamp);
                }
            }
            catch (Exception e)
            {
                try
                {
                    _messageQueue.CompleteAdding();
                }
                catch (Exception ex)
                {
                    // ignored
                    // ignored
                    LogError(ex);
                }
            }
        }

        private void WriteLoggerEvent(string logInfo, DateTimeOffset timestamp)
        {
            var logPath = AppDomain.CurrentDomain.BaseDirectory + "logs\\";
            var dateNow = DateTime.Now;
            var filePath = GetLogFilePath(logPath, dateNow);
            var fullName = timestamp.ToString("yyyyMMdd") + ".log";
            try
            {
                //var previousFileName = Interlocked.CompareExchange(ref _logFileName, fullName, logPath);
                //if (_logFileName != previousFileName)
                //{
                // 文件名字被修改
                using var sw = File.Exists(filePath) ? File.AppendText(filePath) : File.CreateText(filePath);
                var originalWriter = Interlocked.Exchange(ref _fileStream, _fileStream);
                if (originalWriter is not null)
                {
                    originalWriter.Flush();
                    originalWriter.Dispose();
                }

                //}

                var strLog = new StringBuilder();
                strLog.Append(dateNow.ToString("yyyy-MM-dd HH:mm:ss").Trim())
                      .Append("==>")
                      .Append("\t\t")
                      .Append(logInfo)
                      .Append("\t\t")
                      .Append("\r\n");

                // _fileStream.Write(bytes, 0, bytes.Length);
                // _fileStream.Flush();
                sw.Write(strLog);
            }
            catch (Exception e)
            {
                Console.WriteLine($@"Error when trying to log to file({fullName}) \n" + logInfo + Environment.NewLine + e);
            }
        }

        /// <summary>
        /// 获取日志文件地址
        /// </summary>
        /// <param name="logPath"></param>
        /// <param name="dateNow"></param>
        /// <returns></returns>
        private string GetLogFilePath(string logPath, DateTime dateNow)
        {
            return Path.Combine(logPath, dateNow.ToString("yyyyMMdd") + ".log");
        }

        /// <summary>
        /// 清理历史日志文件
        /// </summary>
        /// <param name="logDirectory"></param>
        /// <param name="days"></param>
        private void CleanOldLogs(string logDirectory, int days)
        {
            try
            {
                var directoryInfo = new DirectoryInfo(logDirectory);
                foreach (var file in directoryInfo.GetFiles("*.log"))
                {
                    if (file.CreationTime < DateTime.Now.AddDays(-days))
                    {
                        file.Delete();
                    }
                }
            }
            catch (Exception ex)
            {
                // ignored
                Console.WriteLine("Error cleaning old logs: " + ex.Message);
            }
        }

        private void LogError(Exception ex)
        {
            try
            {
                var errorLogPath = AppDomain.CurrentDomain.BaseDirectory + "logs\\error.log";
                using var sw = File.AppendText(errorLogPath);
                sw.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} ==> Error: {ex.Message}");
            }
            catch
            {
                // ignored
            }
        }

        private bool _disposing;

        public void Dispose()
        {
            if (!_disposing)
                return;
            _messageQueue.CompleteAdding();
            _fileStream?.Flush();
            _fileStream?.Dispose();
            _messageQueue?.Dispose();
        }
    }
}
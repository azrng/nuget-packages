using Azrng.Core;
using Azrng.Core.Extension;
using Azrng.Core.Helpers;
using Microsoft.Extensions.Logging;

namespace Azrng.ConsoleApp.DependencyInjection.Logger
{
    public class ExtensionsLogger : ILogger
    {
        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();

            public void Dispose()
            {
            }
        }

        /// <summary>
        /// LogLevel 到 LocalLogHelper 日志类型字符串的映射，避免每条日志都走 switch 分支
        /// </summary>
        private static readonly Dictionary<LogLevel, string> _levelNames = new()
        {
            [LogLevel.Trace] = nameof(LogLevel.Trace),
            [LogLevel.Debug] = nameof(LogLevel.Debug),
            [LogLevel.Information] = nameof(LogLevel.Information),
            [LogLevel.Warning] = nameof(LogLevel.Warning),
            [LogLevel.Error] = nameof(LogLevel.Error),
            [LogLevel.Critical] = nameof(LogLevel.Critical),
        };

        private readonly string _categoryName;

        public ExtensionsLogger(string categoryName)
        {
            _categoryName = categoryName;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return NullScope.Instance;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel != LogLevel.None && (int)logLevel >= (int)CoreGlobalConfig.MinimumLevel;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            var formattedMessage = formatter(state, exception);
            var logMessage = _categoryName.IsNotNullOrEmpty()
                ? $"{_categoryName} {formattedMessage}"
                : formattedMessage;

            if (exception is not null)
            {
                logMessage = $"{logMessage}{Environment.NewLine}{exception}";
            }

            // IsEnabled 已完成级别过滤，这里统一走 WriteMyLogs 入口，避免 switch 分支
            if (!_levelNames.TryGetValue(logLevel, out var typeName))
            {
                // LogLevel.None 或未知级别直接忽略
                return;
            }

            LocalLogHelper.WriteMyLogs(typeName, logMessage);
        }
    }
}

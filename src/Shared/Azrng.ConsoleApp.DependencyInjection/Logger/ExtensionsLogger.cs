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

            switch (logLevel)
            {
                case LogLevel.Trace:
                    LocalLogHelper.LogTrace(logMessage);
                    break;
                case LogLevel.Debug:
                    LocalLogHelper.LogDebug(logMessage);
                    break;
                case LogLevel.Information:
                    LocalLogHelper.LogInformation(logMessage);
                    break;
                case LogLevel.Warning:
                    LocalLogHelper.LogWarning(logMessage);
                    break;
                case LogLevel.Error:
                    LocalLogHelper.LogError(logMessage);
                    break;
                case LogLevel.Critical:
                    LocalLogHelper.LogCritical(logMessage);
                    break;
                case LogLevel.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null);
            }
        }
    }
}

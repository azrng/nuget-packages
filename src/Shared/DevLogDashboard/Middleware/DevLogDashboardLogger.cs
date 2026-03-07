using Azrng.DevLogDashboard.Models;
using Azrng.DevLogDashboard.Options;
using Azrng.DevLogDashboard.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Azrng.DevLogDashboard.Middleware;

/// <summary>
/// DevLogDashboard 日志记录器
/// </summary>
public class DevLogDashboardLogger : ILogger
{
    private readonly string _category;
    private readonly ILogStore _logStore;
    private readonly DevLogDashboardOptions _options;
    private readonly IHttpContextAccessor? _httpContextAccessor;

    public DevLogDashboardLogger(
        string category,
        ILogStore logStore,
        DevLogDashboardOptions options,
        IHttpContextAccessor? httpContextAccessor = null)
    {
        _category = category;
        _logStore = logStore;
        _options = options;
        _httpContextAccessor = httpContextAccessor;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        if (_options.OnlyLogErrors && logLevel < LogLevel.Warning)
        {
            return false;
        }

        return logLevel >= ConvertLogLevel(_options.MinLogLevel);
    }

    IDisposable ILogger.BeginScope<TState>(TState state)
    {
        return NullScope.Instance;
    }

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        try
        {
            var message = formatter(state, exception);
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            var httpContext = _httpContextAccessor?.HttpContext;
            var requestId = httpContext?.TraceIdentifier;
            var connectionId = httpContext?.Connection.Id;
            var requestPath = httpContext?.Request.Path;
            var requestMethod = httpContext?.Request.Method;
            var responseStatusCode = httpContext?.Response.StatusCode;

            var logEntry = new LogEntry
            {
                Id = Guid.NewGuid().ToString("N"),
                RequestId = requestId,
                ConnectionId = connectionId,
                Level = ConvertLogLevel(logLevel),
                Message = message,
                Timestamp = DateTime.Now,
                Source = _category,
                EventId = eventId.Id,
                RequestPath = requestPath?.ToString(),
                RequestMethod = requestMethod,
                ResponseStatusCode = responseStatusCode,
                Exception = exception?.ToString(),
                StackTrace = exception?.StackTrace,
                MachineName = Environment.MachineName,
                Application = _options.ApplicationName,
                AppVersion = _options.ApplicationVersion,
                SdkVersion = typeof(DevLogDashboardLogger).Assembly.GetName().Version?.ToString(),
                Environment = System.Diagnostics.Debugger.IsAttached ? "Development" : "Production",
                ProcessId = System.Diagnostics.Process.GetCurrentProcess().Id,
                ThreadId = Environment.CurrentManagedThreadId,
                ThreadName = Thread.CurrentThread.Name,
                Logger = _category
            };

            if (state is IEnumerable<KeyValuePair<string, object?>> structuredData)
            {
                foreach (var kvp in structuredData)
                {
                    if (kvp.Key != "{OriginalFormat}" && !logEntry.Properties.ContainsKey(kvp.Key))
                    {
                        logEntry.Properties[kvp.Key] = kvp.Value;
                    }
                }
            }

            _logStore.Add(logEntry);
        }
        catch
        {
            // Provider logging failure must never impact application flow.
        }
    }

    private static LogLevel ConvertLogLevel(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Trace => LogLevel.Trace,
            LogLevel.Debug => LogLevel.Debug,
            LogLevel.Information => LogLevel.Information,
            LogLevel.Warning => LogLevel.Warning,
            LogLevel.Error => LogLevel.Error,
            LogLevel.Critical => LogLevel.Critical,
            _ => LogLevel.Information
        };
    }

    private sealed class NullScope : IDisposable
    {
        public static readonly NullScope Instance = new();

        public void Dispose()
        {
        }
    }
}

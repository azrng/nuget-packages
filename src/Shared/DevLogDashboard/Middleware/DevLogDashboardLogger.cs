using Microsoft.Extensions.Logging;
using DevLogDashboard.Storage;
using DevLogDashboard.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.AspNetCore.Http;

namespace DevLogDashboard.Middleware;

/// <summary>
/// DevLogDashboard 日志记录器
/// </summary>
public class DevLogDashboardLogger : ILogger, IDisposable
{
    private readonly string _category;
    private readonly ILogStore _logStore;
    private readonly DevLogDashboardOptions _options;
    private readonly IHttpContextAccessor? _httpContextAccessor;
    private readonly IDisposable? _scope;

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

    public IDisposable? BeginScope<TState>(TState state)
    {
        return _scope;
    }

    public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel)
    {
        if (_options.OnlyLogErrors && logLevel < Microsoft.Extensions.Logging.LogLevel.Warning)
        {
            return false;
        }

        return logLevel >= ConvertLogLevel(_options.MinLogLevel);
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

        var message = formatter(state, exception);
        if (string.IsNullOrEmpty(message))
        {
            return;
        }

        // 获取当前 HTTP 上下文
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

        // 提取结构化数据
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

    public void Dispose()
    {
        _scope?.Dispose();
    }

    private static LogLevel ConvertLogLevel(Microsoft.Extensions.Logging.LogLevel logLevel)
    {
        return logLevel switch
        {
            Microsoft.Extensions.Logging.LogLevel.Trace => LogLevel.Trace,
            Microsoft.Extensions.Logging.LogLevel.Debug => LogLevel.Debug,
            Microsoft.Extensions.Logging.LogLevel.Information => LogLevel.Information,
            Microsoft.Extensions.Logging.LogLevel.Warning => LogLevel.Warning,
            Microsoft.Extensions.Logging.LogLevel.Error => LogLevel.Error,
            Microsoft.Extensions.Logging.LogLevel.Critical => LogLevel.Critical,
            _ => LogLevel.Information
        };
    }
}

using Azrng.DevLogDashboard.Models;
using Azrng.DevLogDashboard.Options;
using Azrng.DevLogDashboard.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Azrng.DevLogDashboard.Middleware;

/// <summary>
/// DevLogDashboard 日志记录器
/// </summary>
public class DevLogDashboardLogger : ILogger
{
    private static readonly string RuntimeMachineName = Environment.MachineName;
    private static readonly string RuntimeEnvironment = System.Diagnostics.Debugger.IsAttached ? "Development" : "Production";
    private static readonly int RuntimeProcessId = Environment.ProcessId;
    private static readonly string? RuntimeSdkVersion = typeof(DevLogDashboardLogger).Assembly.GetName().Version?.ToString();

    private static readonly JsonSerializerOptions PropertyValueSerializerOptions = new()
                                                                                   {
                                                                                       ReferenceHandler = ReferenceHandler.IgnoreCycles
                                                                                   };

    private readonly string _category;
    private readonly Func<ILogStore> _logStoreFactory;
    private readonly DevLogDashboardOptions _options;
    private readonly IHttpContextAccessor? _httpContextAccessor;

    public DevLogDashboardLogger(
        string category,
        Func<ILogStore> logStoreFactory,
        DevLogDashboardOptions options,
        IHttpContextAccessor? httpContextAccessor = null)
    {
        _category = category;
        _logStoreFactory = logStoreFactory ?? throw new ArgumentNullException(nameof(logStoreFactory));
        _options = options;
        _httpContextAccessor = httpContextAccessor;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        if (_options.OnlyLogErrors && logLevel < LogLevel.Warning)
        {
            return false;
        }

        return logLevel >= _options.MinLogLevel;
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

        if (ShouldSkipCurrentRequest())
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
                               RequestId = requestId,
                               ConnectionId = connectionId,
                               Level = logLevel,
                               Message = message,
                               Timestamp = DateTime.Now,
                               Source = _category,
                               EventId = eventId.Id,
                               RequestPath = requestPath?.ToString(),
                               RequestMethod = requestMethod,
                               ResponseStatusCode = responseStatusCode,
                               Exception = exception?.ToString(),
                               StackTrace = exception?.StackTrace,
                               MachineName = RuntimeMachineName,
                               Application = _options.ApplicationName,
                               AppVersion = _options.ApplicationVersion,
                               SdkVersion = RuntimeSdkVersion,
                               Environment = RuntimeEnvironment,
                               ProcessId = RuntimeProcessId,
                               ThreadId = Environment.CurrentManagedThreadId,
                               ThreadName = Thread.CurrentThread.Name,
                               Logger = _category
                           };

            if (state is IEnumerable<KeyValuePair<string, object?>> structuredData)
            {
                if (_options.SkipStructuredProperties)
                {
                    // 跳过结构化属性，只保存简单的键值对计数
                    logEntry.Properties["_skippedProps"] = structuredData.Count().ToString();
                }
                else
                {
                    foreach (var kvp in structuredData)
                    {
                        if (kvp.Key != "{OriginalFormat}" && !logEntry.Properties.ContainsKey(kvp.Key))
                        {
                            logEntry.Properties[kvp.Key] = NormalizePropertyValue(kvp.Value, _options.MaxPropertySerializationLength);
                        }
                    }
                }
            }

            // Resolve store lazily to avoid circular dependencies during host startup.
            var logStore = _logStoreFactory();
            if (logStore == null)
            {
                return;
            }

            // Fire-and-forget to avoid blocking the request/host startup on storage latency.
            _ = logStore.AddAsync(logEntry);
        }
        catch
        {
            // Provider logging failure must never impact application flow.
        }
    }

    private static object? NormalizePropertyValue(object? value, int maxLength)
    {
        if (value is null)
        {
            return null;
        }

        // 快速路径：简单类型直接返回
        if (value is string strValue)
        {
            return strValue.Length > maxLength ? strValue.Substring(0, maxLength) + "..." : strValue;
        }

        if (value is bool or byte or sbyte or short or ushort or int or uint or long or ulong or float or double
            or decimal or DateTime or DateTimeOffset or Guid)
        {
            return value;
        }

        if (value is Enum enumValue)
        {
            return enumValue.ToString();
        }

        if (value is JsonElement jsonElement)
        {
            return jsonElement;
        }

        // 尝试序列化复杂对象
        try
        {
            var serialized = JsonSerializer.SerializeToElement(value, PropertyValueSerializerOptions);
            var jsonStr = serialized.ToString();

            // 如果序列化结果超过最大长度，截断
            if (jsonStr.Length > maxLength)
            {
                return jsonStr.Substring(0, maxLength) + "...";
            }

            return serialized;
        }
        catch
        {
            // 序列化失败，返回 ToString() 并限制长度
            var toStringValue = value.ToString();
            if (string.IsNullOrEmpty(toStringValue))
            {
                return value.GetType().Name;
            }

            return toStringValue.Length > maxLength ? toStringValue.Substring(0, maxLength) + "..." : toStringValue;
        }
    }

    private bool ShouldSkipCurrentRequest()
    {
        var context = _httpContextAccessor?.HttpContext;
        if (context == null)
        {
            return false;
        }

        var ignoredPaths = _options.IgnoredPaths ?? Array.Empty<string>();
        if (ignoredPaths.Count == 0)
        {
            return false;
        }

        var path = context.Request.Path;
        var fullPath = context.Request.PathBase.Add(path);

        foreach (var rawPath in ignoredPaths)
        {
            if (string.IsNullOrWhiteSpace(rawPath))
            {
                continue;
            }

            var ignoredPath = new PathString(rawPath);
            if (path.StartsWithSegments(ignoredPath, StringComparison.OrdinalIgnoreCase) ||
                fullPath.StartsWithSegments(ignoredPath, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private sealed class NullScope : IDisposable
    {
        public static readonly NullScope Instance = new();

        public void Dispose() { }
    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using DevLogDashboard.Storage;
using DevLogDashboard.Options;

namespace DevLogDashboard.Middleware;

/// <summary>
/// 日志采集中间件
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;
    private readonly ILogStore _logStore;
    private readonly DevLogDashboardOptions _options;
    private readonly string _machineName;
    private readonly string? _applicationName;
    private readonly string? _applicationVersion;
    private readonly string? _sdkVersion;

    public RequestLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestLoggingMiddleware> logger,
        ILogStore logStore,
        DevLogDashboardOptions options,
        IHostEnvironment? environment = null)
    {
        _next = next;
        _logger = logger;
        _logStore = logStore;
        _options = options;
        _machineName = Environment.MachineName;
        _applicationName = options.ApplicationName ?? environment?.ApplicationName;
        _applicationVersion = options.ApplicationVersion;
        _sdkVersion = typeof(RequestLoggingMiddleware).Assembly.GetName().Version?.ToString();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // 检查是否应该跳过
        if (ShouldSkipRequest(context))
        {
            await _next(context);
            return;
        }

        var requestId = context.TraceIdentifier;
        var startTime = DateTime.Now;
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // 确保 TraceIdentifier 被设置
        if (string.IsNullOrEmpty(context.TraceIdentifier))
        {
            context.TraceIdentifier = Guid.NewGuid().ToString("N");
        }

        try
        {
            // 记录请求开始
            LogRequestStart(context, requestId);

            await _next(context);

            // 记录请求结束
            stopwatch.Stop();
            LogRequestEnd(context, requestId, startTime, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            // 记录异常
            stopwatch.Stop();
            LogException(context, requestId, ex, startTime, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    private bool ShouldSkipRequest(HttpContext context)
    {
        // 检查路径
        if (_options.IgnoredPaths.Any(p => context.Request.Path.StartsWithSegments(p)))
        {
            return true;
        }

        // 检查方法
        if (_options.IgnoredMethods.Contains(context.Request.Method.ToUpperInvariant()))
        {
            return true;
        }

        return false;
    }

    private void LogRequestStart(HttpContext context, string requestId)
    {
        if (_options.OnlyLogErrors)
        {
            return;
        }

        var logEntry = CreateLogEntry(
            requestId: requestId,
            level: LogLevel.Information,
            message: $"{context.Request.Method} {context.Request.Path}{context.Request.QueryString} request starting",
            context: context
        );

        logEntry.Properties["EventType"] = "RequestStart";

        _logStore.Add(logEntry);
    }

    private void LogRequestEnd(HttpContext context, string requestId, DateTime startTime, long elapsedMs)
    {
        if (_options.OnlyLogErrors && context.Response.StatusCode < 400)
        {
            return;
        }

        var level = context.Response.StatusCode >= 500 ? LogLevel.Error :
                    context.Response.StatusCode >= 400 ? LogLevel.Warning : LogLevel.Information;

        var message = $"{context.Request.Method} {context.Request.Path} - {context.Response.StatusCode} {elapsedMs}ms";

        var logEntry = CreateLogEntry(
            requestId: requestId,
            level: level,
            message: message,
            context: context
        );

        logEntry.ResponseStatusCode = context.Response.StatusCode;
        logEntry.ElapsedMilliseconds = elapsedMs;
        logEntry.Properties["EventType"] = "RequestEnd";
        logEntry.Properties["StatusCode"] = context.Response.StatusCode;

        _logStore.Add(logEntry);
    }

    private void LogException(HttpContext context, string requestId, Exception ex, DateTime startTime, long elapsedMs)
    {
        var logEntry = CreateLogEntry(
            requestId: requestId,
            level: LogLevel.Error,
            message: $"Exception occurred: {ex.Message}",
            exception: ex,
            context: context
        );

        logEntry.ElapsedMilliseconds = elapsedMs;
        logEntry.Properties["EventType"] = "Exception";

        _logStore.Add(logEntry);
    }

    private LogEntry CreateLogEntry(
        string requestId,
        LogLevel level,
        string message,
        Exception? exception = null,
        HttpContext? context = null)
    {
        var logEntry = new LogEntry
        {
            RequestId = requestId,
            Level = level,
            Message = message,
            Timestamp = DateTime.Now,
            Source = _logger.GetType().Namespace,
            Logger = _logger.GetType().FullName,
            MachineName = _machineName,
            Application = _applicationName,
            AppVersion = _applicationVersion,
            SdkVersion = _sdkVersion,
            Environment = System.Diagnostics.Debugger.IsAttached ? "Development" : "Production",
            ProcessId = System.Diagnostics.Process.GetCurrentProcess().Id,
            ThreadId = Environment.CurrentManagedThreadId,
            ThreadName = Thread.CurrentThread.Name
        };

        if (context != null)
        {
            logEntry.RequestPath = context.Request.Path;
            logEntry.RequestMethod = context.Request.Method;

            if (context.Request.Query.Count > 0)
            {
                logEntry.Properties["QueryString"] = context.Request.QueryString.ToString();
            }

            // 记录客户端信息
            if (context.Request.Headers.TryGetValue("User-Agent", out var userAgent))
            {
                logEntry.Properties["UserAgent"] = userAgent.ToString();
            }

            if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
            {
                logEntry.Properties["ClientIp"] = forwardedFor.ToString().Split(',').First().Trim();
            }
            else
            {
                logEntry.Properties["ClientIp"] = context.Connection.RemoteIpAddress?.ToString();
            }

            // 记录 ConnectionId
            logEntry.ConnectionId = context.Connection.Id;
        }

        if (exception != null)
        {
            logEntry.Exception = exception.ToString();
            logEntry.StackTrace = exception.StackTrace;

            if (exception.InnerException != null)
            {
                logEntry.Properties["InnerException"] = exception.InnerException.ToString();
            }
        }

        return logEntry;
    }
}

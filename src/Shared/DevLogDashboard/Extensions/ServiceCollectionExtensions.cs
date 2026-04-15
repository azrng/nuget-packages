using Azrng.DevLogDashboard.Background;
using Azrng.DevLogDashboard.Middleware;
using Azrng.DevLogDashboard.Models;
using Azrng.DevLogDashboard.Options;
using Azrng.DevLogDashboard.Storage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Azrng.DevLogDashboard.Extensions;

/// <summary>
/// ServiceCollection extensions
/// </summary>
public static class ServiceCollectionExtensions
{
    private const string DefaultEndpointPath = "/dev-logs";
    private static readonly string RuntimeMachineName = Environment.MachineName;
    private static readonly int RuntimeProcessId = Environment.ProcessId;

    /// <summary>
    /// Add DevLogDashboard services with custom log store type.
    /// </summary>
    public static IServiceCollection AddDevLogDashboard<TLogStore>(
        this IServiceCollection services,
        Action<DevLogDashboardOptions>? configureOptions = null)
        where TLogStore : class, ILogStore
    {
        return services.AddDevLogDashboard<TLogStore>(configureOptions, null);
    }

    /// <summary>
    /// Add DevLogDashboard services with custom log store type and background queue options.
    /// </summary>
    public static IServiceCollection AddDevLogDashboard<TLogStore>(
        this IServiceCollection services,
        Action<DevLogDashboardOptions>? configureOptions = null,
        Action<BackgroundLogWriterOptions>? configureBackgroundOptions = null)
        where TLogStore : class, ILogStore
    {
        var options = CreateOptions(configureOptions);
        var backgroundOptions = CreateBackgroundOptions(configureBackgroundOptions);

        services.AddSingleton(options);
        services.AddSingleton<ILogStore, TLogStore>();
        return RegisterCommonServices(services, backgroundOptions);
    }

    /// <summary>
    /// Add DevLogDashboard services with custom log store factory.
    /// </summary>
    public static IServiceCollection AddDevLogDashboard(
        this IServiceCollection services,
        Func<IServiceProvider, ILogStore> logStoreFactory,
        Action<DevLogDashboardOptions>? configureOptions = null)
    {
        var options = CreateOptions(configureOptions);
        var backgroundOptions = CreateBackgroundOptions(null);

        services.AddSingleton(options);
        services.AddSingleton<ILogStore>(logStoreFactory);
        return RegisterCommonServices(services, backgroundOptions);
    }

    /// <summary>
    /// Add DevLogDashboard services.
    /// </summary>
    public static IServiceCollection AddDevLogDashboard(
        this IServiceCollection services,
        Action<DevLogDashboardOptions>? configureOptions = null)
    {
        var options = CreateOptions(configureOptions);
        var backgroundOptions = CreateBackgroundOptions(null);

        services.AddSingleton(options);
        services.AddSingleton<ILogStore>(sp =>
            new InMemoryLogStore(options.MaxLogCount));
        return RegisterCommonServices(services, backgroundOptions);
    }

    private static DevLogDashboardOptions CreateOptions(Action<DevLogDashboardOptions>? configureOptions)
    {
        var options = new DevLogDashboardOptions();
        configureOptions?.Invoke(options);
        NormalizeOptions(options);
        return options;
    }

    private static BackgroundLogWriterOptions CreateBackgroundOptions(Action<BackgroundLogWriterOptions>? configureBackgroundOptions)
    {
        var options = new BackgroundLogWriterOptions();
        configureBackgroundOptions?.Invoke(options);
        NormalizeBackgroundOptions(options);
        return options;
    }

    private static IServiceCollection RegisterCommonServices(IServiceCollection services, BackgroundLogWriterOptions backgroundOptions)
    {
        services.AddHttpContextAccessor();
        services.AddSingleton<IBackgroundLogQueue>(sp =>
            new BackgroundLogQueue(sp.GetRequiredService<DevLogDashboardOptions>().MaxLogCount));
        services.AddSingleton(backgroundOptions);
        services.AddHostedService<BackgroundLogWriter>();

        services.AddSingleton<ILoggerProvider>(sp =>
        {
            var opts = sp.GetRequiredService<DevLogDashboardOptions>();
            var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
            var logQueue = sp.GetRequiredService<IBackgroundLogQueue>();
            var hostEnvironment = sp.GetService<IHostEnvironment>();
            return new DevLogDashboardLoggerProvider(
                opts,
                httpContextAccessor,
                logQueue,
                hostEnvironment?.EnvironmentName);
        });

        return services;
    }

    /// <summary>
    /// Use DevLogDashboard middleware.
    /// </summary>
    public static IApplicationBuilder UseDevLogDashboard(this IApplicationBuilder app)
    {
        var options = app.ApplicationServices.GetRequiredService<DevLogDashboardOptions>();
        var logQueue = app.ApplicationServices.GetRequiredService<IBackgroundLogQueue>();
        var hostEnvironment = app.ApplicationServices.GetService<IHostEnvironment>();
        var environmentName = hostEnvironment?.EnvironmentName ?? (System.Diagnostics.Debugger.IsAttached ? "Development" : "Production");

        app.Use(async (context, next) =>
        {
            if (ShouldSkipRequest(context, options))
            {
                await next();
                return;
            }

            var requestId = context.TraceIdentifier;
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                await next();
                stopwatch.Stop();
                TryLogRequest(context, logQueue, options, requestId, stopwatch.ElapsedMilliseconds, environmentName);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                TryLogException(context, logQueue, options, requestId, ex, stopwatch.ElapsedMilliseconds, environmentName);
                throw;
            }
        });

        app.Map(options.EndpointPath, branch => branch.UseMiddleware<DevLogDashboardMiddleware>());

        return app;
    }

    private static bool ShouldSkipRequest(HttpContext context, DevLogDashboardOptions options)
    {
        var endpointPath = NormalizePath(options.EndpointPath, DefaultEndpointPath)!;

        var requestPath = context.Request.Path;
        var fullPath = context.Request.PathBase.Add(requestPath);

        if (requestPath.StartsWithSegments(endpointPath, StringComparison.OrdinalIgnoreCase)
            || fullPath.StartsWithSegments(endpointPath, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        foreach (var rawPath in options.IgnoredPaths ?? Array.Empty<string>())
        {
            var ignoredPath = NormalizePath(rawPath, null);
            if (ignoredPath == null)
            {
                continue;
            }

            if (requestPath.StartsWithSegments(ignoredPath, StringComparison.OrdinalIgnoreCase)
                || fullPath.StartsWithSegments(ignoredPath, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static void TryLogRequest(HttpContext context, IBackgroundLogQueue logQueue, DevLogDashboardOptions options,
        string requestId, long elapsedMs, string environmentName)
    {
        try
        {
            LogRequest(context, logQueue, options, requestId, elapsedMs, environmentName);
        }
        catch
        {
            // Dashboard logging must never break normal request flow.
        }
    }

    private static void TryLogException(HttpContext context, IBackgroundLogQueue logQueue, DevLogDashboardOptions options,
        string requestId, Exception ex, long elapsedMs, string environmentName)
    {
        try
        {
            LogException(context, logQueue, options, requestId, ex, elapsedMs, environmentName);
        }
        catch
        {
            // Dashboard logging must never mask original business exceptions.
        }
    }

    private static void LogRequest(HttpContext context, IBackgroundLogQueue logQueue, DevLogDashboardOptions options,
        string requestId, long elapsedMs, string environmentName)
    {
        var level = context.Response.StatusCode >= 500 ? LogLevel.Error :
                    context.Response.StatusCode >= 400 ? LogLevel.Warning : LogLevel.Information;

        if (options.OnlyLogErrors && level < LogLevel.Warning)
        {
            return;
        }

        var logEntry = new LogEntry
        {
            RequestId = requestId,
            ConnectionId = context.Connection.Id,
            Level = level,
            Message = $"{context.Request.Method} {context.Request.Path} - {context.Response.StatusCode} {elapsedMs}ms",
            Timestamp = DateTime.Now,
            RequestPath = context.Request.Path,
            RequestMethod = context.Request.Method,
            ResponseStatusCode = context.Response.StatusCode,
            ElapsedMilliseconds = elapsedMs,
            Source = "DevLogDashboard.RequestLogging",
            MachineName = RuntimeMachineName,
            Application = options.ApplicationName,
            AppVersion = options.ApplicationVersion,
            Environment = environmentName,
            ProcessId = RuntimeProcessId,
            ThreadId = Environment.CurrentManagedThreadId,
        };

        logEntry.Properties["EventType"] = "RequestEnd";
        logEntry.Properties["StatusCode"] = context.Response.StatusCode;

        // 异步写入队列
        _ = logQueue.QueueLogEntryAsync(logEntry);
    }

    private static void LogException(HttpContext context, IBackgroundLogQueue logQueue, DevLogDashboardOptions options,
        string requestId, Exception ex, long elapsedMs, string environmentName)
    {
        // 如果响应状态码显示成功但实际发生了异常，使用 500 作为状态码
        var actualStatusCode = context.Response.StatusCode;
        if (actualStatusCode < 400)
        {
            actualStatusCode = 500;
        }

        var logEntry = new LogEntry
        {
            RequestId = requestId,
            ConnectionId = context.Connection.Id,
            Level = LogLevel.Error,
            Message = $"Exception occurred: {ex.Message}",
            Timestamp = DateTime.Now,
            RequestPath = context.Request.Path,
            RequestMethod = context.Request.Method,
            ElapsedMilliseconds = elapsedMs,
            Source = "DevLogDashboard.RequestLogging",
            Exception = ex.ToString(),
            StackTrace = ex.StackTrace,
            MachineName = RuntimeMachineName,
            Application = options.ApplicationName,
            AppVersion = options.ApplicationVersion,
            Environment = environmentName,
            ProcessId = RuntimeProcessId,
            ThreadId = Environment.CurrentManagedThreadId,
            ResponseStatusCode = actualStatusCode,
        };

        logEntry.Properties["EventType"] = "Exception";
        logEntry.Properties["StatusCode"] = actualStatusCode;

        // 异步写入队列
        _ = logQueue.QueueLogEntryAsync(logEntry);
    }

    private static void NormalizeOptions(DevLogDashboardOptions options)
    {
        options.EndpointPath = NormalizePath(options.EndpointPath, DefaultEndpointPath)!;

        var ignoredPaths = (options.IgnoredPaths ?? Array.Empty<string>())
            .Select(path => NormalizePath(path, null))
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Select(path => path!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (!ignoredPaths.Contains(options.EndpointPath, StringComparer.OrdinalIgnoreCase))
        {
            ignoredPaths.Add(options.EndpointPath);
        }

        options.IgnoredPaths = ignoredPaths;

        if (options.MaxLogCount <= 0)
        {
            options.MaxLogCount = 10000;
        }

        if (options.MaxPropertySerializationLength <= 0)
        {
            options.MaxPropertySerializationLength = 2048;
        }
    }

    private static void NormalizeBackgroundOptions(BackgroundLogWriterOptions options)
    {
        if (options.BatchSize <= 0)
        {
            options.BatchSize = 100;
        }

        if (options.PollInterval <= TimeSpan.Zero)
        {
            options.PollInterval = TimeSpan.FromSeconds(1);
        }

        if (options.ShutdownFlushTimeout <= TimeSpan.Zero)
        {
            options.ShutdownFlushTimeout = TimeSpan.FromSeconds(5);
        }
    }

    private static string? NormalizePath(string? rawPath, string? fallback)
    {
        if (string.IsNullOrWhiteSpace(rawPath))
        {
            return fallback;
        }

        var path = rawPath.Trim();

        if (Uri.TryCreate(path, UriKind.Absolute, out var uri))
        {
            path = uri.AbsolutePath;
        }

        var queryOrFragmentIndex = path.IndexOfAny(new[] { '?', '#' });
        if (queryOrFragmentIndex >= 0)
        {
            path = path[..queryOrFragmentIndex];
        }

        path = path.Replace('\\', '/');

        while (path.Contains("//", StringComparison.Ordinal))
        {
            path = path.Replace("//", "/", StringComparison.Ordinal);
        }

        if (!path.StartsWith('/'))
        {
            path = "/" + path.TrimStart('/');
        }

        if (path.Length > 1)
        {
            path = path.TrimEnd('/');
        }

        return string.IsNullOrWhiteSpace(path) ? fallback : path;
    }
}

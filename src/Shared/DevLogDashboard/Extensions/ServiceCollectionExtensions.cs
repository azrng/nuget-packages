using Azrng.DevLogDashboard.Middleware;
using Azrng.DevLogDashboard.Models;
using Azrng.DevLogDashboard.Options;
using Azrng.DevLogDashboard.Storage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Azrng.DevLogDashboard.Extensions;

/// <summary>
/// ServiceCollection extensions
/// </summary>
public static class ServiceCollectionExtensions
{
    private const string DefaultEndpointPath = "/dev-logs";
    private static readonly string RuntimeMachineName = Environment.MachineName;
    private static readonly string RuntimeEnvironment = System.Diagnostics.Debugger.IsAttached ? "Development" : "Production";
    private static readonly int RuntimeProcessId = Environment.ProcessId;

    /// <summary>
    /// Add DevLogDashboard services.
    /// </summary>
    public static IServiceCollection AddDevLogDashboard(
        this IServiceCollection services,
        Action<DevLogDashboardOptions>? configureOptions = null)
    {
        var options = new DevLogDashboardOptions();
        configureOptions?.Invoke(options);
        NormalizeOptions(options);

        services.AddSingleton(options);

        services.AddSingleton<ILogStore>(sp =>
            new InMemoryLogStore(options.MaxLogCount));

        services.AddHttpContextAccessor();

        services.AddSingleton<ILoggerProvider>(sp =>
        {
            var logStore = sp.GetRequiredService<ILogStore>();
            var opts = sp.GetRequiredService<DevLogDashboardOptions>();
            var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
            return new DevLogDashboardLoggerProvider(logStore, opts, httpContextAccessor);
        });

        return services;
    }

    /// <summary>
    /// Use DevLogDashboard middleware.
    /// </summary>
    public static IApplicationBuilder UseDevLogDashboard(this IApplicationBuilder app)
    {
        var options = app.ApplicationServices.GetRequiredService<DevLogDashboardOptions>();
        var logStore = app.ApplicationServices.GetRequiredService<ILogStore>();

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
                TryLogRequest(context, logStore, options, requestId, stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                TryLogException(context, logStore, options, requestId, ex, stopwatch.ElapsedMilliseconds);
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

        if ((options.IgnoredMethods ?? Array.Empty<string>())
            .Contains(context.Request.Method, StringComparer.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }

    private static void TryLogRequest(HttpContext context, ILogStore logStore, DevLogDashboardOptions options,
        string requestId, long elapsedMs)
    {
        try
        {
            LogRequest(context, logStore, options, requestId, elapsedMs);
        }
        catch
        {
            // Dashboard logging must never break normal request flow.
        }
    }

    private static void TryLogException(HttpContext context, ILogStore logStore, DevLogDashboardOptions options,
        string requestId, Exception ex, long elapsedMs)
    {
        try
        {
            LogException(context, logStore, options, requestId, ex, elapsedMs);
        }
        catch
        {
            // Dashboard logging must never mask original business exceptions.
        }
    }

    private static void LogRequest(HttpContext context, ILogStore logStore, DevLogDashboardOptions options,
        string requestId, long elapsedMs)
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
            Environment = RuntimeEnvironment,
            ProcessId = RuntimeProcessId,
            ThreadId = Environment.CurrentManagedThreadId,
        };

        logEntry.Properties["EventType"] = "RequestEnd";
        logEntry.Properties["StatusCode"] = context.Response.StatusCode;

        logStore.Add(logEntry);
    }

    private static void LogException(HttpContext context, ILogStore logStore, DevLogDashboardOptions options,
        string requestId, Exception ex, long elapsedMs)
    {
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
            Environment = RuntimeEnvironment,
            ProcessId = RuntimeProcessId,
            ThreadId = Environment.CurrentManagedThreadId,
        };

        logEntry.Properties["EventType"] = "Exception";

        logStore.Add(logEntry);
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

        options.IgnoredMethods = (options.IgnoredMethods ?? Array.Empty<string>())
            .Where(method => !string.IsNullOrWhiteSpace(method))
            .Select(method => method.Trim().ToUpperInvariant())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (options.IgnoredMethods.Count == 0)
        {
            options.IgnoredMethods.Add("OPTIONS");
        }

        if (options.MaxLogCount <= 0)
        {
            options.MaxLogCount = 10000;
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

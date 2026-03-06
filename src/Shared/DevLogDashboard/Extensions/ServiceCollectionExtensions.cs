using Azrng.DevLogDashboard.Endpoints;
using DevLogDashboard.Middleware;
using DevLogDashboard.Options;
using DevLogDashboard.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace DevLogDashboard.Extensions;

/// <summary>
/// ServiceCollection 扩展方法
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 添加 DevLogDashboard 服务
    /// </summary>
    public static ILogDashboardBuilder AddDevLogDashboard(
        this IServiceCollection services,
        Action<DevLogDashboardOptions>? configureOptions = null)
    {
        var options = new DevLogDashboardOptions();
        configureOptions?.Invoke(options);

        // 注册选项
        services.AddSingleton(options);

        // 注册日志存储
        services.AddSingleton<ILogStore>(sp =>
            new InMemoryLogStore(options.MaxLogCount));

        // 注册 HTTP 上下文访问器
        services.AddHttpContextAccessor();

        // 注册日志提供程序
        services.AddSingleton<ILoggerProvider>(sp =>
        {
            var logStore = sp.GetRequiredService<ILogStore>();
            var opts = sp.GetRequiredService<DevLogDashboardOptions>();
            var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
            return new DevLogDashboardLoggerProvider(logStore, opts, httpContextAccessor);
        });

        return new DefaultLogDashboardBuilder(services, options);
    }

    /// <summary>
    /// 使用 DevLogDashboard 中间件
    /// </summary>
    public static IApplicationBuilder UseDevLogDashboard(this IApplicationBuilder app)
    {
        var options = app.ApplicationServices.GetRequiredService<DevLogDashboardOptions>();
        var logStore = app.ApplicationServices.GetRequiredService<ILogStore>();

        // 添加自定义日志提供程序
        var loggerFactory = app.ApplicationServices.GetService<ILoggerFactory>();
        if (loggerFactory != null)
        {
            var loggerProvider = app.ApplicationServices.GetService<ILoggerProvider>();
            if (loggerProvider is DevLogDashboardLoggerProvider provider)
            {
                loggerFactory.AddProvider(provider);
            }
        }

        // 使用请求日志中间件（需要在最前面）
        app.Use(async (context, next) =>
        {
            if (ShouldSkipRequest(context, options))
            {
                await next();
                return;
            }

            var requestId = context.TraceIdentifier;
            var startTime = DateTime.Now;
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                await next();
                stopwatch.Stop();
                LogRequest(context, logStore, options, requestId, startTime, stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                LogException(context, logStore, options, requestId, ex, startTime, stopwatch.ElapsedMilliseconds);
                throw;
            }
        });

        // 使用 Map 方式注册仪表板，完全自包含，不需要 UseRouting 和 UseEndpoints
        app.Map(options.EndpointPath, branch => branch.UseMiddleware<DevLogDashboardMiddleware>());

        return app;
    }

    private static bool ShouldSkipRequest(HttpContext context, DevLogDashboardOptions options)
    {
        // 检查是否跳过仪表板路径（组件本身的请求不应该被记录）
        if (context.Request.Path.StartsWithSegments(options.EndpointPath))
        {
            return true; // 跳过仪表板自身的日志
        }

        // 检查路径
        if (options.IgnoredPaths.Any(p => context.Request.Path.StartsWithSegments(p)))
        {
            return true;
        }

        // 检查方法
        if (options.IgnoredMethods.Contains(context.Request.Method.ToUpperInvariant()))
        {
            return true;
        }

        return false;
    }

    private static void LogRequest(HttpContext context, ILogStore logStore, DevLogDashboardOptions options,
        string requestId, DateTime startTime, long elapsedMs)
    {
        var level = context.Response.StatusCode >= 500 ? LogLevel.Error :
                    context.Response.StatusCode >= 400 ? LogLevel.Warning : LogLevel.Information;

        if (options.OnlyLogErrors && level < LogLevel.Warning)
        {
            return;
        }

        var logEntry = new LogEntry
        {
            Id = Guid.NewGuid().ToString("N"),
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
            MachineName = Environment.MachineName,
            Application = options.ApplicationName,
            AppVersion = options.ApplicationVersion,
            Environment = System.Diagnostics.Debugger.IsAttached ? "Development" : "Production",
            ProcessId = System.Diagnostics.Process.GetCurrentProcess().Id,
            ThreadId = Environment.CurrentManagedThreadId,
        };

        logEntry.Properties["EventType"] = "RequestEnd";
        logEntry.Properties["StatusCode"] = context.Response.StatusCode;

        logStore.Add(logEntry);
    }

    private static void LogException(HttpContext context, ILogStore logStore, DevLogDashboardOptions options,
        string requestId, Exception ex, DateTime startTime, long elapsedMs)
    {
        var logEntry = new LogEntry
        {
            Id = Guid.NewGuid().ToString("N"),
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
            MachineName = Environment.MachineName,
            Application = options.ApplicationName,
            AppVersion = options.ApplicationVersion,
            Environment = System.Diagnostics.Debugger.IsAttached ? "Development" : "Production",
            ProcessId = System.Diagnostics.Process.GetCurrentProcess().Id,
            ThreadId = Environment.CurrentManagedThreadId,
        };

        logEntry.Properties["EventType"] = "Exception";

        logStore.Add(logEntry);
    }
}

/// <summary>
/// DevLogDashboard 构建器接口
/// </summary>
public interface ILogDashboardBuilder
{
    IServiceCollection Services { get; }
    DevLogDashboardOptions Options { get; }
}

/// <summary>
/// 默认 DevLogDashboard 构建器
/// </summary>
internal class DefaultLogDashboardBuilder : ILogDashboardBuilder
{
    public IServiceCollection Services { get; }
    public DevLogDashboardOptions Options { get; }

    public DefaultLogDashboardBuilder(IServiceCollection services, DevLogDashboardOptions options)
    {
        Services = services;
        Options = options;
    }

    /// <summary>
    /// 配置授权过滤器
    /// </summary>
    public ILogDashboardBuilder WithAuthorization(Func<HttpContext, Task<bool>> filter)
    {
        Options.AuthorizationFilter = filter;
        return this;
    }

    /// <summary>
    /// 配置最大日志数量
    /// </summary>
    public ILogDashboardBuilder WithMaxLogCount(int maxLogCount)
    {
        Options.MaxLogCount = maxLogCount;
        return this;
    }

    /// <summary>
    /// 配置仪表板路径
    /// </summary>
    public ILogDashboardBuilder WithEndpointPath(string path)
    {
        Options.EndpointPath = path;
        return this;
    }

    /// <summary>
    /// 配置忽略的路径
    /// </summary>
    public ILogDashboardBuilder WithIgnoredPaths(IEnumerable<string> paths)
    {
        Options.IgnoredPaths = paths.ToList();
        return this;
    }

    /// <summary>
    /// 配置应用名称
    /// </summary>
    public ILogDashboardBuilder WithApplicationName(string name)
    {
        Options.ApplicationName = name;
        return this;
    }

    /// <summary>
    /// 配置应用版本
    /// </summary>
    public ILogDashboardBuilder WithApplicationVersion(string version)
    {
        Options.ApplicationVersion = version;
        return this;
    }
}

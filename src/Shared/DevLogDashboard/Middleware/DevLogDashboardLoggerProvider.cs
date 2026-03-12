using Azrng.DevLogDashboard.Background;
using Azrng.DevLogDashboard.Options;
using Azrng.DevLogDashboard.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Azrng.DevLogDashboard.Middleware;

/// <summary>
/// DevLogDashboard 日志提供程序
/// </summary>
[ProviderAlias("DevLogDashboard")]
public class DevLogDashboardLoggerProvider : ILoggerProvider, ISupportExternalScope
{
    private readonly Func<ILogStore> _logStoreFactory;
    private readonly DevLogDashboardOptions _options;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IBackgroundLogQueue _logQueue;
    private readonly ConcurrentDictionary<string, DevLogDashboardLogger> _loggers = new();

    public DevLogDashboardLoggerProvider(
        Func<ILogStore> logStoreFactory,
        DevLogDashboardOptions options,
        IHttpContextAccessor httpContextAccessor,
        IBackgroundLogQueue logQueue)
    {
        _logStoreFactory = logStoreFactory ?? throw new ArgumentNullException(nameof(logStoreFactory));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _logQueue = logQueue ?? throw new ArgumentNullException(nameof(logQueue));
    }

    public ILogger CreateLogger(string categoryName)
    {
        return _loggers.GetOrAdd(categoryName, name =>
            new DevLogDashboardLogger(name, _logStoreFactory, _options, _httpContextAccessor, _logQueue));
    }

    public void SetScopeProvider(IExternalScopeProvider scopeProvider)
    { }

    public void Dispose()
    {
        _loggers.Clear();
    }
}

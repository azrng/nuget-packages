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
    private readonly ILogStore _logStore;
    private readonly DevLogDashboardOptions _options;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ConcurrentDictionary<string, DevLogDashboardLogger> _loggers = new();

    public DevLogDashboardLoggerProvider(ILogStore logStore, DevLogDashboardOptions options, IHttpContextAccessor httpContextAccessor)
    {
        _logStore = logStore;
        _options = options;
        _httpContextAccessor = httpContextAccessor;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return _loggers.GetOrAdd(categoryName, name =>
            new DevLogDashboardLogger(name, _logStore, _options, _httpContextAccessor));
    }

    public void SetScopeProvider(IExternalScopeProvider scopeProvider)
    { }

    public void Dispose()
    {
        _loggers.Clear();
    }
}

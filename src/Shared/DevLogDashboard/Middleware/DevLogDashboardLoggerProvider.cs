using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using DevLogDashboard.Storage;
using DevLogDashboard.Options;

namespace DevLogDashboard.Middleware;

/// <summary>
/// DevLogDashboard 日志提供程序
/// </summary>
[ProviderAlias("DevLogDashboard")]
public class DevLogDashboardLoggerProvider : ILoggerProvider, ISupportExternalScope
{
    private readonly ILogStore _logStore;
    private readonly DevLogDashboardOptions _options;
    private readonly ConcurrentDictionary<string, DevLogDashboardLogger> _loggers = new();
    private IExternalScopeProvider? _scopeProvider;

    public DevLogDashboardLoggerProvider(ILogStore logStore, DevLogDashboardOptions options)
    {
        _logStore = logStore;
        _options = options;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return _loggers.GetOrAdd(categoryName, name =>
            new DevLogDashboardLogger(name, _logStore, _options));
    }

    public void SetScopeProvider(IExternalScopeProvider scopeProvider)
    {
        _scopeProvider = scopeProvider;
    }

    public void Dispose()
    {
        _loggers.Clear();
    }
}

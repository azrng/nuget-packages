using Microsoft.Extensions.Configuration;

namespace Azrng.AspNetCore.DbEnvConfig;

/// <summary>
/// 数据库配置源
/// </summary>
internal class DbConfigurationSource : IConfigurationSource
{
    private readonly DBConfigOptions _options;
    private readonly IScriptService _scriptService;
    private DbConfigurationProvider? _uniqueInstance;
    private readonly object _locker = new object();

    public DbConfigurationSource(DBConfigOptions options, IScriptService scriptService)
    {
        _options = options;
        _scriptService = scriptService;
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        if (_uniqueInstance is null)
        {
            lock (_locker)
            {
                _uniqueInstance ??= new DbConfigurationProvider(_options, _scriptService);
            }
        }

        return _uniqueInstance;
    }
}
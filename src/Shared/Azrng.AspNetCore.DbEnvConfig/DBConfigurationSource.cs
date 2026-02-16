using Microsoft.Extensions.Configuration;

namespace Azrng.AspNetCore.DbEnvConfig;

/// <summary>
/// 数据库配置源
/// </summary>
/// <remarks>
/// 此类负责创建 <see cref="DbConfigurationProvider"/> 实例
/// 使用单例模式确保同一个配置源只创建一个提供程序实例
/// </remarks>
internal class DbConfigurationSource : IConfigurationSource
{
    private readonly DBConfigOptions _options;
    private readonly IScriptService _scriptService;
    private DbConfigurationProvider? _uniqueInstance;
    private readonly object _locker = new();

    /// <summary>
    /// 初始化 <see cref="DbConfigurationSource"/> 的新实例
    /// </summary>
    /// <param name="options">配置选项</param>
    /// <param name="scriptService">脚本服务</param>
    public DbConfigurationSource(DBConfigOptions options, IScriptService scriptService)
    {
        _options = options;
        _scriptService = scriptService;
    }

    /// <summary>
    /// 构建 <see cref="IConfigurationProvider"/> 实例
    /// </summary>
    /// <param name="builder">配置构建器</param>
    /// <returns>数据库配置提供程序实例</returns>
    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        if (_uniqueInstance is null)
        {
            lock (_locker)
            {
                _uniqueInstance ??= new DbConfigurationProvider(_options, _scriptService);
            }
        }

        return _uniqueInstance!;
    }
}
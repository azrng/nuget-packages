using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Azrng.AspNetCore.DbEnvConfig;

/// <summary>
/// 数据库配置源
/// </summary>
/// <remarks>
/// 此类负责创建 <see cref="DbConfigurationProvider"/> 实例。
/// 每次 <see cref="Build"/> 都创建新的 Provider，避免已 Dispose 的 Provider 被同一个配置源再次复用。
/// </remarks>
internal class DbConfigurationSource : IConfigurationSource
{
    private readonly DbConfigOptions _options;
    private readonly IScriptService _scriptService;
    private readonly ILogger? _logger;

    /// <summary>
    /// 初始化 <see cref="DbConfigurationSource"/> 的新实例
    /// </summary>
    /// <param name="options">配置选项</param>
    /// <param name="scriptService">脚本服务</param>
    /// <param name="logger">日志记录器（可选）</param>
    public DbConfigurationSource(DbConfigOptions options, IScriptService scriptService, ILogger? logger = null)
    {
        _options = options;
        _scriptService = scriptService;
        _logger = logger;
    }

    /// <summary>
    /// 构建 <see cref="IConfigurationProvider"/> 实例
    /// </summary>
    /// <param name="builder">配置构建器</param>
    /// <returns>数据库配置提供程序实例</returns>
    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new DbConfigurationProvider(_options, _scriptService, _logger);
    }
}

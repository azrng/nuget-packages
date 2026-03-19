using Azrng.Core.Model;

namespace Azrng.Database.DynamicSqlBuilder;

/// <summary>
/// SQL构建器配置选项
/// </summary>
public class SqlBuilderOptions
{
    /// <summary>
    /// 数据库方言（默认PostgreSQL）
    /// </summary>
    public DatabaseType Dialect { get; set; } = DatabaseType.PostgresSql;

    /// <summary>
    /// 是否启用SQL日志记录
    /// </summary>
    public bool EnableSqlLogging { get; set; }

    /// <summary>
    /// 是否启用字段名验证（默认启用）
    /// </summary>
    public bool EnableFieldNameValidation { get; set; } = true;

    /// <summary>
    /// SQL生成完成时的回调
    /// </summary>
    public Action<string, object> OnSqlGenerated { get; set; }

    /// <summary>
    /// 默认配置实例
    /// </summary>
    public static readonly SqlBuilderOptions Default = new();
}

/// <summary>
/// SQL构建器配置管理器
/// </summary>
public static class SqlBuilderConfigurer
{
    private static SqlBuilderOptions _currentOptions = SqlBuilderOptions.Default;

    /// <summary>
    /// 获取当前配置
    /// </summary>
    public static SqlBuilderOptions GetCurrentOptions() => _currentOptions;

    /// <summary>
    /// 配置SQL构建器
    /// </summary>
    /// <param name="configureOptions">配置委托</param>
    public static void Configure(Action<SqlBuilderOptions> configureOptions)
    {
        if (configureOptions == null)
            throw new ArgumentNullException(nameof(configureOptions));

        configureOptions(_currentOptions);
    }

    /// <summary>
    /// 重置为默认配置
    /// </summary>
    public static void ResetToDefault()
    {
        _currentOptions = SqlBuilderOptions.Default;
    }
}

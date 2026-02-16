using Microsoft.Extensions.Configuration;

namespace Azrng.AspNetCore.DbEnvConfig;

/// <summary>
/// 数据库配置提供程序扩展
/// </summary>
public static class DbConfigurationProviderExtensions
{
    /// <summary>
    /// 添加数据库配置提供程序到 IConfigurationBuilder
    /// </summary>
    /// <param name="builder">配置构建器</param>
    /// <param name="action">配置选项设置</param>
    /// <param name="scriptService">自定义脚本服务（可选）</param>
    /// <returns>配置构建器</returns>
    /// <exception cref="ArgumentNullException">当 action 为 null 时抛出</exception>
    /// <exception cref="ArgumentException">当配置参数无效时抛出</exception>
    /// <example>
    /// 示例：添加数据库配置
    /// <code>
    /// builder.Configuration.AddDbConfiguration(options =>
    /// {
    ///     options.CreateDbConnection = () => new NpgsqlConnection(connectionString);
    ///     options.TableName = "config.system_config";
    ///     options.ConfigKeyField = "code";
    ///     options.ConfigValueField = "value";
    ///     options.FilterWhere = " AND is_delete = false";
    /// });
    /// </code>
    /// </example>
    public static IConfigurationBuilder AddDbConfiguration(
        this IConfigurationBuilder builder,
        Action<DBConfigOptions> action,
        IScriptService? scriptService = null)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        var setup = new DBConfigOptions();
        action(setup);

        setup.ParamVerify();
        return builder.Add(new DbConfigurationSource(setup, scriptService ?? new DefaultScriptService()));
    }
}
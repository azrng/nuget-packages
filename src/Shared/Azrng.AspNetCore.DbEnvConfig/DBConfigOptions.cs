using System.Data;

namespace Azrng.AspNetCore.DbEnvConfig;

/// <summary>
/// 数据库配置选项
/// </summary>
public class DBConfigOptions
{
    /// <summary>
    /// 获取或设置数据库连接工厂函数
    /// </summary>
    public Func<IDbConnection>? CreateDbConnection { get; set; }

    /// <summary>
    /// 获取数据库模式（Schema）
    /// </summary>
    public string? Schema { get; private set; }

    /// <summary>
    /// 获取或设置表名
    /// </summary>
    /// <remarks>
    /// 如果是 PostgreSQL，需要指定 schema，格式为 "schema.table" 或 "table"
    /// 例如："config.system_config" 或 "system_config"
    /// </remarks>
    /// <example>
    /// "system_config" 或 "config.system_config"
    /// </example>
    public string TableName { get; set; } = "config.system_config";

    /// <summary>
    /// 获取完整表名
    /// </summary>
    public string? FullTableName { get; private set; }

    /// <summary>
    /// 获取或设置配置键所属的列名
    /// </summary>
    public string ConfigKeyField { get; set; } = "code";

    /// <summary>
    /// 获取或设置配置值所属的列名
    /// </summary>
    public string ConfigValueField { get; set; } = "value";

    /// <summary>
    /// 获取或设置是否自动刷新配置
    /// </summary>
    public bool ReloadOnChange { get; set; } = true;

    /// <summary>
    /// 获取或设置刷新间隔
    /// </summary>
    public TimeSpan ReloadInterval { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// 获取或设置 SQL 查询筛选条件
    /// </summary>
    /// <remarks>
    /// 示例：" AND is_delete = false"
    /// 注意：需要以 " AND" 开头
    /// </remarks>
    public string? FilterWhere { get; set; }

    /// <summary>
    /// 获取或设置是否输出查询日志
    /// </summary>
    public bool IsConsoleQueryLog { get; set; } = true;

    /// <summary>
    /// 验证配置参数
    /// </summary>
    /// <exception cref="ArgumentNullException">当必需参数为空时抛出</exception>
    /// <exception cref="ArgumentException">当参数格式不正确时抛出</exception>
    public void ParamVerify()
    {
        if (CreateDbConnection == null)
        {
            throw new ArgumentNullException(nameof(CreateDbConnection), "数据库连接工厂不能为空");
        }

        if (string.IsNullOrWhiteSpace(TableName))
        {
            throw new ArgumentException("数据库配置所属表不能为空", nameof(TableName));
        }

        var arr = TableName.Split('.');
        if (arr.Length > 2)
        {
            throw new ArgumentException("表名称格式错误，应该是 'table' 或 'schema.table'", nameof(TableName));
        }

        if (arr.Length == 2)
        {
            Schema = arr[0];
            TableName = arr[1];
            FullTableName = $"{Schema}.{TableName}";
        }
        else
        {
            FullTableName = TableName;
        }

        if (string.IsNullOrWhiteSpace(ConfigKeyField))
        {
            throw new ArgumentException("数据库配置 Key 所属列不能为空", nameof(ConfigKeyField));
        }

        if (string.IsNullOrWhiteSpace(ConfigValueField))
        {
            throw new ArgumentException("数据库配置 Value 所属列不能为空", nameof(ConfigValueField));
        }
    }
}
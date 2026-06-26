using System.Data;

namespace Azrng.AspNetCore.DbEnvConfig;

/// <summary>
/// 数据库配置选项
/// </summary>
public class DbConfigOptions
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
    /// <br/>
    /// <b>安全警告</b>：该值会以原始字符串拼接进 SQL 语句，不会参数化。
    /// 请仅使用编译期静态字面量，<b>切勿</b>来自用户输入、请求参数或其它不可信来源，
    /// 否则会引入 SQL 注入风险。同理，<see cref="TableName"/>、<see cref="Schema"/>、
    /// <see cref="ConfigKeyField"/>、<see cref="ConfigValueField"/> 也不可来自不可信来源。
    /// </remarks>
    public string? FilterWhere { get; set; }

    /// <summary>
    /// 获取或设置是否输出查询日志
    /// </summary>
    public bool IsConsoleQueryLog { get; set; } = true;

    /// <summary>
    /// 验证配置参数并规范化表名（拆分 schema、计算 <see cref="FullTableName"/>）
    /// </summary>
    /// <exception cref="ArgumentNullException">当必需参数为空时抛出</exception>
    /// <exception cref="ArgumentException">当参数格式不正确时抛出</exception>
    public void Normalize()
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

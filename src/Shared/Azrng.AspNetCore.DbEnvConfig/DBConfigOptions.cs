using System.Data;

namespace Azrng.AspNetCore.DbEnvConfig;

public class DBConfigOptions
{
    public Func<IDbConnection> CreateDbConnection { get; set; } = null!;

    /// <summary>
    /// 模式
    /// </summary>
    public string? Schema { get; private set; }

    /// <summary>
    /// 表名 如果是pgsql需要指定schema
    /// </summary>
    /// <example>system_config 或者 config.system_config</example>
    public string TableName { get; set; } = "config.system_config";

    /// <summary>
    /// 完整表名
    /// </summary>
    public string FullTableName { get; private set; } = null!;

    /// <summary>
    /// 配置Key所属列
    /// </summary>
    public string ConfigKeyField { get; set; } = "code";

    /// <summary>
    /// 配置Value所属列
    /// </summary>
    public string ConfigValueField { get; set; } = "value";

    /// <summary>
    /// 是否自动刷新
    /// </summary>
    public bool ReloadOnChange { get; set; } = true;

    /// <summary>
    /// 刷新间隔
    /// </summary>
    public TimeSpan ReloadInterval { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// 筛选条件
    /// </summary>
    public string FilterWhere { get; set; } = null!;

    /// <summary>
    /// 输出日志
    /// </summary>
    public bool IsConsoleQueryLog { get; set; } = true;

    public void ParamVerify()
    {
        if (string.IsNullOrWhiteSpace(TableName))
        {
            throw new ArgumentException("数据库配置所属表不能为空！");
        }

        var arr = TableName.Split(".");
        if (arr.Length > 2)
        {
            throw new ArgumentException("表名称设置异常");
        }

        if (arr.Length == 2)
        {
            Schema = arr[0];
            TableName = arr[1];
            FullTableName = $"{Schema}.{TableName}";
        }
        else
        {
            TableName = arr[0];
            FullTableName = TableName;
        }

        if (string.IsNullOrWhiteSpace(ConfigKeyField))
        {
            throw new ArgumentException("数据库配置Key所属列不能为空！");
        }

        if (string.IsNullOrWhiteSpace(ConfigValueField))
        {
            throw new ArgumentException("数据库配置Value所属列不能为空！");
        }
    }
}
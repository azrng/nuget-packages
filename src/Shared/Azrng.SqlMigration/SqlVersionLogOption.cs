namespace Azrng.SqlMigration;

/// <summary>
/// 版本日志表配置
/// </summary>
public class SqlVersionLogOption
{
    private string _idColumn = "id";
    private string? _orderByColumn;

    /// <summary>
    /// 版本日志表名
    /// </summary>
    public string TableName { get; set; } = "app_version_log";

    /// <summary>
    /// 主键列名
    /// </summary>
    public string IdColumn
    {
        get => _idColumn;
        set => _idColumn = value;
    }

    /// <summary>
    /// 版本列名
    /// </summary>
    public string VersionColumn { get; set; } = "version";

    /// <summary>
    /// 排序列名，默认跟随 <see cref="IdColumn"/>
    /// </summary>
    public string OrderByColumn
    {
        get => string.IsNullOrWhiteSpace(_orderByColumn) ? IdColumn : _orderByColumn;
        set => _orderByColumn = value;
    }

    /// <summary>
    /// 自定义版本日志表初始化 SQL。
    /// 未设置时使用库内置的默认建表逻辑。
    /// </summary>
    public string? InitTableSql { get; set; }
}

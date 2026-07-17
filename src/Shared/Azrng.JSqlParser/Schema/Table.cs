using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Statement.Select;

namespace Azrng.JSqlParser.Schema;

/// <summary>
/// Represents a table reference in SQL.
/// </summary>
public class Table : ASTNodeAccessImpl, IFromItem
{
    /// <summary>服务器/实例名（4 段命名 server.db.schema.name 的首段），对齐上游 Server。未指定时为 null。</summary>
    public string? ServerName { get; set; }
    public string? Database { get; set; }
    public string? SchemaName { get; set; }
    public string Name { get; set; } = "";
    public Alias? Alias { get; set; }

    /// <summary>SQL Server 表提示（WITH (NOLOCK) 等），出现在表后。未指定时为 null。</summary>
    public SQLServerHints? SqlServerHints { get; set; }

    /// <summary>MySQL 索引提示（USE/IGNORE/FORCE INDEX/KEY (...)），出现在表后。未指定时为 null。</summary>
    public MySQLIndexHint? MySqlIndexHint { get; set; }

    /// <summary>TABLESAMPLE 子句（FROM 子句采样），未指定时为 null。</summary>
    public TableSample? TableSample { get; set; }

    /// <summary>时间旅行子句（Snowflake AT/BEFORE），未指定时为 null。</summary>
    public TimeTravelClause? TimeTravel { get; set; }

    /// <summary>FROM 子句 PIVOT（行列转换），未指定时为 null。</summary>
    public Statement.Select.Pivot? Pivot { get; set; }

    /// <summary>FROM 子句 UNPIVOT，未指定时为 null。</summary>
    public Statement.Select.UnPivot? UnPivot { get; set; }

    /// <summary>别名后的 time-travel（BigQuery @ / FOR SYSTEM_TIME AS OF 出现在 alias 之后），对齐上游 timeTravelStrAfterAlias。</summary>
    public TimeTravelClause? TimeTravelAfterAlias { get; set; }

    public string GetFullyQualifiedName()
    {
        var parts = new System.Collections.Generic.List<string>();
        if (ServerName != null) parts.Add(ServerName);
        if (Database != null) parts.Add(Database);
        if (SchemaName != null) parts.Add(SchemaName);
        parts.Add(Name);
        return string.Join(".", parts);
    }

    /// <summary>返回别名（兼容旧 API，改用 <see cref="Alias"/> 属性）。</summary>
    [Obsolete("改用 " + nameof(Alias) + " 属性")]
    public Alias? GetAlias() => Alias;
    /// <summary>设置别名（兼容旧 API，改用 <see cref="Alias"/> 属性）。</summary>
    [Obsolete("改用 " + nameof(Alias) + " 属性")]
    public void SetAlias(Alias alias) { Alias = alias; }

    public override string ToString()
    {
        var name = GetFullyQualifiedName();
        var result = Alias != null ? $"{name} {Alias}" : name;
        if (TimeTravelAfterAlias != null) result += $" {TimeTravelAfterAlias}";
        if (SqlServerHints != null) result += SqlServerHints;
        if (MySqlIndexHint != null) result += MySqlIndexHint;
        if (TableSample != null) result += $" {TableSample}";
        if (TimeTravel != null) result += $" {TimeTravel}";
        if (Pivot != null) result += $" {Pivot}";
        if (UnPivot != null) result += $" {UnPivot}";
        return result;
    }
}

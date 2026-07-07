using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Statement.Select;

namespace Azrng.JSqlParser.Schema;

/// <summary>
/// Represents a table reference in SQL.
/// </summary>
public class Table : ASTNodeAccessImpl, FromItem
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

    public string GetFullyQualifiedName()
    {
        var parts = new System.Collections.Generic.List<string>();
        if (ServerName != null) parts.Add(ServerName);
        if (Database != null) parts.Add(Database);
        if (SchemaName != null) parts.Add(SchemaName);
        parts.Add(Name);
        return string.Join(".", parts);
    }

    public Alias? GetAlias() => Alias;
    public void SetAlias(Alias alias) { Alias = alias; }

    public override string ToString()
    {
        var name = GetFullyQualifiedName();
        var result = Alias != null ? $"{name} {Alias}" : name;
        if (SqlServerHints != null) result += SqlServerHints;
        if (MySqlIndexHint != null) result += MySqlIndexHint;
        return result;
    }
}

using System.Text;
using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Schema;

namespace Azrng.JSqlParser.Statement.Select;

/// <summary>
/// SQL/JSON 的 JSON_TABLE 表函数。
/// <para>
/// 语法：JSON_TABLE(json_expr [, path] COLUMNS (col_def, ...)) [AS] alias
/// </para>
/// 移植自上游 JSqlParser commit c5e2fdcd 的 JsonTableFunction（简化版）。
/// </summary>
public class JsonTable : ASTNodeAccessImpl, FromItem
{
    /// <summary>JSON 文档表达式（第一个参数）。</summary>
    public Azrng.JSqlParser.Expression.Expression? JsonExpression { get; set; }

    /// <summary>JSON 路径（如 '$.path'），未指定时为 null。</summary>
    public string? PathExpression { get; set; }

    /// <summary>列定义列表。</summary>
    public List<JsonTableColumn> Columns { get; set; } = new();

    /// <summary>FROM 子句中的别名。</summary>
    public Alias? Alias { get; set; }

    public Alias? GetAlias() => Alias;
    public void SetAlias(Alias alias) => Alias = alias;

    public override string ToString()
    {
        var sb = new StringBuilder("JSON_TABLE(");
        sb.Append(JsonExpression);
        if (PathExpression != null) sb.Append(", ").Append(PathExpression);
        sb.Append(" COLUMNS (");
        for (int i = 0; i < Columns.Count; i++)
        {
            if (i > 0) sb.Append(", ");
            sb.Append(Columns[i]);
        }
        sb.Append("))");
        if (Alias != null) sb.Append(' ').Append(Alias);
        return sb.ToString();
    }
}

/// <summary>
/// JSON_TABLE 的列定义。
/// </summary>
public class JsonTableColumn
{
    /// <summary>列名。</summary>
    public string Name { get; set; } = "";

    /// <summary>列数据类型（如 INT、VARCHAR(100)），FOR ORDINALITY 时为 null。</summary>
    public string? DataType { get; set; }

    /// <summary>JSON 路径（PATH '...'），未指定时为 null。</summary>
    public string? Path { get; set; }

    /// <summary>是否为 FOR ORDINALITY（序号列）。</summary>
    public bool ForOrdinality { get; set; }

    public override string ToString()
    {
        if (ForOrdinality) return $"{Name} FOR ORDINALITY";
        var sb = new StringBuilder(Name);
        if (DataType != null) sb.Append(' ').Append(DataType);
        if (Path != null) sb.Append(" PATH ").Append(Path);
        return sb.ToString();
    }
}

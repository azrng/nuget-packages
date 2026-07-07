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

    /// <summary>PASSING 子句的参数列表（value AS name 形式）。</summary>
    public List<JsonTablePassingClause> PassingClauses { get; } = new();

    /// <summary>ON ERROR 行为（NULL/ERROR/EMPTY 等），未指定时为 null。</summary>
    public string? OnErrorBehavior { get; set; }

    /// <summary>ON EMPTY 行为，未指定时为 null。</summary>
    public string? OnEmptyBehavior { get; set; }

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

        // PASSING
        if (PassingClauses.Count > 0)
        {
            sb.Append(" PASSING ").Append(string.Join(", ", PassingClauses));
        }

        // ON ERROR / ON EMPTY（按上游，COLUMNS 前）
        if (OnErrorBehavior != null) sb.Append(' ').Append(OnErrorBehavior).Append(" ON ERROR");
        if (OnEmptyBehavior != null) sb.Append(' ').Append(OnEmptyBehavior).Append(" ON EMPTY");

        sb.Append(" COLUMNS (");
        for (int i = 0; i < Columns.Count; i++)
        {
            if (i > 0) sb.Append(", ");
            sb.Append(Columns[i]);
        }
        sb.Append(')');

        sb.Append(')');
        if (Alias != null) sb.Append(' ').Append(Alias);
        return sb.ToString();
    }
}

/// <summary>
/// JSON_TABLE 的 PASSING 子句项：value AS name。
/// </summary>
public class JsonTablePassingClause
{
    public Azrng.JSqlParser.Expression.Expression? ValueExpression { get; set; }

    public string? ParameterName { get; set; }

    public override string ToString()
        => ParameterName != null ? $"{ValueExpression} AS {ParameterName}" : ValueExpression?.ToString() ?? "";
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

    /// <summary>NESTED PATH 子列定义（递归），非 NESTED 时为 null。</summary>
    public List<JsonTableColumn>? NestedColumns { get; set; }

    /// <summary>是否为 NESTED PATH 列。</summary>
    public bool IsNested => NestedColumns != null;

    public override string ToString()
    {
        if (IsNested)
        {
            var sb = new StringBuilder("NESTED PATH ");
            if (Path != null) sb.Append(Path);
            sb.Append(" COLUMNS (");
            for (int i = 0; i < NestedColumns!.Count; i++)
            {
                if (i > 0) sb.Append(", ");
                sb.Append(NestedColumns[i]);
            }
            sb.Append(')');
            return sb.ToString();
        }
        if (ForOrdinality) return $"{Name} FOR ORDINALITY";
        var sb2 = new StringBuilder(Name);
        if (DataType != null) sb2.Append(' ').Append(DataType);
        if (Path != null) sb2.Append(" PATH ").Append(Path);
        return sb2.ToString();
    }
}

using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Schema;
using SelectStatement = Azrng.JSqlParser.Statement.Select.Select;

namespace Azrng.JSqlParser.Statement.CreateTable;

/// <summary>
/// Represents a CREATE TABLE statement in SQL.
/// </summary>
/// <remarks>
/// 对齐上游 <c>net.sf.jsqlparser.statement.create.table.CreateTable</c>。
/// 表级选项（ENGINE / CHARSET / PARTITION BY / ORDER BY / SAMPLE BY 等）与 CREATE 选项
/// 采用字符串透传策略（List&lt;string&gt;），与上游 <c>tableOptionsStrings</c> / <c>createOptionsStrings</c> 的 round-trip 行为一致。
/// </remarks>
public class CreateTable : ASTNodeAccessImpl, Statement
{
    public Table? Table { get; set; }
    public System.Collections.Generic.List<ColumnDefinition>? ColumnDefinitions { get; set; }

    /// <summary>表级约束/索引列表（可含 <see cref="ForeignKeyIndex"/>/<see cref="CheckConstraint"/>/<see cref="ExcludeConstraint"/> 子类）。</summary>
    public System.Collections.Generic.List<Constraint>? Constraints { get; set; }

    /// <summary>仅列名形式（CREATE TABLE t (c1, c2) AS SELECT），无类型定义。为 null 时忽略。</summary>
    public System.Collections.Generic.List<string>? Columns { get; set; }

    public bool IfNotExists { get; set; }

    /// <summary>CREATE OR REPLACE。</summary>
    public bool OrReplace { get; set; }

    /// <summary>CREATE UNLOGGED（PostgreSQL）。</summary>
    public bool Unlogged { get; set; }

    /// <summary>CREATE 关键字之后的选项原始字符串（GLOBAL / TEMPORARY / TEMP / EXTERNAL 等）。</summary>
    public System.Collections.Generic.List<string>? CreateOptions { get; set; }

    /// <summary>表选项原始字符串（ENGINE / CHARSET / COLLATE / COMMENT / PARTITION BY / ORDER BY / SAMPLE BY / ROW_FORMAT 等），按出现顺序透传。</summary>
    public System.Collections.Generic.List<string>? TableOptions { get; set; }

    /// <summary>CREATE TABLE ... AS SELECT（CTAS）的查询语句。</summary>
    public SelectStatement? Select { get; set; }

    /// <summary>LIKE 表（CREATE TABLE t LIKE other / LIKE (other)）。</summary>
    public Table? LikeTable { get; set; }

    /// <summary>AS SELECT / LIKE 表是否带括号。</summary>
    public bool SelectParenthesis { get; set; }

    /// <summary>Oracle ENABLE/DISABLE ROW MOVEMENT。</summary>
    public RowMovement? RowMovement { get; set; }

    /// <summary>Spanner INTERLEAVE IN PARENT 子句。</summary>
    public SpannerInterleaveIn? InterleaveIn { get; set; }

    public T Accept<T, S>(IStatementVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder("CREATE ");
        if (Unlogged) sb.Append("UNLOGGED ");
        if (CreateOptions is { Count: > 0 })
            sb.Append(string.Join(" ", CreateOptions)).Append(' ');
        if (OrReplace) sb.Append("OR REPLACE ");
        sb.Append("TABLE ");
        if (IfNotExists) sb.Append("IF NOT EXISTS ");
        sb.Append(Table);

        // 列定义 / 约束 / 仅列名三种括号内容
        if (Columns is { Count: > 0 })
        {
            sb.Append(" (").Append(string.Join(", ", Columns)).Append(')');
        }
        else if ((ColumnDefinitions != null && ColumnDefinitions.Count > 0) || (Constraints != null && Constraints.Count > 0))
        {
            var items = new List<string>();
            if (ColumnDefinitions != null)
                items.AddRange(ColumnDefinitions.Select(c => c.ToString()));
            if (Constraints != null)
                items.AddRange(Constraints.Select(c => c.ToString()));
            sb.Append(" (").Append(string.Join(", ", items)).Append(')');
        }

        // 表级选项字符串透传
        if (TableOptions is { Count: > 0 })
            sb.Append(' ').Append(string.Join(" ", TableOptions));

        // 表属性：RowMovement / AS SELECT / LIKE / INTERLEAVE IN
        if (RowMovement != null) sb.Append(' ').Append(RowMovement);
        if (Select != null)
            sb.Append(" AS ").Append(SelectParenthesis ? "(" : "").Append(Select).Append(SelectParenthesis ? ")" : "");
        if (LikeTable != null)
            sb.Append(" LIKE ").Append(SelectParenthesis ? "(" : "").Append(LikeTable).Append(SelectParenthesis ? ")" : "");
        if (InterleaveIn != null) sb.Append(InterleaveIn);

        return sb.ToString();
    }
}

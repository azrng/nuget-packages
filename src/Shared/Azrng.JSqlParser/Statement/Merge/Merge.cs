using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Schema;
using Azrng.JSqlParser.Statement.Select;

namespace Azrng.JSqlParser.Statement.Merge;

/// <summary>
/// Represents a MERGE statement in SQL.
/// </summary>
public class Merge : ASTNodeAccessImpl, IStatement
{
    public Table? Table { get; set; }

    /// <summary>MERGE 目标表的别名（MERGE INTO t alias USING ...），未指定时为 null。</summary>
    public Alias? Alias { get; set; }

    /// <summary>USING 源（表/子查询），对应 MERGE ... USING fromItem。未指定时为 null。</summary>
    public IFromItem? SourceTable { get; set; }

    public Azrng.JSqlParser.Expression.IExpression? OnCondition { get; set; }
    public System.Collections.Generic.List<MergeOperation> Operations { get; set; } = new();

    public T Accept<T, S>(IStatementVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder();
        sb.Append("MERGE INTO ").Append(Table);
        if (Alias != null) sb.Append(' ').Append(Alias);
        if (SourceTable != null) sb.Append(" USING ").Append(SourceTable);
        if (OnCondition != null) sb.Append(" ON ").Append(OnCondition);
        foreach (var op in Operations) sb.Append(' ').Append(op);
        return sb.ToString();
    }
}

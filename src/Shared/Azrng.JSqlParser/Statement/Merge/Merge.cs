using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Schema;

namespace Azrng.JSqlParser.Statement.Merge;

/// <summary>
/// Represents a MERGE statement in SQL.
/// </summary>
public class Merge : ASTNodeAccessImpl, Statement
{
    public Table? Table { get; set; }
    public Azrng.JSqlParser.Expression.Expression? OnCondition { get; set; }
    public System.Collections.Generic.List<MergeOperation> Operations { get; set; } = new();

    public T Accept<T, S>(StatementVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder();
        sb.Append("MERGE INTO ").Append(Table);
        if (OnCondition != null) sb.Append(" ON ").Append(OnCondition);
        foreach (var op in Operations) sb.Append(' ').Append(op);
        return sb.ToString();
    }
}

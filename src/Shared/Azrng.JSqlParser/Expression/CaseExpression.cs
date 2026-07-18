using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Expression;

/// <summary>
/// Represents a CASE expression in SQL.
/// </summary>
public class CaseExpression : ASTNodeAccessImpl, IExpression
{
    public IExpression? SwitchExpression { get; set; }
    public System.Collections.Generic.List<WhenClause> WhenClauses { get; set; } = new();
    public IExpression? ElseExpression { get; set; }

    public T Accept<T, S>(IExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder("CASE");
        if (SwitchExpression != null) sb.Append(' ').Append(SwitchExpression);
        foreach (var w in WhenClauses) sb.Append(' ').Append(w);
        if (ElseExpression != null) sb.Append(" ELSE ").Append(ElseExpression);
        sb.Append(" END");
        return sb.ToString();
    }
}

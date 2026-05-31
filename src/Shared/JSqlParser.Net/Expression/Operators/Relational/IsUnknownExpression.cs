using JSqlParser.Net.Parser;

namespace JSqlParser.Net.Expression.Operators.Relational;

/// <summary>
/// IS UNKNOWN expression (SQL standard).
/// </summary>
public class IsUnknownExpression : ASTNodeAccessImpl, Expression
{
    public Expression LeftExpression { get; set; } = null!;
    public bool Not { get; set; }

    public T Accept<T, S>(ExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        var not = Not ? "NOT " : "";
        return $"{LeftExpression} IS {not}UNKNOWN";
    }
}

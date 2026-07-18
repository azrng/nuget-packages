using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Expression.Operators.Relational;

/// <summary>
/// IS UNKNOWN expression (SQL standard).
/// </summary>
public class IsUnknownExpression : ASTNodeAccessImpl, IExpression
{
    public required IExpression LeftExpression { get; set; }
    public bool Not { get; set; }

    public T Accept<T, S>(IExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        var not = Not ? "NOT " : "";
        return $"{LeftExpression} IS {not}UNKNOWN";
    }
}

using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Expression;

/// <summary>
/// Represents an INTERVAL expression (e.g., INTERVAL '7' DAY).
/// </summary>
public class IntervalExpression : ASTNodeAccessImpl, Expression
{
    public string? Parameter { get; set; }
    public string? IntervalType { get; set; }
    public bool IntervalKeyword { get; set; } = true;
    public Expression? Expression { get; set; }

    public T Accept<T, S>(IExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        if (IntervalKeyword)
        {
            var param = Expression != null ? Expression.ToString() : $"'{Parameter}'";
            return $"INTERVAL {param}" + (IntervalType != null ? $" {IntervalType}" : "");
        }
        return Expression?.ToString() ?? "";
    }
}

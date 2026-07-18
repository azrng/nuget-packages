using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Expression.Operators.Relational;

/// <summary>
/// Represents an IS NULL / IS NOT NULL expression in SQL.
/// </summary>
public class IsNullExpression : ASTNodeAccessImpl, IExpression
{
    public IExpression LeftExpression { get; set; } = null!;
    public bool Not { get; set; }

    public T Accept<T, S>(IExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString() => $"{LeftExpression} {(Not ? "IS NOT NULL" : "IS NULL")}";
}

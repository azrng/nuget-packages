using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Expression.Operators.Relational;

/// <summary>
/// Represents an IN expression in SQL.
/// </summary>
public class InExpression : ASTNodeAccessImpl, IExpression
{
    public IExpression LeftExpression { get; set; } = null!;
    public IExpression RightExpression { get; set; } = null!;
    public bool Not { get; set; }

    public T Accept<T, S>(IExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString() =>
        $"{LeftExpression} {(Not ? "NOT IN" : "IN")} ({RightExpression})";
}

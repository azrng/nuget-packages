using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Expression.Operators.Relational;

/// <summary>
/// Represents an EXISTS expression in SQL.
/// </summary>
public class ExistsExpression : ASTNodeAccessImpl, IExpression
{
    public IExpression RightExpression { get; set; } = null!;
    public bool Not { get; set; }

    public T Accept<T, S>(IExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString() => $"{(Not ? "NOT EXISTS" : "EXISTS")} ({RightExpression})";
}

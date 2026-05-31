using JSqlParser.Net.Parser;

namespace JSqlParser.Net.Expression.Operators.Relational;

/// <summary>
/// Represents an EXISTS expression in SQL.
/// </summary>
public class ExistsExpression : ASTNodeAccessImpl, Expression
{
    public Expression RightExpression { get; set; } = null!;
    public bool Not { get; set; }

    public T Accept<T, S>(ExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString() => $"{(Not ? "NOT EXISTS" : "EXISTS")} ({RightExpression})";
}

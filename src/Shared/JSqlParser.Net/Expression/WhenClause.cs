using JSqlParser.Net.Parser;

namespace JSqlParser.Net.Expression;

/// <summary>
/// Represents a WHEN clause in a CASE expression.
/// </summary>
public class WhenClause : ASTNodeAccessImpl, Expression
{
    public Expression WhenExpression { get; set; } = null!;
    public Expression ThenExpression { get; set; } = null!;

    public T Accept<T, S>(ExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString() => $"WHEN {WhenExpression} THEN {ThenExpression}";
}

using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Expression;

/// <summary>
/// Represents a WHEN clause in a CASE expression.
/// </summary>
public class WhenClause : ASTNodeAccessImpl, IExpression
{
    public required IExpression WhenExpression { get; set; }
    public required IExpression ThenExpression { get; set; }

    public T Accept<T, S>(IExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString() => $"WHEN {WhenExpression} THEN {ThenExpression}";
}

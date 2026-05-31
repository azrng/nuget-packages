using JSqlParser.Net.Parser;

namespace JSqlParser.Net.Expression.Operators.Relational;

/// <summary>
/// Represents a BETWEEN expression in SQL.
/// </summary>
public class Between : ASTNodeAccessImpl, Expression
{
    public Expression LeftExpression { get; set; } = null!;
    public Expression BetweenExpressionStart { get; set; } = null!;
    public Expression BetweenExpressionEnd { get; set; } = null!;
    public bool Not { get; set; }

    public T Accept<T, S>(ExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString() =>
        $"{LeftExpression} {(Not ? "NOT BETWEEN" : "BETWEEN")} {BetweenExpressionStart} AND {BetweenExpressionEnd}";
}

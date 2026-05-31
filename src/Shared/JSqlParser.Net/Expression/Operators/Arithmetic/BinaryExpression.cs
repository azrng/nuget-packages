using JSqlParser.Net.Expression;
using JSqlParser.Net.Parser;

namespace JSqlParser.Net.Expression.Operators.Arithmetic;

/// <summary>
/// Base class for binary arithmetic expressions (e.g., a + b, a * b).
/// </summary>
public abstract class BinaryExpression : ASTNodeAccessImpl, Expression
{
    public Expression LeftExpression { get; set; } = null!;
    public Expression RightExpression { get; set; } = null!;

    public abstract T Accept<T, S>(ExpressionVisitor<T> visitor, S context);

    protected string StringExpression => $"{LeftExpression} {GetStringExpression()} {RightExpression}";

    public abstract string GetStringExpression();

    public override string ToString() => StringExpression;
}

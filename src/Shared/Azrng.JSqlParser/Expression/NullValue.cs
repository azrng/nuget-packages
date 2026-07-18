using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Expression;

/// <summary>
/// Represents a NULL value in SQL.
/// </summary>
public sealed class NullValue : ASTNodeAccessImpl, IExpression
{
    public T Accept<T, S>(IExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString() => "NULL";
}

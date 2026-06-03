using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Expression;

/// <summary>
/// Represents a NULL value in SQL.
/// </summary>
public class NullValue : ASTNodeAccessImpl, Expression
{
    public T Accept<T, S>(ExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString() => "NULL";
}

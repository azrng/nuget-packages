using JSqlParser.Net.Parser;

namespace JSqlParser.Net.Expression;

/// <summary>
/// Represents a NULL value in SQL.
/// </summary>
public class NullValue : ASTNodeAccessImpl, Expression
{
    public T Accept<T, S>(ExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString() => "NULL";
}

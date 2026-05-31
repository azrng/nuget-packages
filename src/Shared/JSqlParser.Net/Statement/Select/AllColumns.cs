using JSqlParser.Net.Parser;
using JSqlParser.Net.Expression;

namespace JSqlParser.Net.Statement.Select;

/// <summary>
/// Represents the * wildcard in SQL (e.g., SELECT *).
/// </summary>
public class AllColumns : ASTNodeAccessImpl, JSqlParser.Net.Expression.Expression
{
    public T Accept<T, S>(ExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);
    public override string ToString() => "*";
}

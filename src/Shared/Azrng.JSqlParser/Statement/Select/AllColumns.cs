using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Expression;

namespace Azrng.JSqlParser.Statement.Select;

/// <summary>
/// Represents the * wildcard in SQL (e.g., SELECT *).
/// </summary>
public class AllColumns : ASTNodeAccessImpl, Azrng.JSqlParser.Expression.Expression
{
    public T Accept<T, S>(IExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);
    public override string ToString() => "*";
}

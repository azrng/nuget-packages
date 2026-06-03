using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Expression;

/// <summary>
/// Represents a JDBC parameter (?) in SQL.
/// </summary>
public class JdbcParameter : ASTNodeAccessImpl, Expression
{
    public int Index { get; set; }
    public T Accept<T, S>(ExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);
    public override string ToString() => "?";
}

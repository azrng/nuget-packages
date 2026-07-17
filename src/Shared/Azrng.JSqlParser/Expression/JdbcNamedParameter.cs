using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Expression;

/// <summary>
/// Represents a named JDBC parameter (:name or @name) in SQL.
/// </summary>
public class JdbcNamedParameter : ASTNodeAccessImpl, Expression
{
    public string Name { get; set; } = "";
    public string Prefix { get; set; } = ":";
    public T Accept<T, S>(IExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);
    public override string ToString() => $"{Prefix}{Name}";
}

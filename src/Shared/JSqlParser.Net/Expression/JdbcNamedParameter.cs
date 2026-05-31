using JSqlParser.Net.Parser;

namespace JSqlParser.Net.Expression;

/// <summary>
/// Represents a named JDBC parameter (:name) in SQL.
/// </summary>
public class JdbcNamedParameter : ASTNodeAccessImpl, Expression
{
    public string Name { get; set; } = "";
    public T Accept<T, S>(ExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);
    public override string ToString() => $":{Name}";
}

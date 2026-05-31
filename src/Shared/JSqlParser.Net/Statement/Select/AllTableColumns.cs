using JSqlParser.Net.Parser;
using JSqlParser.Net.Schema;
using JSqlParser.Net.Expression;

namespace JSqlParser.Net.Statement.Select;

/// <summary>
/// Represents table.* wildcard in SQL (e.g., SELECT t.*).
/// </summary>
public class AllTableColumns : ASTNodeAccessImpl, JSqlParser.Net.Expression.Expression
{
    public Table Table { get; set; } = null!;

    public T Accept<T, S>(ExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);
    public override string ToString() => $"{Table}.*";
}

using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Schema;
using Azrng.JSqlParser.Expression;

namespace Azrng.JSqlParser.Statement.Select;

/// <summary>
/// Represents table.* wildcard in SQL (e.g., SELECT t.*).
/// </summary>
public class AllTableColumns : ASTNodeAccessImpl, Azrng.JSqlParser.Expression.Expression
{
    public Table Table { get; set; } = null!;

    public T Accept<T, S>(ExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);
    public override string ToString() => $"{Table}.*";
}

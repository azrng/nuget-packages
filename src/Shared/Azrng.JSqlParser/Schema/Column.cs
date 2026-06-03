using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Expression;

namespace Azrng.JSqlParser.Schema;

/// <summary>
/// Represents a column reference in SQL.
/// </summary>
public class Column : ASTNodeAccessImpl, Azrng.JSqlParser.Expression.Expression
{
    public Table? Table { get; set; }
    public string ColumnName { get; set; } = "";

    public T Accept<T, S>(ExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public string GetFullyQualifiedName()
    {
        if (Table != null)
        {
            var tableName = Table.GetFullyQualifiedName();
            return $"{tableName}.{ColumnName}";
        }
        return ColumnName;
    }

    public override string ToString() => GetFullyQualifiedName();
}

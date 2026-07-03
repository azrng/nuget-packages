using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Statement;

namespace Azrng.JSqlParser.Schema;

/// <summary>
/// Represents a column reference in SQL.
/// </summary>
public class Column : ASTNodeAccessImpl, Azrng.JSqlParser.Expression.Expression
{
    public Table? Table { get; set; }
    public string ColumnName { get; set; } = "";

    /// <summary>RETURNING 子句中 OLD/NEW 引用类型（PostgreSQL 18），非 RETURNING 场景为 null。</summary>
    public ReturningReferenceType? ReturningReferenceType { get; set; }

    /// <summary>RETURNING 子句中的限定符原文（如 "old" / "new" 或自定义别名），非 RETURNING 场景为 null。</summary>
    public string? ReturningQualifier { get; set; }

    public T Accept<T, S>(ExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public string GetFullyQualifiedName()
    {
        if (ReturningQualifier != null)
        {
            return $"{ReturningQualifier}.{ColumnName}";
        }
        if (Table != null)
        {
            var tableName = Table.GetFullyQualifiedName();
            return $"{tableName}.{ColumnName}";
        }
        return ColumnName;
    }

    public override string ToString() => GetFullyQualifiedName();
}

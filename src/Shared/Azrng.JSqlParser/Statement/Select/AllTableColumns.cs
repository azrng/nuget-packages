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

    /// <summary>RETURNING 子句中 OLD/NEW 引用类型（PostgreSQL 18），非 RETURNING 场景为 null。</summary>
    public ReturningReferenceType? ReturningReferenceType { get; set; }

    /// <summary>RETURNING 子句中的限定符原文，非 RETURNING 场景为 null。</summary>
    public string? ReturningQualifier { get; set; }

    public T Accept<T, S>(IExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        if (ReturningQualifier != null) return $"{ReturningQualifier}.*";
        return $"{Table}.*";
    }
}

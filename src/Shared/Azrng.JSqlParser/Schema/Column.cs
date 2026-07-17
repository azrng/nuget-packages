using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Statement;

namespace Azrng.JSqlParser.Schema;

/// <summary>
/// Oracle 老式外连接语法（+）的连接方向。
/// 对齐上游 SupportsOldOracleJoinSyntax，仅保留 Column 实际使用的部分。
/// </summary>
public enum OracleJoinSyntax
{
    /// <summary>无外连接标记。</summary>
    None,

    /// <summary>右侧外连接（列后跟 (+)）。</summary>
    Right,

    /// <summary>左侧外连接。</summary>
    Left
}

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

    /// <summary>
    /// Oracle 老式外连接语法标记：列名后跟(+) 表示该侧为外连接的可选侧。
    /// 默认 <see cref="OracleJoinSyntax.None"/>。对齐上游 commit 834afe18。
    /// </summary>
    public OracleJoinSyntax OldOracleJoinSyntax { get; set; } = OracleJoinSyntax.None;

    /// <summary>列注释文本（COMMENT 'x' 或 /* x */），对齐上游 commentText。未指定时为 null。</summary>
    public string? CommentText { get; set; }

    /// <summary>列后的数组构造/下标（如 ARRAY[...] 或 col[i]），对齐上游 arrayConstructor。未指定时为 null。</summary>
    public ArrayConstructor? ArrayConstructor { get; set; }

    public T Accept<T, S>(ExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public string GetFullyQualifiedName()
    {
        string name;
        if (ReturningQualifier != null)
        {
            name = $"{ReturningQualifier}.{ColumnName}";
        }
        else if (Table != null)
        {
            var tableName = Table.GetFullyQualifiedName();
            name = $"{tableName}.{ColumnName}";
        }
        else
        {
            name = ColumnName;
        }
        // 数组构造/下标追加（对齐上游 Column.getFullyQualifiedName 末尾）
        if (ArrayConstructor != null) name += ArrayConstructor;
        return name;
    }

    public override string ToString()
    {
        var baseName = GetFullyQualifiedName();
        // Oracle 老式外连接 (+) 后缀
        if (OldOracleJoinSyntax != OracleJoinSyntax.None)
        {
            baseName += "(+)";
        }
        // 列注释（对齐上游 toString 的 COMMENT 输出）
        if (CommentText != null) baseName += $" COMMENT {CommentText}";
        return baseName;
    }
}

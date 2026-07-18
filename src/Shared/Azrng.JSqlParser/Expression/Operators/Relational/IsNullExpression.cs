using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Expression.Operators.Relational;

/// <summary>
/// 表示 IS NULL / IS NOT NULL 表达式，或 PostgreSQL 简写形式 ISNULL / NOTNULL。
/// 对齐上游 <c>IsNullExpression</c>：<c>useIsNull</c> 控制 <c>x ISNULL</c> 简写、
/// <c>useNotNull</c> 控制 <c>x NOTNULL</c> 简写。
/// </summary>
public class IsNullExpression : ASTNodeAccessImpl, IExpression
{
    public required IExpression LeftExpression { get; set; }

    /// <summary>是否取反（IS NOT NULL）。在 PG 简写模式下不影响输出（NOTNULL 自带否定语义）。</summary>
    public bool Not { get; set; }

    /// <summary>
    /// PostgreSQL 简写形式 <c>x ISNULL</c>。为 true 时 ToString 输出 <c>x ISNULL</c>（而不是 IS NULL）。
    /// 对齐上游 <c>useIsNull</c>。
    /// </summary>
    public bool UseIsNull { get; set; }

    /// <summary>
    /// PostgreSQL 简写形式 <c>x NOTNULL</c>。为 true 时 ToString 输出 <c>x NOTNULL</c>。
    /// 对齐上游 <c>useNotNull</c>。优先级高于 <see cref="UseIsNull"/> 与 <see cref="Not"/>。
    /// </summary>
    public bool UseNotNull { get; set; }

    public T Accept<T, S>(IExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        // 对齐上游 IsNullExpression.java:74-82 三分支输出顺序
        if (UseNotNull) return $"{LeftExpression} NOTNULL";
        if (UseIsNull) return $"{LeftExpression}{(Not ? " NOT" : "")} ISNULL";
        return $"{LeftExpression} IS {(Not ? "NOT " : "")}NULL";
    }
}

using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Expression;

/// <summary>
/// 行构造器表达式：<c>ROW(1, 2, 3)</c> 或 <c>(1, 2, 3)</c>（PostgreSQL/MySQL/Oracle）。
/// 与上游 RowConstructor 对齐。
/// <para>
/// 用于 IN 子查询的多列比较：<c>WHERE (a, b) IN (SELECT x, y FROM t)</c>，
/// 或 VALUES 子句、PostgreSQL 复合类型构造。
/// </para>
/// </summary>
public class RowConstructor : ASTNodeAccessImpl, Expression
{
    /// <summary>构造器名，通常是 "ROW"；括号形式时为 null。</summary>
    public string? Name { get; set; }

    /// <summary>行中的表达式列表。</summary>
    public ExpressionList? Expressions { get; set; }

    public RowConstructor() { }

    public RowConstructor(string? name, ExpressionList? expressions)
    {
        Name = name;
        Expressions = expressions;
    }

    public T Accept<T, S>(IExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        var prefix = Name != null ? Name : "";
        return $"{prefix}({Expressions})";
    }
}

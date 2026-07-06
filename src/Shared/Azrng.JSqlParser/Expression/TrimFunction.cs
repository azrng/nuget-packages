using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Expression;

/// <summary>
/// TRIM 函数表达式：<c>TRIM([LEADING|TRAILING|BOTH] [chars] [FROM] str)</c>。
/// 与上游 TrimFunction 对齐。
/// <para>
/// 示例：
/// <list type="bullet">
/// <item><c>TRIM('  hello  ')</c> — 简单形式</item>
/// <item><c>TRIM(LEADING ' ' FROM '  hello')</c> — 带规范和 FROM</item>
/// <item><c>TRIM(BOTH ',' FROM ',,hello,,')</c> — PostgreSQL/标准 SQL</item>
/// </list>
/// </para>
/// </summary>
public class TrimFunction : ASTNodeAccessImpl, Expression
{
    public TrimSpecification? TrimSpecification { get; set; }

    /// <summary>要去除的字符表达式（FROM/逗号之前的部分），未指定时为 null。</summary>
    public Expression? Expression { get; set; }

    /// <summary>被处理的目标表达式（FROM/逗号之后的部分），未指定时为 null。</summary>
    public Expression? FromExpression { get; set; }

    /// <summary>true 表示使用 FROM 关键字连接，false 表示使用逗号。</summary>
    public bool UsingFromKeyword { get; set; }

    public T Accept<T, S>(ExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder();
        sb.Append("Trim(");
        if (TrimSpecification.HasValue)
        {
            sb.Append(' ').Append(TrimSpecification.Value.ToString().ToUpperInvariant());
        }
        if (Expression != null) sb.Append(' ').Append(Expression);
        if (FromExpression != null)
        {
            sb.Append(UsingFromKeyword ? " FROM " : ", ").Append(FromExpression);
        }
        sb.Append(" )");
        return sb.ToString();
    }
}

/// <summary>TRIM 规范：LEADING/TRAILING/BOTH。</summary>
public enum TrimSpecification
{
    Leading,
    Trailing,
    Both
}

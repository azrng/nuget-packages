using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Expression;

/// <summary>
/// 时区表达式：<c>expr AT TIME ZONE zone</c>（PostgreSQL/SQL Server）。
/// 与上游 TimezoneExpression 对齐。
/// <para>示例：<c>timestamp '2024-01-01' AT TIME ZONE 'UTC'</c></para>
/// </summary>
public class TimezoneExpression : ASTNodeAccessImpl, IExpression
{
    public IExpression? LeftExpression { get; set; }

    /// <summary>时区表达式（字符串字面量或列引用）。</summary>
    public IExpression? TimeZoneExpression { get; set; }

    public T Accept<T, S>(IExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString() => $"{LeftExpression} AT TIME ZONE {TimeZoneExpression}";
}

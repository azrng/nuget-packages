using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Expression;

/// <summary>
/// 日期/时间单位字面量表达式（YEAR/MONTH/DAY/HOUR/MINUTE/SECOND 等）。
/// 对应上游 commit 4fdfa785 / DateUnitExpression。
/// <para>
/// 出现场景：
/// <list type="bullet">
/// <item><c>TIMESTAMPDIFF(YEAR, start, end)</c> 第一个参数</item>
/// <item><c>EXTRACT(YEAR FROM date)</c> 字段位置（已由 ExtractExpression.Name 字符串处理）</item>
/// <item>独立表达式位置（按需保留）</item>
/// </list>
/// </para>
/// </summary>
public class DateUnitExpression : ASTNodeAccessImpl, IExpression
{
    public DateUnit Unit { get; set; }

    public DateUnitExpression() { }

    public DateUnitExpression(DateUnit unit) => Unit = unit;

    /// <summary>从字符串构造，不区分大小写。</summary>
    public DateUnitExpression(string unitStr)
    {
        Unit = Enum.Parse<DateUnit>(unitStr, ignoreCase: true);
    }

    public T Accept<T, S>(IExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString() => Unit.ToString().ToUpperInvariant();
}

/// <summary>
/// SQL 日期/时间单位枚举。覆盖主流数据库（MySQL/PG/SQL Server/Oracle）的单位。
/// </summary>
public enum DateUnit
{
    Century,
    Decade,
    Year,
    Quarter,
    Month,
    Week,
    Day,
    Hour,
    Minute,
    Second,
    Millisecond,
    Microsecond,
    Nanosecond
}

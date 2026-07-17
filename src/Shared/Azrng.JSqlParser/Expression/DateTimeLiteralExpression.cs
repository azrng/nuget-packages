using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Expression;

/// <summary>
/// 日期时间类型前缀字面量（<c>DATE '2024-01-01'</c>、<c>TIMESTAMP '2024-01-01 10:00:00'</c> 等），
/// 对齐上游 DateTimeLiteralExpression。
///
/// 注意：与 <see cref="DateValue"/>/<see cref="TimeValue"/>/<see cref="TimestampValue"/> 设计不同——
/// 后者将值解析为强类型 <c>System.DateTime</c>，本类保留字符串原值与类型枚举，对齐上游序列化行为。
/// </summary>
public class DateTimeLiteralExpression : ASTNodeAccessImpl, Expression
{
    public string Value { get; set; } = string.Empty;

    public DateTimeType? Type { get; set; }

    public T Accept<T, S>(ExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString() => Type is { } t ? $"{t.ToString().ToUpperInvariant()} {Value}" : Value;
}

/// <summary>日期时间字面量类型枚举，对齐上游 DateTimeLiteralExpression.DateTime。</summary>
public enum DateTimeType
{
    Date,
    Datetime,
    Time,
    Timestamp,
    Timestamptz
}

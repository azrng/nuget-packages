using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Expression.Operators.Arithmetic;
using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Expression.Operators.Relational;

/// <summary>
/// PostgreSQL 正则匹配运算符（<c>~</c> / <c>~*</c> / <c>!~</c> / <c>!~*</c>），
/// 对齐上游 JSqlParser 的 RegExpMatchOperator。
/// </summary>
/// <remarks>
/// 通过 <see cref="OperatorType"/> 枚举区分四种变体（对齐上游 RegExpMatchOperatorType）。
/// <b>注意</b>：关键字形式的 <c>REGEXP</c>/<c>RLIKE</c>/<c>REGEXP_LIKE</c> 在上游归
/// <see cref="LikeExpression"/>（KeyWord.REGEXP 等），不在本类。
/// </remarks>
public class RegExpMatchOperator : BinaryExpression
{
    /// <summary>构造时必须指定运算符类型，对齐上游 requireNonNull。</summary>
    public RegExpMatchOperator(RegExpMatchOperatorType operatorType)
    {
        OperatorType = operatorType;
    }

    /// <summary>运算符类型（四态之一），无默认值。</summary>
    public RegExpMatchOperatorType OperatorType { get; set; }

    public override T Accept<T, S>(IExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    /// <summary>运算符符号：按 <see cref="OperatorType"/> 映射到 ~ / ~* / !~ / !~*。</summary>
    public override string OperatorSymbol => OperatorType switch
    {
        RegExpMatchOperatorType.MatchCaseSensitive => "~",
        RegExpMatchOperatorType.MatchCaseInsensitive => "~*",
        RegExpMatchOperatorType.NotMatchCaseSensitive => "!~",
        RegExpMatchOperatorType.NotMatchCaseInsensitive => "!~*",
        _ => throw new InvalidOperationException($"未知的 RegExpMatchOperatorType: {OperatorType}")
    };
}

namespace Azrng.JSqlParser.Expression.Operators.Relational;

/// <summary>
/// PostgreSQL 正则匹配运算符类型，对齐上游 JSqlParser 的 RegExpMatchOperatorType。
/// </summary>
/// <remarks>
/// 区分大小写敏感性与否定形式，对应 PostgreSQL 的四种符号运算符：
/// <c>~</c>、<c>~*</c>、<c>!~</c>、<c>!~*</c>。
/// 与关键字形式（<c>REGEXP</c>/<c>RLIKE</c> 等，归 <see cref="LikeExpression"/>）区分开。
/// </remarks>
public enum RegExpMatchOperatorType
{
    /// <summary>大小写敏感匹配：~</summary>
    MatchCaseSensitive,

    /// <summary>大小写不敏感匹配：~*</summary>
    MatchCaseInsensitive,

    /// <summary>大小写敏感否定匹配：!~</summary>
    NotMatchCaseSensitive,

    /// <summary>大小写不敏感否定匹配：!~*</summary>
    NotMatchCaseInsensitive
}

using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Expression.Operators.Arithmetic;

namespace Azrng.JSqlParser.Expression.Operators.Relational;

/// <summary>
/// 关键字形式的模式匹配表达式，对齐上游 JSqlParser 的 LikeExpression。
/// </summary>
/// <remarks>
/// 统一承载 LIKE / ILIKE / RLIKE / REGEXP / REGEXP_LIKE / SIMILAR TO / MATCH_* 等所有关键字形式的匹配，
/// 通过 <see cref="LikeKeyWord"/> 枚举区分具体关键字（对齐上游 KeyWord 枚举，PascalCase 命名）。
/// <b>注意</b>：PG 符号形式的正则匹配（<c>~</c>/<c>~*</c>/<c>!~</c>/<c>!~*</c>）走 <see cref="RegExpMatchOperator"/>，不在本类。
/// SIMILAR TO 在上游也归本类（KeyWord.SIMILAR_TO），C# 此前是独立的 SimilarToExpression，现已合并。
/// </remarks>
public class LikeExpression : BinaryExpression
{
    /// <summary>是否带 NOT 前缀（NOT LIKE / NOT REGEXP 等）。</summary>
    public bool Not { get; set; }

    /// <summary>
    /// 关键字类型，默认 LIKE。对齐上游 likeKeyWord 字段。
    /// 决定 OperatorSymbol / ToString 输出哪种关键字。
    /// </summary>
    public KeyWord LikeKeyWord { get; set; } = KeyWord.Like;

    /// <summary>
    /// MySQL 大小写敏感 LIKE 标记。为 true 时 ToString 在右表达式前输出 <c>BINARY </c>。
    /// 对齐上游 <c>useBinary</c>。
    /// </summary>
    public bool UseBinary { get; set; }

    /// <summary>ESCAPE 转义表达式（如 LIKE 'a%' ESCAPE '\'），未指定时为 null。</summary>
    public IExpression? Escape { get; set; }

    public override T Accept<T, S>(IExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    /// <summary>
    /// 运算符符号文本：按 <see cref="LikeKeyWord"/> 输出对应的 SQL 关键字。
    /// SQL 关键字保留上游原始写法（含下划线/空格，如 REGEXP_LIKE / MATCH_ANY / SIMILAR TO），
    /// 不依赖 .ToUpperInvariant() 把枚举名转大写（会丢失下划线）。
    /// </summary>
    public override string OperatorSymbol => LikeKeyWord switch
    {
        KeyWord.Like => "LIKE",
        KeyWord.Ilike => "ILIKE",
        KeyWord.Rlike => "RLIKE",
        KeyWord.RegexpLike => "REGEXP_LIKE",
        KeyWord.Regexp => "REGEXP",
        KeyWord.SimilarTo => "SIMILAR TO",
        KeyWord.MatchAny => "MATCH_ANY",
        KeyWord.MatchAll => "MATCH_ALL",
        KeyWord.MatchPhrase => "MATCH_PHRASE",
        KeyWord.MatchPhrasePrefix => "MATCH_PHRASE_PREFIX",
        KeyWord.MatchRegexp => "MATCH_REGEXP",
        _ => LikeKeyWord.ToString().ToUpperInvariant()
    };

    public override string ToString()
    {
        // 对齐上游 LikeExpression.java:50-58：
        //   左 [NOT ] KeyWord [BINARY ] 右 [ESCAPE ...]
        // BINARY 输出在 KeyWord 之后、右表达式之前（MySQL 大小写敏感 LIKE）。
        var sb = new System.Text.StringBuilder();
        sb.Append(LeftExpression).Append(' ');
        if (Not) sb.Append("NOT ");
        sb.Append(OperatorSymbol).Append(' ');
        if (UseBinary) sb.Append("BINARY ");
        sb.Append(RightExpression);
        if (Escape != null) sb.Append(" ESCAPE ").Append(Escape);
        return sb.ToString();
    }

    /// <summary>LIKE 系关键字枚举，对齐上游 LikeExpression.KeyWord。</summary>
    public enum KeyWord
    {
        /// <summary>LIKE</summary>
        Like,

        /// <summary>ILIKE（大小写不敏感 LIKE，PostgreSQL）</summary>
        Ilike,

        /// <summary>RLIKE（MySQL 正则匹配关键字）</summary>
        Rlike,

        /// <summary>REGEXP_LIKE（Oracle 正则匹配）</summary>
        RegexpLike,

        /// <summary>REGEXP</summary>
        Regexp,

        /// <summary>SIMILAR TO（SQL 标准相似匹配）</summary>
        SimilarTo,

        /// <summary>MATCH_ANY</summary>
        MatchAny,

        /// <summary>MATCH_ALL</summary>
        MatchAll,

        /// <summary>MATCH_PHRASE（ClickHouse/ES 全文短语）</summary>
        MatchPhrase,

        /// <summary>MATCH_PHRASE_PREFIX</summary>
        MatchPhrasePrefix,

        /// <summary>MATCH_REGEXP</summary>
        MatchRegexp
    }
}

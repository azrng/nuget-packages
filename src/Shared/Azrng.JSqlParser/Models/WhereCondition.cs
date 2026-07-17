using JExpression = Azrng.JSqlParser.Expression.Expression;

namespace Azrng.JSqlParser.Models;

/// <summary>
/// WHERE 树中拍平后的一个叶子条件。
/// </summary>
/// <remarks>
/// 中性 DTO，由 <c>GetWhereConditions</c> 遍历 AND/OR 链生成。
/// 仅覆盖 And/Or/Binary/In/Between 五类运算符（与下游 LocalSqlParser 原逻辑一致），
/// 其他运算符（Exists/IsNull/Like 等）不在此结果中；需要时业务方用 <c>Descendants&lt;T&gt;</c>。
/// 不含列归属反查、参数收集装配等产品逻辑——业务方按字段自行映射到业务 DTO。
/// </remarks>
public sealed record WhereCondition
{
    /// <summary>连接符：<c>"AND"</c>/<c>"OR"</c>，链中首个条件为空字符串。</summary>
    public string LinkType { get; init; } = string.Empty;

    /// <summary>左侧表达式（通常是列引用）。</summary>
    public JExpression LeftExpression { get; init; } = null!;

    /// <summary>右侧表达式（通常是值/参数/列引用）。</summary>
    public JExpression RightExpression { get; init; } = null!;

    /// <summary>运算符文本：<c>=</c>、<c>&gt;</c>、<c>IN</c>、<c>NOT IN</c>、<c>BETWEEN</c> 等。</summary>
    public string Operator { get; init; } = string.Empty;

    /// <summary>该条件的原始 SQL 文本（round-trip）。</summary>
    public string SqlInfo { get; init; } = string.Empty;
}

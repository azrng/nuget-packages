using Azrng.JSqlParser.Expression;

namespace Azrng.JSqlParser.Models;

/// <summary>
/// WHERE 树中拍平后的一个叶子条件。
/// </summary>
/// <remarks>
/// 中性 DTO，由 <c>GetWhereConditions</c> 遍历 AND/OR 链生成。
/// 二元运算符（=、&gt;、&lt;、LIKE、IN、BETWEEN 等所有继承 <c>BinaryExpression</c> 的类型）填充
/// <see cref="LeftExpression"/>/<see cref="RightExpression"/>；单目运算符（IS NULL/EXISTS 等经兜底提取）
/// 仅填充 <see cref="LeftExpression"/>，<see cref="RightExpression"/> 为 null。
/// 不含列归属反查、参数收集装配等产品逻辑——业务方按字段自行映射到业务 DTO。
/// </remarks>
public sealed record WhereCondition
{
    /// <summary>连接符：<c>"AND"</c>/<c>"OR"</c>，链中首个条件为空字符串。</summary>
    public string LinkType { get; init; } = string.Empty;

    /// <summary>左侧表达式（通常是列引用）。</summary>
    public IExpression LeftExpression { get; init; } = null!;

    /// <summary>
    /// 右侧表达式（通常是值/参数/列引用）。
    /// 单目运算符（IS NULL/EXISTS 等，经兜底提取）时为 null。
    /// </summary>
    public IExpression? RightExpression { get; init; }

    /// <summary>运算符文本：<c>=</c>、<c>&gt;</c>、<c>IN</c>、<c>NOT IN</c>、<c>BETWEEN</c> 等。</summary>
    public string Operator { get; init; } = string.Empty;

    /// <summary>该条件的原始 SQL 文本（round-trip）。</summary>
    public string SqlInfo { get; init; } = string.Empty;
}

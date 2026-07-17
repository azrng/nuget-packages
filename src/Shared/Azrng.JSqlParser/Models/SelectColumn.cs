using JExpression = Azrng.JSqlParser.Expression.Expression;

namespace Azrng.JSqlParser.Models;

/// <summary>
/// SELECT 列表中的一个结构化项。
/// </summary>
/// <remarks>
/// 中性 DTO，对应 SELECT 项的四种形态（见 <see cref="SelectColumnKind"/>）。
/// 不含虚拟列必填别名校验、来源列推断等产品规则——业务方按 <see cref="Kind"/> 自行判定。
/// </remarks>
public sealed record SelectColumn
{
    /// <summary>该项的分类。</summary>
    public SelectColumnKind Kind { get; init; }

    /// <summary>
    /// 表别名：<see cref="SelectColumnKind.AllTable"/> 时为限定表名/别名；
    /// <see cref="SelectColumnKind.Column"/> 时为列显式归属（如 <c>u.name</c> 的 <c>u</c>），无显式归属时为 null；
    /// 其余 Kind 为 null。
    /// </summary>
    public string? TableAlias { get; init; }

    /// <summary>列名：<see cref="SelectColumnKind.Column"/> 时的列名；其余 Kind 为 null。</summary>
    public string? ColumnName { get; init; }

    /// <summary>SELECT 项的 AS 别名（如 <c>name AS n</c> 的 <c>n</c>），未指定时为 null。</summary>
    public string? Alias { get; init; }

    /// <summary>
    /// 原始表达式：<see cref="SelectColumnKind.Column"/> 与 <see cref="SelectColumnKind.Expression"/> 时保留，
    /// 供业务方深挖（如推断 CAST 类型、收集子列）；<see cref="SelectColumnKind.All"/> 与
    /// <see cref="SelectColumnKind.AllTable"/> 时为 null。
    /// </summary>
    public JExpression? Expression { get; init; }
}

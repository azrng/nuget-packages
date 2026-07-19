namespace Azrng.JSqlParser.Statement.Select;

/// <summary>
/// GROUP BY 子句的单项。仅承载表达式本身，不结构化 ASC/DESC 方向字段。
/// </summary>
/// <remarks>
/// MySQL <c>GROUP BY a ASC, b DESC</c> 是 MySQL 8.0 已弃用且无实际排序效果的语法，
/// 业务应使用 ORDER BY 控制结果顺序。Azrng 选择「能解析老 SQL、不报错」的兼容策略，
/// 但不通过 <c>IsAsc/IsDesc</c> 等结构化字段暴露方向语义，避免下游依赖已弃用语义。
/// 方向部分通过 <see cref="OriginalText"/> 整体透传，保 round-trip 不丢失。
/// </remarks>
public class GroupByColumnReference
{
    /// <summary>分组表达式。</summary>
    public required Expression.IExpression Expression { get; set; }

    /// <summary>
    /// 透传的原始文本（含可选 ASC/DESC），用于 ToString 还原 SQL。
    /// 为 null 时 ToString 仅输出 <see cref="Expression"/>。
    /// </summary>
    public string? OriginalText { get; set; }

    public override string ToString() => OriginalText ?? Expression.ToString() ?? "";
}

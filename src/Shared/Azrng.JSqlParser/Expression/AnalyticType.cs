namespace Azrng.JSqlParser.Expression;

/// <summary>
/// 分析/窗口函数的类型，对齐上游 AnalyticType。
/// 决定 AnalyticExpression.ToString() 的输出形态。
/// </summary>
public enum AnalyticType
{
    /// <summary>标准窗口函数：expr OVER (...)</summary>
    Over,

    /// <summary>仅 WITHIN GROUP（无 OVER）：expr WITHIN GROUP (ORDER BY ...)</summary>
    WithinGroup,

    /// <summary>WITHIN GROUP + OVER：expr WITHIN GROUP (ORDER BY ...) OVER (PARTITION BY ...)</summary>
    WithinGroupOver,

    /// <summary>仅 FILTER（无 OVER 无 WITHIN GROUP）：expr FILTER (WHERE ...)</summary>
    FilterOnly,
}

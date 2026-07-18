namespace Azrng.JSqlParser.Statement.Select;

/// <summary>
/// GROUP BY 子句的单项：表达式 + 可选 ASC/DESC 方向。
/// 对齐上游 GroupByColumnReference（expression + usingOrders/ascDescMarkers）。
/// 用于支持 MySQL/SQL Server <c>GROUP BY a ASC, b DESC</c> 形式。
/// </summary>
public class GroupByColumnReference
{
    /// <summary>分组表达式。</summary>
    public required Expression.IExpression Expression { get; set; }

    /// <summary>是否为 ASC。未指定方向时为 null。</summary>
    public bool? IsAsc { get; set; }

    /// <summary>是否为 DESC。未指定方向时为 null。</summary>
    public bool? IsDesc => !IsAsc.HasValue ? null : !IsAsc.Value;

    public override string ToString()
    {
        return IsAsc switch
        {
            true => $"{Expression} ASC",
            false => $"{Expression} DESC",
            _ => Expression.ToString() ?? ""
        };
    }
}

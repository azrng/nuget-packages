using System.Text;
using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Schema;

namespace Azrng.JSqlParser.Statement.Select;

/// <summary>
/// FROM 子句 PIVOT，对齐上游 Pivot。
/// 形式：<c>PIVOT (agg_func FOR for_col IN (val1, val2)) [AS alias]</c>。
/// 简化版：functionItems 用单函数，forColumns 用列列表，inItems 用表达式列表。
/// </summary>
public class Pivot
{
    /// <summary>聚合函数项（如 SUM(amount)）。</summary>
    public required Function Function { get; set; }

    /// <summary>是否 PIVOT XML 变体（Oracle）：PIVOT XML (...) 输出 XML 格式结果。</summary>
    public bool IsXml { get; set; }

    /// <summary>FOR 列（如 FOR product_type）。</summary>
    public List<Column> ForColumns { get; set; } = new();

    /// <summary>IN 值列表（如 IN ('A', 'B')）。</summary>
    public List<Expression.IExpression> InItems { get; set; } = new();

    /// <summary>别名，可选。</summary>
    public Alias? Alias { get; set; }

    public override string ToString()
    {
        var sb = new StringBuilder("PIVOT");
        if (IsXml) sb.Append(" XML");
        sb.Append(" (");
        sb.Append(Function);
        sb.Append(" FOR ");
        sb.Append(string.Join(", ", ForColumns));
        sb.Append(" IN (");
        sb.Append(string.Join(", ", InItems));
        sb.Append("))");
        if (Alias != null) sb.Append(' ').Append(Alias);
        return sb.ToString();
    }
}

using System.Text;
using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Schema;

namespace Azrng.JSqlParser.Statement.Select;

/// <summary>
/// FROM 子句 PIVOT，对齐上游 Pivot。
/// 形式：<c>PIVOT (agg_func1, agg_func2 FOR for_col IN (val1, val2)) [AS alias]</c>。
/// </summary>
/// <remarks>
/// 聚合函数支持多个（对齐上游 <c>functionItems: List&lt;SelectItem&lt;Function&gt;&gt;</c>）。
/// 为兼顾旧 API，保留 <see cref="Function"/> 单值属性（取 <see cref="Functions"/> 首项）。
/// </remarks>
public class Pivot
{
    private List<Function> _functions = new();

    /// <summary>
    /// 聚合函数列表（对齐上游 functionItems）。多聚合 PIVOT (SUM(a), COUNT(b)) 全部保留。
    /// </summary>
    public List<Function> Functions
    {
        get => _functions;
        set => _functions = value ?? new();
    }

    /// <summary>
    /// 单聚合快捷访问：取 <see cref="Functions"/> 首项。
    /// 设置时清空 Functions 并 Add 该值（与 OnExpression/OnExpressions 双 API 设计一致）。
    /// </summary>
    public Function Function
    {
        get => _functions.Count > 0 ? _functions[0] : throw new InvalidOperationException(
            $"{nameof(Pivot)} 当前 {nameof(Functions)} 为空，无法取首项。请先 Add 或直接访问 {nameof(Functions)}。");
        set
        {
            _functions.Clear();
            if (value != null) _functions.Add(value);
        }
    }

    /// <summary>是否 PIVOT XML 变体（Oracle）：PIVOT XML (...) 输出 XML 格式结果。</summary>
    public bool IsXml { get; set; }

    /// <summary>FOR 列（如 FOR product_type）。</summary>
    public List<Column> ForColumns { get; set; } = new();

    /// <summary>IN 值列表（如 IN ('A', 'B')）。</summary>
    public List<IExpression> InItems { get; set; } = new();

    /// <summary>别名，可选。</summary>
    public Alias? Alias { get; set; }

    public override string ToString()
    {
        var sb = new StringBuilder("PIVOT");
        if (IsXml) sb.Append(" XML");
        sb.Append(" (");
        sb.Append(string.Join(", ", Functions));
        sb.Append(" FOR ");
        sb.Append(string.Join(", ", ForColumns));
        sb.Append(" IN (");
        sb.Append(string.Join(", ", InItems));
        sb.Append("))");
        if (Alias != null) sb.Append(' ').Append(Alias);
        return sb.ToString();
    }
}

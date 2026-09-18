using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Statement.Select;

namespace Azrng.JSqlParser.Expression;

/// <summary>
/// ClickHouse COLUMNS(...) 变换器（#2631/#2635）：
/// COLUMNS(expr) [APPLY f] [EXCEPT (cols)] [REPLACE item AS col, ...]，变换器可组合。
/// </summary>
public class ColumnsExpression : ASTNodeAccessImpl, IExpression
{
    /// <summary>COLUMNS(...) 内的匹配表达式（正则字符串或 *）。</summary>
    public required IExpression Pattern { get; set; }

    /// <summary>APPLY f 的函数表达式，未指定时为 null。</summary>
    public IExpression? Apply { get; set; }

    /// <summary>EXCEPT 排除列名列表，未指定时为 null。</summary>
    public List<string>? Except { get; set; }

    /// <summary>REPLACE 替换项列表（selectItem 形式，如 x AS c），未指定时为 null。</summary>
    public List<SelectItem>? Replace { get; set; }

    public T Accept<T, S>(IExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder("COLUMNS(").Append(Pattern).Append(')');
        if (Apply != null) sb.Append(" APPLY ").Append(Apply);
        if (Except is { Count: > 0 }) sb.Append(" EXCEPT (").Append(string.Join(", ", Except)).Append(')');
        if (Replace is { Count: > 0 }) sb.Append(" REPLACE ").Append(string.Join(", ", Replace));
        return sb.ToString();
    }
}

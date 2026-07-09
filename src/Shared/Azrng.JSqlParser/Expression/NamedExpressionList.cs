using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Expression;

/// <summary>
/// 命名参数表达式列表，对齐上游 NamedExpressionList。
/// 用于 SUBSTRING(x FROM 1 FOR 3)、POSITION(a IN b)、OVERLAY(x PLACING y FROM 1 FOR 2) 等 SQL 标准命名参数语法。
/// Names[i] 为第 i 个表达式的前缀关键字（FROM/IN/PLACING/FOR），首元素前缀为空字符串。
/// </summary>
public class NamedExpressionList : ASTNodeAccessImpl
{
    public List<Expression> Expressions { get; set; } = new();

    /// <summary>每段表达式的前缀关键字（""/FROM/IN/PLACING/FOR），与 Expressions 等长。</summary>
    public List<string> Names { get; set; } = new();

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < Expressions.Count; i++)
        {
            if (i > 0) sb.Append(' ');
            var name = i < Names.Count ? Names[i] : "";
            if (!string.IsNullOrEmpty(name)) sb.Append(name).Append(' ');
            sb.Append(Expressions[i]);
        }
        return sb.ToString();
    }
}

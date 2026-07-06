using System.Text;

namespace Azrng.JSqlParser.Expression;

/// <summary>
/// JSON 函数内的单个输入/元素表达式项：expression [FORMAT JSON [ENCODING x]]。
/// 与上游 JsonFunctionExpression 对齐。
/// </summary>
public class JsonFunctionExpression
{
    public Expression? Expression { get; set; }

    public bool UsingFormatJson { get; set; }

    public string? Encoding { get; set; }

    public JsonFunctionExpression() { }

    public JsonFunctionExpression(Expression? expression)
    {
        Expression = expression;
    }

    public void AppendTo(StringBuilder sb)
    {
        sb.Append(Expression);
        if (UsingFormatJson)
        {
            sb.Append(" FORMAT JSON");
            if (Encoding != null)
            {
                sb.Append(" ENCODING ").Append(Encoding);
            }
        }
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        AppendTo(sb);
        return sb.ToString();
    }
}

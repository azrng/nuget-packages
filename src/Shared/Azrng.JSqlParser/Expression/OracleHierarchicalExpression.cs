using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Expression;

/// <summary>
/// Oracle 层次查询子句（START WITH ... CONNECT BY ...），对齐上游 OracleHierarchicalExpression。
/// 语法：START WITH expr CONNECT BY [NOCYCLE] expr，或 CONNECT BY [NOCYCLE] expr [START WITH expr]。
/// 作为 PlainSelect.OracleHierarchical 属性承载（不实现 Expression 接口，避免破坏 visitor 体系）。
/// </summary>
public class OracleHierarchicalExpression : ASTNodeAccessImpl
{
    /// <summary>START WITH 表达式（起点条件），未指定时为 null。</summary>
    public IExpression? StartExpression { get; set; }

    /// <summary>CONNECT BY 表达式（连接条件）。</summary>
    public IExpression? ConnectExpression { get; set; }

    /// <summary>是否 NOCYCLE。</summary>
    public bool NoCycle { get; set; }

    /// <summary>是否 CONNECT BY 在 START WITH 之前（connectFirst）。</summary>
    public bool ConnectFirst { get; set; }

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder();
        var connectPart = $"CONNECT BY {(NoCycle ? "NOCYCLE " : "")}{ConnectExpression}";

        if (ConnectFirst)
        {
            sb.Append(connectPart);
            if (StartExpression != null)
                sb.Append(" START WITH ").Append(StartExpression);
        }
        else
        {
            sb.Append("START WITH ").Append(StartExpression).Append(' ').Append(connectPart);
        }
        return sb.ToString();
    }
}

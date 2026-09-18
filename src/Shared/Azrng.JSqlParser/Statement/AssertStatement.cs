using System.Text;
using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Statement;

/// <summary>
/// BigQuery ASSERT 语句形式：ASSERT condition [AS 'message']（#2642）。
/// 函数形式 ASSERT(cond, 'msg') 走 Function 表达式。
/// </summary>
public class AssertStatement : ASTNodeAccessImpl, IStatement
{
    public IExpression? Condition { get; set; }

    /// <summary>AS 'message' 的消息原文（含引号），未指定时为 null。</summary>
    public string? Message { get; set; }

    public T Accept<T, S>(IStatementVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        var sb = new StringBuilder("ASSERT ").Append(Condition);
        if (Message != null) sb.Append(" AS ").Append(Message);
        return sb.ToString();
    }
}

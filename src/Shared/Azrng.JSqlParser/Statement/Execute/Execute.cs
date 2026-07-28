using System.Text;
using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Statement.Execute;

/// <summary>
/// EXECUTE / EXEC / CALL 语句，对齐上游 Execute。
/// 形式：<c>EXECUTE proc(args)</c> / <c>CALL proc(args)</c>。
/// </summary>
public class Execute : ASTNodeAccessImpl, IStatement
{
    public ExecType ExecType { get; set; } = ExecType.EXECUTE;

    public string Name { get; set; } = "";

    /// <summary>参数列表（带括号），无参数时为 null。</summary>
    public ExpressionList? ExprList { get; set; }

    /// <summary>
    /// 无括号或含 OUTPUT 的参数原文列表（#268），如 <c>'foo'</c>、<c>@out OUTPUT</c>、<c>@p1 = 1</c>。
    /// 优先于 <see cref="ExprList"/> 用于 ToString。
    /// </summary>
    public List<string>? PlainArguments { get; set; }

    /// <summary>参数是否使用括号形式 <c>EXEC p(a,b)</c>。</summary>
    public bool HasParentheses { get; set; }

    public T Accept<T, S>(IStatementVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        var sb = new StringBuilder($"{ExecType} {Name}");
        if (PlainArguments is { Count: > 0 })
        {
            if (HasParentheses)
                sb.Append('(').Append(string.Join(", ", PlainArguments)).Append(')');
            else
                sb.Append(' ').Append(string.Join(", ", PlainArguments));
        }
        else if (ExprList != null && ExprList.Expressions.Count > 0)
        {
            sb.Append($"({string.Join(", ", ExprList.Expressions)})");
        }
        // 空括号 CALL p()：历史 ToString 省略 ()，与 StatementsBatch4 对齐不输出空括号
        return sb.ToString();
    }
}

/// <summary>执行类型，对齐上游 ExecType。</summary>
public enum ExecType { EXECUTE, EXEC, CALL }

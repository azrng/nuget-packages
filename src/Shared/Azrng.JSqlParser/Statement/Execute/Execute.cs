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

    public T Accept<T, S>(IStatementVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        var sb = new StringBuilder($"{ExecType} {Name}");
        if (ExprList != null && ExprList.Expressions.Count > 0)
            sb.Append($"({string.Join(", ", ExprList.Expressions)})");
        return sb.ToString();
    }
}

/// <summary>执行类型，对齐上游 ExecType。</summary>
public enum ExecType { EXECUTE, EXEC, CALL }

using System.Text;
using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Statement;

/// <summary>
/// IF...ELSE 语句，对齐上游 IfElseStatement。
///
/// 形式：<c>IF condition statement [ELSE statement]</c>。
/// 注意：上游不支持 PL/SQL 的 THEN/ELSIF/END IF，仅 T-SQL/Postgres 风格。
/// </summary>
public class IfElseStatement : ASTNodeAccessImpl, IStatement
{
    /// <summary>IF 条件。</summary>
    public required Expression.IExpression Condition { get; set; }

    /// <summary>IF 分支语句。</summary>
    public required IStatement IfStatement { get; set; }

    /// <summary>ELSE 分支语句，可选。</summary>
    public IStatement? ElseStatement { get; set; }

    public T Accept<T, S>(IStatementVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        var sb = new StringBuilder($"IF {Condition} {IfStatement}");
        if (ElseStatement != null) sb.Append($" ELSE {ElseStatement}");
        return sb.ToString();
    }
}

using System.Text;
using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Schema;

namespace Azrng.JSqlParser.Statement.Create.Policy;

/// <summary>
/// PostgreSQL CREATE POLICY 语句（行级安全 RLS）。
/// <para>
/// 语法：CREATE POLICY name ON table
///   [ FOR { ALL | SELECT | INSERT | UPDATE | DELETE } ]
///   [ TO { role | PUBLIC | CURRENT_USER | SESSION_USER } [, ...] ]
///   [ USING ( expression ) ]
///   [ WITH CHECK ( expression ) ]
/// </para>
/// 移植自上游 JSqlParser commit 999cdca2 的 CreatePolicy。
/// </summary>
public class CreatePolicy : ASTNodeAccessImpl, IStatement
{
    public string? PolicyName { get; set; }
    public Table? Table { get; set; }

    /// <summary>命令类型：ALL / SELECT / INSERT / UPDATE / DELETE，未指定时为 null。</summary>
    public string? Command { get; set; }

    /// <summary>角色列表（TO 子句），未指定时为空。</summary>
    public List<string> Roles { get; set; } = new();

    /// <summary>USING ( expression ) 子句，未指定时为 null。</summary>
    public Expression.IExpression? UsingExpression { get; set; }

    /// <summary>WITH CHECK ( expression ) 子句，未指定时为 null。</summary>
    public Expression.IExpression? WithCheckExpression { get; set; }

    public T Accept<T, S>(IStatementVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        var sb = new StringBuilder("CREATE POLICY ");
        sb.Append(PolicyName).Append(" ON ").Append(Table);

        if (Command != null) sb.Append(" FOR ").Append(Command);

        if (Roles.Count > 0) sb.Append(" TO ").Append(string.Join(", ", Roles));

        if (UsingExpression != null) sb.Append(" USING (").Append(UsingExpression).Append(')');

        if (WithCheckExpression != null) sb.Append(" WITH CHECK (").Append(WithCheckExpression).Append(')');

        return sb.ToString();
    }
}

using System.Text;
using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Statement;

/// <summary>
/// PL/SQL / T-SQL BEGIN...END 块，对齐上游 Block。
///
/// 块体为 <see cref="Statements"/> 列表（复用通用 statement 入口）。
/// 注意：上游不含 DECLARE 段和 EXCEPTION 段（DECLARE 在 Block 外）。
/// </summary>
public class Block : ASTNodeAccessImpl, Statement
{
    public Statements Statements { get; set; } = new();

    /// <summary>END 后是否带分号（对齐上游 hasSemicolonAfterEnd）。</summary>
    public bool HasSemicolonAfterEnd { get; set; }

    public T Accept<T, S>(StatementVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        var sb = new StringBuilder("BEGIN ");
        if (Statements.StatementList != null)
        {
            sb.Append(string.Join("; ", Statements.StatementList));
        }
        sb.Append("; END");
        if (HasSemicolonAfterEnd) sb.Append(';');
        return sb.ToString();
    }
}

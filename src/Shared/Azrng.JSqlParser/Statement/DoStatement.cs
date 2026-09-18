using System.Text;
using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Statement;

/// <summary>
/// DO 语句（#2589 PostgreSQL 匿名代码块，如 <c>DO $$ BEGIN ... END $$;</c>，
/// 也覆盖 MySQL DO expr 形式）。DO 之后整体透传保 round-trip。
/// </summary>
public class DoStatement : ASTNodeAccessImpl, IStatement
{
    /// <summary>DO 之后的原文（LANGUAGE 前缀、代码块字面量等），透传。</summary>
    public string? Text { get; set; }

    public T Accept<T, S>(IStatementVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        var sb = new StringBuilder("DO");
        if (!string.IsNullOrEmpty(Text)) sb.Append(' ').Append(Text);
        return sb.ToString();
    }
}

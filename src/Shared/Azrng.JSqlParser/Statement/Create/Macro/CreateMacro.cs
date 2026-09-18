using System.Text;
using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Statement.Create.Macro;

/// <summary>
/// DuckDB CREATE MACRO name(params) AS expr / CREATE MACRO m AS TABLE ...（#2643）。
/// 简化透传版：MACRO 之后整体透传保 round-trip。
/// </summary>
public class CreateMacro : ASTNodeAccessImpl, IStatement
{
    public string? Text { get; set; }

    public T Accept<T, S>(IStatementVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        var sb = new StringBuilder("CREATE MACRO");
        if (!string.IsNullOrEmpty(Text)) sb.Append(' ').Append(Text);
        return sb.ToString();
    }
}

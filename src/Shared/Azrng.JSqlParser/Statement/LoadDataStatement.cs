using System.Text;
using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Statement;

/// <summary>
/// BigQuery LOAD DATA [OVERWRITE] ...（#2642 外表装载语句），整体透传保 round-trip。
/// </summary>
public class LoadDataStatement : ASTNodeAccessImpl, IStatement
{
    public bool Overwrite { get; set; }

    /// <summary>OVERWRITE 之后的原文，透传。</summary>
    public string? Text { get; set; }

    public T Accept<T, S>(IStatementVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        var sb = new StringBuilder("LOAD DATA ");
        if (Overwrite) sb.Append("OVERWRITE ");
        if (!string.IsNullOrEmpty(Text)) sb.Append(Text);
        return sb.ToString();
    }
}

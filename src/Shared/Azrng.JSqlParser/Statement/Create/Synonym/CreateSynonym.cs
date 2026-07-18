using System.Text;
using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Statement.Create.Synonym;

/// <summary>
/// CREATE SYNONYM 语句（Oracle），对齐上游 CreateSynonym。
/// 形式：<c>CREATE [OR REPLACE] [PUBLIC] SYNONYM name FOR target</c>。
/// </summary>
public class CreateSynonym : ASTNodeAccessImpl, IStatement
{
    /// <summary>同义词名。</summary>
    public string Name { get; set; } = "";

    /// <summary>是否带 OR REPLACE。</summary>
    public bool OrReplace { get; set; }

    /// <summary>是否 PUBLIC 同义词。</summary>
    public bool PublicSynonym { get; set; }

    /// <summary>FOR 目标列表（可多个）。</summary>
    public List<string> ForList { get; } = new();

    public T Accept<T, S>(IStatementVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        var sb = new StringBuilder("CREATE ");
        if (OrReplace) sb.Append("OR REPLACE ");
        if (PublicSynonym) sb.Append("PUBLIC ");
        sb.Append("SYNONYM ").Append(Name);
        if (ForList.Count > 0) sb.Append(" FOR ").Append(string.Join(", ", ForList));
        return sb.ToString();
    }
}

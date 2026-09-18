using System.Text;
using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Statement.Select;

namespace Azrng.JSqlParser.Statement.Export;

/// <summary>
/// BigQuery EXPORT DATA ['uri' | OPTIONS(...)] [AS] query（#2642）。
/// uri 与选项透传，查询结构化。
/// </summary>
public class ExportData : ASTNodeAccessImpl, IStatement
{
    /// <summary>EXPORT DATA 与查询之间的片段原文列表（'uri'、OPTIONS(...)、WITH CONNECTION ...），透传。</summary>
    public List<string> Specs { get; } = new();

    /// <summary>AS 关键字标志。</summary>
    public bool UseAs { get; set; }

    /// <summary>导出的查询。</summary>
    public Select.Select? Select { get; set; }

    public T Accept<T, S>(IStatementVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        var sb = new StringBuilder("EXPORT DATA");
        foreach (var spec in Specs) sb.Append(' ').Append(spec);
        if (UseAs) sb.Append(" AS");
        if (Select != null) sb.Append(' ').Append(Select);
        return sb.ToString();
    }
}

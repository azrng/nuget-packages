using System.Text;
using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Statement;

public class ExplainStatement : ASTNodeAccessImpl, IStatement
{
    public IStatement? Statement { get; set; }

    /// <summary>EXPLAIN ANALYZE 前缀（语句级，区别于括号内 ANALYZE 选项）。</summary>
    public bool Analyze { get; set; }

    /// <summary>EXPLAIN VERBOSE 前缀（PostgreSQL）。</summary>
    public bool Verbose { get; set; }

    /// <summary>
    /// PostgreSQL 括号选项原始文本（如 <c>(ANALYZE, VERBOSE, COSTS, BUFFERS)</c> / <c>(FORMAT JSON)</c>）。
    /// 为 null 表示未指定括号选项；非 null 时 ToString 原样输出（保 round-trip）。
    /// </summary>
    public string? Options { get; set; }

    public T Accept<T, S>(IStatementVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        var sb = new StringBuilder("EXPLAIN");
        if (Analyze) sb.Append(" ANALYZE");
        if (Verbose) sb.Append(" VERBOSE");
        if (!string.IsNullOrEmpty(Options)) sb.Append(' ').Append(Options);
        sb.Append(' ').Append(Statement);
        return sb.ToString();
    }
}

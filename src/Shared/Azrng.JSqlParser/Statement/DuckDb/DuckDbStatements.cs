using System.Text;
using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Statement.DuckDb;

/// <summary>
/// DuckDB COPY 语句（#2643）：COPY t TO 'file' (FORMAT CSV, ...) / COPY (SELECT ...) TO 'file'。
/// 源与方向结构化，目标与选项透传。
/// </summary>
public class CopyStatement : ASTNodeAccessImpl, IStatement
{
    /// <summary>COPY 与 TO/FROM 之间的源原文（表名或括号查询），透传。</summary>
    public string Source { get; set; } = "";

    /// <summary>方向：true=TO（导出），false=FROM（导入）。</summary>
    public bool To { get; set; }

    /// <summary>目标文件与选项原文，透传。</summary>
    public string? Tail { get; set; }

    public T Accept<T, S>(IStatementVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        var sb = new StringBuilder("COPY ").Append(Source).Append(To ? " TO " : " FROM ");
        if (Tail != null) sb.Append(Tail);
        return sb.ToString();
    }
}

/// <summary>DuckDB ATTACH 'file.db' [AS alias]（#2643），整体透传。</summary>
public class AttachStatement : ASTNodeAccessImpl, IStatement
{
    public string? Text { get; set; }

    public T Accept<T, S>(IStatementVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        var sb = new StringBuilder("ATTACH");
        if (!string.IsNullOrEmpty(Text)) sb.Append(' ').Append(Text);
        return sb.ToString();
    }
}

/// <summary>DuckDB/SQLite PRAGMA 语句（#2643），整体透传。</summary>
public class PragmaStatement : ASTNodeAccessImpl, IStatement
{
    public string? Text { get; set; }

    public T Accept<T, S>(IStatementVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        var sb = new StringBuilder("PRAGMA");
        if (!string.IsNullOrEmpty(Text)) sb.Append(' ').Append(Text);
        return sb.ToString();
    }
}

using System.Text;
using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Schema;

namespace Azrng.JSqlParser.Statement.Create.Publication;

/// <summary>
/// CREATE PUBLICATION name [FOR ALL TABLES | FOR TABLE t1, t2] [WITH (...)]（#2554 PG publication DDL）。
/// </summary>
public class CreatePublication : ASTNodeAccessImpl, IStatement
{
    public string Name { get; set; } = "";

    /// <summary>FOR ALL TABLES 形式标志。</summary>
    public bool ForAllTables { get; set; }

    /// <summary>FOR TABLE 指定的表列表，未指定时为 null。</summary>
    public List<Table>? Tables { get; set; }

    /// <summary>WITH 之后的发布选项原文，整体透传，未指定时为 null。</summary>
    public string? OptionsText { get; set; }

    public T Accept<T, S>(IStatementVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        var sb = new StringBuilder("CREATE PUBLICATION ").Append(Name);
        if (ForAllTables) sb.Append(" FOR ALL TABLES");
        else if (Tables is { Count: > 0 }) sb.Append(" FOR TABLE ").Append(string.Join(", ", Tables));
        if (OptionsText != null) sb.Append(" WITH ").Append(OptionsText);
        return sb.ToString();
    }
}

using System.Text;
using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Statement.Create.Extension;

/// <summary>
/// CREATE EXTENSION [IF NOT EXISTS] name [WITH (SCHEMA|VERSION|CASCADE)...]（#2553 PG extension DDL）。
/// 简化透传版：名称结构化，WITH 选项整体透传保 round-trip。
/// </summary>
public class CreateExtension : ASTNodeAccessImpl, IStatement
{
    public bool IfNotExists { get; set; }

    public string Name { get; set; } = "";

    /// <summary>WITH 之后的选项原文，整体透传，未指定时为 null。</summary>
    public string? OptionsText { get; set; }

    public T Accept<T, S>(IStatementVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        var sb = new StringBuilder("CREATE EXTENSION ");
        if (IfNotExists) sb.Append("IF NOT EXISTS ");
        sb.Append(Name);
        if (OptionsText != null) sb.Append(" WITH ").Append(OptionsText);
        return sb.ToString();
    }
}

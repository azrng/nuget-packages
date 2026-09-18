using System.Text;
using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Statement.Create.Domain;

/// <summary>
/// CREATE DOMAIN [IF NOT EXISTS] [schema.]name ...（#2536 PG type domain DDL）。
/// 简化透传版：名称结构化，AS 类型/COLLATE/DEFAULT/约束体整体透传保 round-trip。
/// </summary>
public class CreateDomain : ASTNodeAccessImpl, IStatement
{
    public bool IfNotExists { get; set; }

    /// <summary>域名（可含 schema 限定，原文点号形式）。</summary>
    public string Name { get; set; } = "";

    /// <summary>类型定义及之后全部子句原文（AS type COLLATE ... DEFAULT ... CHECK ...），透传。</summary>
    public string? Tail { get; set; }

    public T Accept<T, S>(IStatementVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        var sb = new StringBuilder("CREATE DOMAIN ");
        if (IfNotExists) sb.Append("IF NOT EXISTS ");
        sb.Append(Name);
        if (Tail != null) sb.Append(' ').Append(Tail);
        return sb.ToString();
    }
}

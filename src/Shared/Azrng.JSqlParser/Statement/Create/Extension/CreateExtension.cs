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

    /// <summary>WITH 之后的选项列表（SCHEMA/VERSION/CASCADE），未指定时为 null。</summary>
    public List<ExtensionOption>? Options { get; set; }

    public T Accept<T, S>(IStatementVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        var sb = new StringBuilder("CREATE EXTENSION ");
        if (IfNotExists) sb.Append("IF NOT EXISTS ");
        sb.Append(Name);
        if (Options is { Count: > 0 })
            sb.Append(" WITH ").Append(string.Join(" ", Options));
        return sb.ToString();
    }
}

/// <summary>CREATE EXTENSION 单个选项。</summary>
public class ExtensionOption
{
    public ExtensionOptionKind Kind { get; set; }

    /// <summary>SCHEMA/VERSION 的参数原文，CASCADE 时为 null。</summary>
    public string? Value { get; set; }

    public override string ToString() => Kind switch
    {
        ExtensionOptionKind.Schema => $"SCHEMA {Value}",
        ExtensionOptionKind.Version => $"VERSION {Value}",
        _ => "CASCADE"
    };
}

/// <summary>CREATE EXTENSION 选项类别。</summary>
public enum ExtensionOptionKind
{
    Schema,
    Version,
    Cascade
}

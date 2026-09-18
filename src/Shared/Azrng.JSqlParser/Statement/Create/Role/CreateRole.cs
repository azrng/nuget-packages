using System.Text;
using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Statement.Create.Role;

/// <summary>
/// CREATE USER / CREATE ROLE / CREATE GROUP（#2546 MySQL CREATE USER、#2555 PG role DDL）。
/// 简化透传版：名称结构化，WITH 属性整体透传保 round-trip（对齐上游 CreateUser/CreateRole 的能力子集）。
/// </summary>
public class CreateRole : ASTNodeAccessImpl, IStatement
{
    /// <summary>CREATE 后的命令字：USER / ROLE / GROUP。</summary>
    public string Command { get; set; } = "USER";

    public bool IfNotExists { get; set; }

    /// <summary>账户/角色名原文（含引号形式，如 'jeffrey'）。</summary>
    public string Name { get; set; } = "";

    /// <summary>@host 部分（MySQL 账户），未指定时为 null。</summary>
    public string? Host { get; set; }

    /// <summary>WITH 之后的属性原文（PG LOGIN/PASSWORD/...），整体透传，未指定时为 null。</summary>
    public string? OptionsText { get; set; }

    public T Accept<T, S>(IStatementVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        var sb = new StringBuilder("CREATE ").Append(Command).Append(' ');
        if (IfNotExists) sb.Append("IF NOT EXISTS ");
        sb.Append(Name);
        if (Host != null) sb.Append('@').Append(Host);
        if (OptionsText != null) sb.Append(' ').Append(OptionsText);
        return sb.ToString();
    }
}

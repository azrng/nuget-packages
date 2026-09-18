using System.Text;
using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Statement.Create.Subscription;

/// <summary>
/// CREATE SUBSCRIPTION name CONNECTION 'conn' PUBLICATION pub1, pub2 [WITH (...)]（#2554 PG subscription DDL）。
/// </summary>
public class CreateSubscription : ASTNodeAccessImpl, IStatement
{
    public string Name { get; set; } = "";

    /// <summary>CONNECTION 连接串原文（含引号）。</summary>
    public string Connection { get; set; } = "";

    /// <summary>PUBLICATION 后的发布名列表。</summary>
    public List<string> Publications { get; set; } = new();

    /// <summary>WITH 之后的订阅选项原文，整体透传，未指定时为 null。</summary>
    public string? OptionsText { get; set; }

    public T Accept<T, S>(IStatementVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        var sb = new StringBuilder("CREATE SUBSCRIPTION ").Append(Name);
        sb.Append(" CONNECTION ").Append(Connection);
        sb.Append(" PUBLICATION ").Append(string.Join(", ", Publications));
        if (OptionsText != null) sb.Append(" WITH ").Append(OptionsText);
        return sb.ToString();
    }
}

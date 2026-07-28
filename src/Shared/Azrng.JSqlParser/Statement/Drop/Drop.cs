using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Schema;

namespace Azrng.JSqlParser.Statement.Drop;

/// <summary>
/// Represents a DROP statement in SQL (DROP TABLE/VIEW/INDEX, etc.).
/// </summary>
/// <remarks>
/// 支持多表 DROP（#2065）：<c>DROP TABLE IF EXISTS t1, t2, t3 [CASCADE|RESTRICT]</c>。
/// <see cref="Name"/> 保留为首个对象（向后兼容）；完整列表见 <see cref="NameList"/>。
/// </remarks>
public class Drop : ASTNodeAccessImpl, IStatement
{
    public string Type { get; set; } = "";

    /// <summary>首个被 DROP 的对象（向后兼容；多表时等于 <see cref="NameList"/>[0]）。</summary>
    public Table? Name { get; set; }

    /// <summary>全部被 DROP 的对象列表（#2065 多表）。单对象时仍填充为单元素列表。</summary>
    public List<Table>? NameList { get; set; }

    public bool IfExists { get; set; }

    /// <summary>CASCADE 或 RESTRICT（可选）。</summary>
    public string? DropBehavior { get; set; }

    /// <summary>DROP INDEX ... ON table 的表名。</summary>
    public Table? On { get; set; }

    public T Accept<T, S>(IStatementVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder("DROP ");
        sb.Append(Type).Append(' ');
        if (IfExists) sb.Append("IF EXISTS ");
        if (NameList is { Count: > 0 })
            sb.Append(string.Join(", ", NameList));
        else
            sb.Append(Name);
        if (On != null) sb.Append(" ON ").Append(On);
        if (!string.IsNullOrEmpty(DropBehavior)) sb.Append(' ').Append(DropBehavior);
        return sb.ToString();
    }
}

using Azrng.JSqlParser.Schema;

namespace Azrng.JSqlParser.Statement.CreateTable;

/// <summary>
/// 外键约束，对齐上游 <c>net.sf.jsqlparser.statement.create.table.ForeignKeyIndex</c>。
/// 在 <see cref="Constraint"/> 基础上补全引用表、引用列与 ON DELETE/UPDATE 引用动作。
/// </summary>
public class ForeignKeyIndex : Constraint
{
    /// <summary>被引用表（REFERENCES 后的目标表）。</summary>
    public Table? ReferencedTable { get; set; }

    /// <summary>被引用列名列表。未指定时为 null。</summary>
    public System.Collections.Generic.List<string>? ReferencedColumnNames { get; set; }

    /// <summary>ON DELETE 引用动作。未指定时为 null。</summary>
    public ReferentialAction? OnDelete { get; set; }

    /// <summary>ON UPDATE 引用动作。未指定时为 null。</summary>
    public ReferentialAction? OnUpdate { get; set; }

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder();
        sb.Append(base.ToString());
        if (ReferencedTable != null)
        {
            sb.Append(" REFERENCES ").Append(ReferencedTable);
            if (ReferencedColumnNames is { Count: > 0 })
            {
                sb.Append("(").Append(string.Join(", ", ReferencedColumnNames)).Append(')');
            }
        }
        if (OnDelete != null) sb.Append(' ').Append(OnDelete);
        if (OnUpdate != null) sb.Append(' ').Append(OnUpdate);
        return sb.ToString();
    }
}

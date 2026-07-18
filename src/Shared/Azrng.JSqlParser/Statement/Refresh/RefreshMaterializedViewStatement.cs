using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Schema;

namespace Azrng.JSqlParser.Statement.Refresh;

/// <summary>
/// PostgreSQL REFRESH MATERIALIZED VIEW 语句，对齐上游 RefreshMaterializedViewStatement。
/// </summary>
public class RefreshMaterializedViewStatement : ASTNodeAccessImpl, IStatement
{
    public Table? View { get; set; }

    /// <summary>刷新模式：WITH DATA / WITH NO DATA / 未指定（DEFAULT）。对齐上游 refreshMode。</summary>
    public RefreshMode RefreshMode { get; set; } = RefreshMode.Default;

    /// <summary>是否 CONCURRENTLY 刷新。</summary>
    public bool Concurrently { get; set; }

    public T Accept<T, S>(IStatementVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder("REFRESH MATERIALIZED VIEW ");
        if (Concurrently) sb.Append("CONCURRENTLY ");
        sb.Append(View);
        // 注：PostgreSQL 不允许 CONCURRENTLY 与 WITH NO DATA 并存；上游对此降级处理。
        // 此处按 RefreshMode 输出，Concurrently + WithNoData 时仍输出 WITH NO DATA（与上游 toString 一致）。
        if (RefreshMode == RefreshMode.WithData) sb.Append(" WITH DATA");
        else if (RefreshMode == RefreshMode.WithNoData) sb.Append(" WITH NO DATA");
        return sb.ToString();
    }
}

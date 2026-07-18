using System.Text;
using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Schema;

namespace Azrng.JSqlParser.Statement.Select;

/// <summary>
/// Abstract base class for SELECT statements.
/// Implements both Statement and Expression interfaces.
/// </summary>
public abstract class Select : ASTNodeAccessImpl, IStatement, IExpression
{
    public List<WithItem>? WithItemsList { get; set; }

    public Limit? Limit { get; set; }

    public Limit? LimitBy { get; set; }

    public Offset? Offset { get; set; }

    public Fetch? Fetch { get; set; }

    public bool OracleSiblings { get; set; }

    public List<OrderByElement>? OrderByElements { get; set; }

    /// <summary>FOR UPDATE / FOR SHARE 锁模式，未指定时为 null。</summary>
    public ForMode? ForMode { get; set; }

    /// <summary>FOR UPDATE OF 表列表，未指定时为 null。</summary>
    public List<Table>? ForUpdateTables { get; set; }

    /// <summary>WAIT n 等待超时，未指定时为 null。</summary>
    public Wait? Wait { get; set; }

    /// <summary>是否指定 NOWAIT。</summary>
    public bool NoWait { get; set; }

    /// <summary>是否指定 SKIP LOCKED。</summary>
    public bool SkipLocked { get; set; }

    /// <summary>
    /// Oracle 风格下 FOR UPDATE 出现在 ORDER BY 之前，反序列化时需将 ORDER BY 输出在 FOR UPDATE 之后。
    /// </summary>
    public bool ForUpdateBeforeOrderBy { get; set; }

    /// <summary>DB2 WITH ISOLATION 隔离级别（UR/RS/RR/CS），未指定时为 null。对齐上游 isolation。</summary>
    public string? Isolation { get; set; }

    public abstract T Accept<T, S>(ISelectVisitor<T> selectVisitor, S context);

    public T Accept<T, S>(IStatementVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public T Accept<T, S>(IExpressionVisitor<T> visitor, S context) => visitor.Visit(this, context);

    /// <summary>返回 OF 子句的第一个表，未指定时为 null。</summary>
    public Table? ForUpdateTable => (ForUpdateTables != null && ForUpdateTables.Count > 0) ? ForUpdateTables[0] : null;

    /// <summary>
    /// 根据当前 FOR UPDATE 字段组装并返回 <see cref="ForUpdateClause"/> 视图。
    /// 当 <see cref="ForMode"/> 为 null（未指定 FOR 子句）时返回 null。
    /// </summary>
    public ForUpdateClause? GetForUpdate()
    {
        if (ForMode == null) return null;
        return new ForUpdateClause
               {
                   Mode = ForMode,
                   Tables = ForUpdateTables,
                   Wait = Wait,
                   NoWait = NoWait,
                   SkipLocked = SkipLocked
               };
    }

    public abstract StringBuilder AppendSelectBodyTo(StringBuilder builder);

    public override StringBuilder AppendTo(StringBuilder builder)
    {
        if (WithItemsList != null && WithItemsList.Count > 0)
        {
            // RECURSIVE 关键字只输出一次（紧跟 WITH 之后），SQL 标准：WITH RECURSIVE a, b。
            // 任一 WithItem.Recursive=true 即视为整体递归（对齐上游 WithItem.recursive 字段语义）。
            bool anyRecursive = false;
            foreach (var w in WithItemsList)
            {
                if (w.Recursive) { anyRecursive = true; break; }
            }
            builder.Append("WITH ");
            if (anyRecursive) builder.Append("RECURSIVE ");
            for (int i = 0; i < WithItemsList.Count; i++)
            {
                if (i > 0) builder.Append(", ");
                builder.Append(WithItemsList[i]);
                builder.Append(' ');
            }
        }

        AppendSelectBodyTo(builder);

        // 非前置场景：ORDER BY 在 FOR UPDATE 之前输出
        if (!ForUpdateBeforeOrderBy)
        {
            AppendOrderByTo(builder);
        }

        // ksqlDB EMIT CHANGES（ORDER BY 之后、LIMIT 之前）
        if (this is PlainSelect { EmitChanges: true })
        {
            builder.Append(" EMIT CHANGES");
        }

        if (LimitBy != null) builder.Append(LimitBy);
        if (Limit != null) builder.Append(Limit);
        if (Offset != null) builder.Append(Offset);
        if (Fetch != null) builder.Append(Fetch);

        // DB2 WITH ISOLATION 隔离级别（FETCH 之后、FOR UPDATE 之前）
        if (Isolation != null) builder.Append(" WITH ").Append(Isolation);

        // FOR UPDATE / FOR SHARE 子句
        if (ForMode != null)
        {
            builder.Append(" FOR ").Append(((ForMode)ForMode).GetValue());

            if (ForUpdateTables != null && ForUpdateTables.Count > 0)
            {
                builder.Append(" OF ");
                for (int i = 0; i < ForUpdateTables.Count; i++)
                {
                    if (i > 0) builder.Append(", ");
                    builder.Append(ForUpdateTables[i]);
                }
            }

            if (Wait != null) builder.Append(Wait);

            if (NoWait) builder.Append(" NOWAIT");
            else if (SkipLocked) builder.Append(" SKIP LOCKED");
        }

        // 前置场景（Oracle 风格）：ORDER BY 在 FOR UPDATE 之后输出
        if (ForUpdateBeforeOrderBy)
        {
            AppendOrderByTo(builder);
        }

        // SQL Server FOR CLAUSE（FOR XML PATH / FOR XML RAW|AUTO|EXPLICIT / FOR JSON / FOR BROWSE）
        if (this is PlainSelect ps2)
        {
            // ForClause（透传）优先于 ForXmlPath（向后兼容）
            if (ps2.ForClause != null)
            {
                builder.Append(" FOR ").Append(ps2.ForClause);
            }
            else if (ps2.ForXmlPath != null)
            {
                builder.Append(" FOR XML PATH");
                if (ps2.ForXmlPath.Length > 0) builder.Append("(").Append(ps2.ForXmlPath).Append(")");
            }
        }

        return builder;
    }

    /// <summary>将 ORDER BY 子句追加到 builder（仅在有元素时输出）。</summary>
    private StringBuilder AppendOrderByTo(StringBuilder builder)
    {
        if (OrderByElements != null && OrderByElements.Count > 0)
        {
            builder.Append(OracleSiblings ? " ORDER SIBLINGS BY " : " ORDER BY ");
            builder.Append(string.Join(", ", OrderByElements));
        }

        return builder;
    }

    public override string ToString() => AppendTo(new StringBuilder()).ToString();
}
using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Schema;

namespace Azrng.JSqlParser.Statement.Insert;

/// <summary>
/// Represents an INSERT statement in SQL.
/// </summary>
public class Insert : ASTNodeAccessImpl, IStatement
{
    public Table? Table { get; set; }
    public System.Collections.Generic.List<Column>? Columns { get; set; }
    public Select.Select? Select { get; set; }
    public System.Collections.Generic.List<Update.UpdateSet>? DuplicateUpdateSets { get; set; }

    /// <summary>MySQL 8.0.20+ ON DUPLICATE KEY UPDATE ... WHERE 的条件表达式，未指定时为 null。</summary>
    public Expression.IExpression? DuplicateUpdateWhereExpression { get; set; }
    public System.Collections.Generic.List<Update.UpdateSet>? SetUpdateSets { get; set; }
    public bool UseValues { get; set; } = true;

    /// <summary>
    /// MySQL INSERT 优先级修饰符（LOW_PRIORITY/DELAYED/HIGH_PRIORITY）。
    /// 与上游 InsertModifierPriority 对齐。None 表示未指定。
    /// </summary>
    public InsertModifierPriority ModifierPriority { get; set; } = InsertModifierPriority.None;

    /// <summary>MySQL INSERT IGNORE 标志。</summary>
    public bool ModifierIgnore { get; set; }

    /// <summary>PostgreSQL ON CONFLICT 的冲突目标，未指定时为 null。</summary>
    public InsertConflictTarget? ConflictTarget { get; set; }

    /// <summary>PostgreSQL ON CONFLICT 的冲突动作，未指定时为 null。</summary>
    public InsertConflictAction? ConflictAction { get; set; }

    /// <summary>VALUES 子句的多行值列表，每行一个 ExpressionList。仅当使用 VALUES 时填充。</summary>
    public System.Collections.Generic.List<ExpressionList>? ValuesItems { get; set; }

    /// <summary>openGauss 的 ON DUPLICATE KEY UPDATE NOTHING 标志。</summary>
    public bool DuplicateUpdateNothing { get; set; }

    /// <summary>
    /// Partition references for INSERT INTO ... PARTITION (...) syntax.
    /// </summary>
    public System.Collections.Generic.List<Partition>? Partitions { get; set; }

    /// <summary>
    /// Whether this is an INSERT OVERWRITE statement.
    /// </summary>
    public bool Overwrite { get; set; }

    /// <summary>
    /// The TABLE keyword for INSERT INTO TABLE syntax.
    /// </summary>
    public bool TableKeyword { get; set; }

    /// <summary>
    /// OVERRIDING SYSTEM VALUE / USER VALUE syntax.
    /// </summary>
    public string? Overriding { get; set; }

    /// <summary>RETURNING / RETURN 子句，未指定时为 null。</summary>
    public ReturningClause? Returning { get; set; }

    /// <summary>MSSQL OUTPUT 子句（OUTPUT inserted.col [INTO ...]），透传原始文本保 round-trip。未指定时为 null。</summary>
    public string? OutputClause { get; set; }

    public T Accept<T, S>(IStatementVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder();
        sb.Append("INSERT ");
        // 输出 MySQL 修饰符（LOW_PRIORITY/DELAYED/HIGH_PRIORITY/IGNORE）
        if (ModifierPriority != InsertModifierPriority.None)
        {
            // LowPriority -> LOW_PRIORITY
            var name = ModifierPriority.ToString();
            var sb2 = new System.Text.StringBuilder();
            for (int i = 0; i < name.Length; i++)
            {
                if (i > 0 && char.IsUpper(name[i])) sb2.Append('_');
                sb2.Append(char.ToUpperInvariant(name[i]));
            }
            sb.Append(sb2).Append(' ');
        }
        if (ModifierIgnore) sb.Append("IGNORE ");
        if (Overwrite) sb.Append("OVERWRITE ");
        else sb.Append("INTO ");
        if (TableKeyword) sb.Append("TABLE ");
        sb.Append(Table);
        if (Partitions != null && Partitions.Count > 0)
        {
            sb.Append(" PARTITION (");
            Partition.AppendPartitionsTo(sb, Partitions);
            sb.Append(')');
        }
        if (Overriding != null) sb.Append(" OVERRIDING ").Append(Overriding).Append(" VALUE");
        if (Columns != null && Columns.Count > 0)
        {
            sb.Append(" (").Append(string.Join(", ", Columns)).Append(')');
        }
        if (OutputClause != null) sb.Append(' ').Append(OutputClause);
        if (Select != null) sb.Append(" ").Append(Select);
        else if (ValuesItems != null && ValuesItems.Count > 0)
        {
            sb.Append(" VALUES ");
            for (int i = 0; i < ValuesItems.Count; i++)
            {
                if (i > 0) sb.Append(", ");
                sb.Append('(').Append(ValuesItems[i]).Append(')');
            }
        }
        if (DuplicateUpdateNothing) sb.Append(" ON DUPLICATE KEY UPDATE NOTHING");
        else if (DuplicateUpdateSets != null && DuplicateUpdateSets.Count > 0)
        {
            sb.Append(" ON DUPLICATE KEY UPDATE ").Append(string.Join(", ", DuplicateUpdateSets));
            if (DuplicateUpdateWhereExpression != null)
                sb.Append(" WHERE ").Append(DuplicateUpdateWhereExpression);
        }
        if (ConflictAction != null)
        {
            sb.Append(" ON CONFLICT");
            if (ConflictTarget != null) sb.Append(ConflictTarget);
            sb.Append(ConflictAction);
        }
        if (Returning != null) sb.Append(Returning);
        return sb.ToString();
    }
}

using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Schema;
using Azrng.JSqlParser.Expression;

namespace Azrng.JSqlParser.Statement.Insert;

/// <summary>
/// UPSERT / REPLACE / INSERT OR REPLACE 语句，对齐上游 Upsert。
/// 支持 Firebird/CockroachDB UPSERT、MySQL REPLACE INTO、SQLite INSERT OR REPLACE。
/// </summary>
public class UpsertStatement : ASTNodeAccessImpl, Statement
{
    public UpsertType UpsertType { get; set; } = UpsertType.Upsert;

    /// <summary>是否使用 INTO 关键字。</summary>
    public bool UseInto { get; set; }

    public Table? Table { get; set; }

    public System.Collections.Generic.List<Column>? Columns { get; set; }

    /// <summary>SET col=val 形式的更新集（UPSERT t SET a=1）。</summary>
    public System.Collections.Generic.List<Update.UpdateSet>? SetUpdateSets { get; set; }

    public Select.Select? Select { get; set; }

    /// <summary>VALUES (...) 值列表（与 SET/SELECT 三选一）。</summary>
    public System.Collections.Generic.List<ExpressionList>? ValuesItems { get; set; }

    public bool UseValues { get; set; } = true;

    /// <summary>ON DUPLICATE KEY UPDATE 的更新集。</summary>
    public System.Collections.Generic.List<Update.UpdateSet>? DuplicateUpdateSets { get; set; }

    public bool DuplicateUpdateNothing { get; set; }

    public T Accept<T, S>(IStatementVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder();
        sb.Append(UpsertType switch
        {
            UpsertType.Upsert => "UPSERT",
            UpsertType.Replace => "REPLACE",
            UpsertType.InsertOrReplace => "INSERT OR REPLACE",
            _ => "UPSERT",
        });
        if (UseInto) sb.Append(" INTO");
        sb.Append(' ').Append(Table);

        if (Columns is { Count: > 0 })
            sb.Append(" (").Append(string.Join(", ", Columns)).Append(')');

        if (SetUpdateSets is { Count: > 0 })
        {
            sb.Append(" SET ");
            sb.Append(string.Join(", ", SetUpdateSets.Select(s => s.ToString())));
        }
        else if (Select != null)
        {
            sb.Append(' ').Append(Select);
        }
        else if (ValuesItems is { Count: > 0 })
        {
            sb.Append(" VALUES ");
            for (int i = 0; i < ValuesItems.Count; i++)
            {
                if (i > 0) sb.Append(", ");
                sb.Append('(').Append(ValuesItems[i]).Append(')');
            }
        }

        if (DuplicateUpdateSets is { Count: > 0 })
        {
            sb.Append(" ON DUPLICATE KEY UPDATE ");
            sb.Append(string.Join(", ", DuplicateUpdateSets.Select(s => s.ToString())));
        }
        else if (DuplicateUpdateNothing)
        {
            sb.Append(" ON DUPLICATE KEY UPDATE NOTHING");
        }

        return sb.ToString();
    }
}

/// <summary>UPSERT 语句类型，对齐上游 UpsertType。</summary>
public enum UpsertType
{
    /// <summary>UPSERT（Firebird/CockroachDB）。</summary>
    Upsert,

    /// <summary>REPLACE（MySQL REPLACE INTO）。</summary>
    Replace,

    /// <summary>INSERT OR REPLACE（SQLite）。</summary>
    InsertOrReplace,
}

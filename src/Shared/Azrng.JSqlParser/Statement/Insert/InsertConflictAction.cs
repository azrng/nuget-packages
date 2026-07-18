using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Statement.Update;

namespace Azrng.JSqlParser.Statement.Insert;

/// <summary>
/// PostgreSQL ON CONFLICT 的冲突动作：DO NOTHING 或 DO UPDATE SET ... [WHERE ...]。
/// 参考 https://www.postgresql.org/docs/current/sql-insert.html
/// </summary>
public class InsertConflictAction : ASTNodeAccessImpl, IModel
{
    public ConflictActionType ConflictActionType { get; set; }

    /// <summary>DO UPDATE SET 的列赋值列表，DO NOTHING 时为 null。</summary>
    public List<UpdateSet>? UpdateSets { get; set; }

    /// <summary>DO UPDATE SET 后的可选 WHERE 条件。</summary>
    public Expression.IExpression? WhereExpression { get; set; }

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder();
        switch (ConflictActionType)
        {
            case ConflictActionType.DoNothing:
                sb.Append(" DO NOTHING");
                break;
            case ConflictActionType.DoUpdate:
                sb.Append(" DO UPDATE SET ");
                if (UpdateSets != null)
                {
                    sb.Append(string.Join(", ", UpdateSets));
                }
                if (WhereExpression != null)
                {
                    sb.Append(" WHERE ").Append(WhereExpression);
                }
                break;
        }
        return sb.ToString();
    }
}

/// <summary>冲突动作类型。</summary>
public enum ConflictActionType
{
    /// <summary>openGauss 的 NOTHING（已废弃，统一用 DoNothing）。</summary>
    Nothing,

    /// <summary>PostgreSQL DO NOTHING。</summary>
    DoNothing,

    /// <summary>PostgreSQL DO UPDATE。</summary>
    DoUpdate
}

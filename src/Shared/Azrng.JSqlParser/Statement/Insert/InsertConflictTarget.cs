using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Parser;

namespace Azrng.JSqlParser.Statement.Insert;

/// <summary>
/// PostgreSQL ON CONFLICT 的冲突目标。
/// 参考 https://www.postgresql.org/docs/current/sql-insert.html
/// <para>
/// 两种形式：
/// <list type="bullet">
/// <item><c>(col1, col2, ...) [WHERE index_predicate]</c> 索引列形式</item>
/// <item><c>ON CONSTRAINT constraint_name</c> 约束名形式</item>
/// </list>
/// </para>
/// </summary>
public class InsertConflictTarget : ASTNodeAccessImpl, Model
{
    /// <summary>索引列名列表（与 IndexExpression 互斥）。</summary>
    public List<string> IndexColumnNames { get; set; } = new();

    /// <summary>索引表达式（与 IndexColumnNames 互斥）。当前文法未接入表达式形式。</summary>
    public Expression.Expression? IndexExpression { get; set; }

    /// <summary>可选的 WHERE 索引谓词。</summary>
    public Expression.Expression? WhereExpression { get; set; }

    /// <summary>ON CONSTRAINT 指定的约束名（与 IndexColumnNames/IndexExpression 互斥）。</summary>
    public string? ConstraintName { get; set; }

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder();
        if (ConstraintName != null)
        {
            sb.Append(" ON CONSTRAINT ").Append(ConstraintName);
        }
        else
        {
            sb.Append(" (").Append(string.Join(", ", IndexColumnNames)).Append(")");
            if (WhereExpression != null)
            {
                sb.Append(" WHERE ").Append(WhereExpression);
            }
        }
        return sb.ToString();
    }
}

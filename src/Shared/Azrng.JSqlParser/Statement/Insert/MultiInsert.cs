using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Schema;
using Azrng.JSqlParser.Statement.Select;
using Azrng.JSqlParser.Statement.Update;

namespace Azrng.JSqlParser.Statement.Insert;

/// <summary>
/// Oracle 多表插入语句（INSERT ALL / INSERT FIRST）。
/// 对应上游 commit 4f982e74 / issue #2394。
/// <para>
/// 语法示例：
/// <code>
/// INSERT ALL
///   WHEN age &gt; 18 THEN INTO adults (id, name) VALUES (id, name)
///   WHEN age &lt;= 18 THEN INTO minors (id, name) VALUES (id, name)
///   ELSE INTO others (id) VALUES (id)
///   SELECT id, name, age FROM src;
/// </code>
/// </para>
/// <para>
/// INSERT ALL 会评估所有 WHEN 分支（一条源行可入多表）；
/// INSERT FIRST 在命中第一个 WHEN 后停止评估（类似 if/elif/else）。
/// </para>
/// </summary>
public class MultiInsert : ASTNodeAccessImpl, Statement
{
    /// <summary>
    /// 多表插入的修饰类型。true 表示 INSERT FIRST（命中即停）；
    /// false 表示 INSERT ALL（评估全部 WHEN）。
    /// </summary>
    public bool IsFirst { get; set; }

    /// <summary>WHEN/ELSE 分支列表，至少包含一个分支。</summary>
    public List<MultiInsertBranch> Branches { get; set; } = new();

    /// <summary>
    /// 末尾的数据源 SELECT（INSERT ALL/FIRST 必须以 SELECT 结尾），
    /// 也可能是 INSERT ... SELECT 形式。
    /// </summary>
    public Select.Select? Select { get; set; }

    public T Accept<T, S>(StatementVisitor<T> visitor, S context) => visitor.Visit(this, context);

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder();
        sb.Append("INSERT ").Append(IsFirst ? "FIRST " : "ALL ");
        foreach (var branch in Branches)
        {
            sb.Append(branch);
        }
        if (Select != null) sb.Append(Select);
        return sb.ToString();
    }
}

/// <summary>
/// INSERT ALL/FIRST 中的一个 WHEN condition THEN INTO ... 或 ELSE INTO ... 分支。
/// </summary>
public class MultiInsertBranch : ASTNodeAccessImpl, Model
{
    /// <summary>
    /// WHEN 后的条件表达式；为 null 时可能是 ELSE 分支或无条件 INTO 分支，
    /// 通过 <see cref="IsElse"/> 区分。
    /// </summary>
    public Expression.Expression? WhenCondition { get; set; }

    /// <summary>true 表示这是 ELSE 分支；false 表示 WHEN 分支或无条件 INTO 分支。</summary>
    public bool IsElse { get; set; }

    /// <summary>分支目标表。</summary>
    public Table? Table { get; set; }

    /// <summary>列列表，未指定时为 null。</summary>
    public List<Column>? Columns { get; set; }

    /// <summary>
    /// 分支数据来源：VALUES (...) 或子查询。两者互斥。
    /// </summary>
    public List<ExpressionList>? ValuesItems { get; set; }

    /// <summary>子查询数据源，与 ValuesItems 互斥。</summary>
    public Select.Select? Select { get; set; }

    /// <summary>是否使用 VALUES 关键字。</summary>
    public bool UseValues { get; set; } = true;

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder();
        // 当 IsElse=true 时输出 ELSE，否则 WhenCondition 不为空输出 WHEN ... THEN，
        // WhenCondition 为空表示无条件 INTO 分支
        if (IsElse)
        {
            sb.Append("ELSE ");
        }
        else if (WhenCondition != null)
        {
            sb.Append("WHEN ").Append(WhenCondition).Append(" THEN ");
        }
        sb.Append("INTO ").Append(Table);
        if (Columns is { Count: > 0 })
        {
            sb.Append(" (").Append(string.Join(", ", Columns)).Append(')');
        }
        if (Select != null)
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
        sb.Append(' ');
        return sb.ToString();
    }
}

using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Schema;
using Azrng.JSqlParser.Statement.Select;
using Azrng.JSqlParser.Statement.Update;

namespace Azrng.JSqlParser.Statement.Insert;

/// <summary>
/// Oracle 多表插入语句（INSERT ALL / INSERT FIRST）。
/// 对应上游 commit 4f982e74 / OracleMultiInsertClause + OracleMultiInsertBranch。
/// <para>
/// 语法示例：
/// <code>
/// INSERT ALL
///   WHEN age &gt; 18 THEN
///     INTO adults (id, name) VALUES (id, name)
///     INTO logs (event) VALUES ('adult')   -- 单分支多 INTO 目标
///   ELSE
///     INTO minors (id, name) VALUES (id, name)
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

    /// <summary>WHEN/ELSE 分支列表，至少包含一个分支。每个分支可含一个或多个 INTO 目标。</summary>
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
/// INSERT ALL/FIRST 中的一个 WHEN condition THEN ... 或 ELSE ... 分支。
/// 一个分支可包含多个 INTO 目标子句（<see cref="Clauses"/>）。
/// 与上游 OracleMultiInsertBranch 对齐。
/// </summary>
public class MultiInsertBranch : ASTNodeAccessImpl, Model
{
    /// <summary>
    /// WHEN 后的条件表达式；为 null 时表示 ELSE 分支。
    /// 与 <see cref="IsElse"/> 互斥（设置其中一个会清空另一个）。
    /// </summary>
    public Expression.Expression? WhenCondition
    {
        get => _whenCondition;
        set
        {
            _whenCondition = value;
            if (value != null) _isElse = false;
        }
    }

    /// <summary>true 表示这是 ELSE 分支；false 表示 WHEN 分支。设置 true 会清空 WhenCondition。</summary>
    public bool IsElse
    {
        get => _isElse;
        set
        {
            _isElse = value;
            if (value) _whenCondition = null;
        }
    }

    private Expression.Expression? _whenCondition;
    private bool _isElse;

    /// <summary>该分支下的所有 INTO 目标子句。一个分支可触发多个目标。</summary>
    public List<MultiInsertClause> Clauses { get; set; } = new();

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder();
        if (IsElse)
        {
            sb.Append("ELSE ");
        }
        else if (WhenCondition != null)
        {
            sb.Append("WHEN ").Append(WhenCondition).Append(" THEN ");
        }
        foreach (var clause in Clauses)
        {
            sb.Append(clause).Append(' ');
        }
        return sb.ToString();
    }
}

/// <summary>
/// INSERT ALL/FIRST 分支内的单个 INTO 目标子句：
/// <c>INTO table (cols) VALUES (...) </c> 或 <c>INTO table (cols) SELECT ...</c>。
/// 与上游 OracleMultiInsertClause 对齐。
/// </summary>
public class MultiInsertClause : ASTNodeAccessImpl, Model
{
    /// <summary>分支目标表。</summary>
    public Table? Table { get; set; }

    /// <summary>列列表，未指定时为 null。</summary>
    public List<Column>? Columns { get; set; }

    /// <summary>
    /// VALUES 子句的多行值列表，与 <see cref="Select"/> 互斥。
    /// </summary>
    public List<ExpressionList>? ValuesItems { get; set; }

    /// <summary>子查询数据源，与 ValuesItems 互斥。</summary>
    public Select.Select? Select { get; set; }

    /// <summary>是否使用 VALUES 关键字（仅用于 ToString 输出）。</summary>
    public bool UseValues { get; set; } = true;

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder();
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
        return sb.ToString();
    }
}

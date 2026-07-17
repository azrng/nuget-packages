using Azrng.JSqlParser.Models;
using Azrng.JSqlParser.Util;
using Azrng.JSqlParser.Statement.Select;

namespace Azrng.JSqlParser;

/// <summary>
/// 语句 AST 的 C# 风格遍历与信息提取扩展方法。
/// </summary>
/// <remarks>
/// 这些扩展方法是 visitor 体系之上的 C# 风格外壳，用于消除
/// 「new 一个 visitor、调 Accept 传进去、再从字段掏结果」的 Java 式写法。
/// 结构化提取（GetTableReferences/GetSelectColumns）返回中性 DTO，
/// 业务方负责套用自己的产品规则与 DTO 装配。
/// </remarks>
public static class StatementExtension
{
    /// <summary>
    /// 获取语句中引用的所有表名（含 WHERE 子查询内的表，已去重）。
    /// </summary>
    /// <remarks>
    /// 动词统一为 <c>Get</c>，与 <see cref="GetTableReferences"/>/GetSelectColumns/GetWhereConditions 一致。
    /// 替代旧的 <c>new TablesNamesFinder().GetTables(stmt)</c>。
    /// 与 <see cref="GetTableReferences"/> 的区别：本方法返回扁平表名（含 WHERE 子查询），
    /// <see cref="GetTableReferences"/> 返回 FROM/JOIN/CTE 的结构化表引用（带别名/全名）。
    /// </remarks>
    /// <param name="statement">SQL 语句。</param>
    /// <returns>表名只读集合（已去重）。</returns>
    /// <example>
    /// <code>
    /// var stmt = CCJSqlParserUtil.Parse("SELECT u.id FROM users u JOIN orders o ON u.id = o.uid")!;
    /// IReadOnlyCollection&lt;string&gt; tables = stmt.GetTableNames();
    /// // => { "users", "orders" }
    /// </code>
    /// </example>
    public static IReadOnlyCollection<string> GetTableNames(this Statement.Statement statement)
    {
        ArgumentNullException.ThrowIfNull(statement);
        var finder = new TablesNamesFinder();
#pragma warning disable CS0618 // 内部复用成熟遍历，Obsolete 面向外部调用方
        var tables = finder.GetTables(statement);
#pragma warning restore CS0618
        // 返回只读视图，避免调用方误改 finder 内部状态
        return tables;
    }

    /// <summary>已过时，请改用 <see cref="GetTableNames"/>。</summary>
    [Obsolete("改用 GetTableNames()，后续版本将移除")]
    public static IReadOnlyCollection<string> ExtractTableNames(this Statement.Statement statement)
        => statement.GetTableNames();

    /// <summary>
    /// 提取 SELECT 语句中引用的全部表（含别名映射、全限定名）。
    /// </summary>
    /// <remarks>
    /// 仅遍历 FROM/JOIN/CTE 内的表引用；WHERE 表达式中的子查询表不在范围
    /// （需要全部表名含 WHERE 子查询时用 <see cref="GetTableNames"/>，它遍历所有表达式）。
    /// </remarks>
    /// <param name="statement">SQL 语句（仅 SELECT 语句有 FROM 子句，其他语句返回空）。</param>
    /// <returns>
    /// 所有出现的表引用（含自连接的多次出现、CTE 内子查询、JOIN 表）。
    /// 不去重、不做别名优先策略——调用方按 <see cref="TableReference.Key"/> 自行 <c>DistinctBy</c> 或聚合。
    /// </returns>
    /// <example>
    /// <code>
    /// var refs = stmt.GetTableReferences();
    /// // SELECT u.id FROM users u JOIN orders o ON ... => [ {Name:users,Alias:u}, {Name:orders,Alias:o} ]
    /// </code>
    /// </example>
    public static IReadOnlyList<TableReference> GetTableReferences(this Statement.Statement statement)
    {
        ArgumentNullException.ThrowIfNull(statement);
        return TableReferencesExtractor.Extract(statement);
    }

    /// <summary>
    /// 提取 SELECT 语句的结构化列（区分 * / t.* / 列 / 表达式）。
    /// </summary>
    /// <remarks>
    /// 对 UNION/INTERSECT/EXCEPT 的 <see cref="SetOperationList"/>，仅取首个分支的列
    /// （对齐"集合运算的输出列由第一个分支决定"语义）。
    /// 不含虚拟列必填别名校验、来源列推断等产品规则——调用方按 <see cref="SelectColumn.Kind"/> 自行处理。
    /// </remarks>
    /// <example>
    /// <code>
    /// var cols = select.GetSelectColumns();
    /// // SELECT u.id, COUNT(*) AS cnt FROM ... =>
    /// //   [ {Kind:Column,ColumnName:id}, {Kind:Expression,Alias:cnt} ]
    /// </code>
    /// </example>
    public static IReadOnlyList<SelectColumn> GetSelectColumns(this Select select)
    {
        ArgumentNullException.ThrowIfNull(select);
        var plainSelect = select switch
        {
            PlainSelect p => p,
            SetOperationList setOp => setOp.Selects.Count > 0 ? setOp.Selects[0] as PlainSelect : null,
            _ => null
        };
        return SelectColumnsExtractor.Extract(plainSelect);
    }


    /// <summary>
    /// 按深度优先顺序收集语句树中所有类型为 <typeparamref name="TStatement"/> 的语句节点
    /// （含嵌套子语句，如 CTE 内的 ParenthesedInsert、Block 内的语句列表）。
    /// </summary>
    /// <remarks>
    /// 仅遍历语句层；Select body 内的表达式（WHERE、SELECT 列表等）不属于语句节点，
    /// 收集这些请用 <see cref="ExpressionExtension.Descendants{T}"/>。
    /// </remarks>
    public static IEnumerable<TStatement> Descendants<TStatement>(this Statement.Statement statement) where TStatement : Statement.Statement
    {
        ArgumentNullException.ThrowIfNull(statement);
        var matched = new List<TStatement>();
        Statement.StatementDescendantsWalker.Walk(statement, node =>
        {
            if (node is TStatement t) matched.Add(t);
        });
        return matched;
    }
}

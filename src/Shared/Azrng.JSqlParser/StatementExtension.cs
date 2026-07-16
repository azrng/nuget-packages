using Azrng.JSqlParser.Util;

namespace Azrng.JSqlParser;

/// <summary>
/// 语句 AST 的 C# 风格遍历与信息提取扩展方法。
/// </summary>
/// <remarks>
/// 这些扩展方法是 visitor 体系之上的 C# 风格外壳，用于消除
/// 「new 一个 visitor、调 Accept 传进去、再从字段掏结果」的 Java 式写法。
/// </remarks>
public static class StatementExtension
{
    /// <summary>
    /// 提取语句中引用的所有表名。
    /// 替代旧的 <c>new TablesNamesFinder().GetTables(stmt)</c>。
    /// </summary>
    /// <param name="statement">SQL 语句。</param>
    /// <returns>表名只读集合（已去重）。</returns>
    /// <example>
    /// <code>
    /// var stmt = CCJSqlParserUtil.Parse("SELECT u.id FROM users u JOIN orders o ON u.id = o.uid")!;
    /// IReadOnlyCollection&lt;string&gt; tables = stmt.ExtractTableNames();
    /// // => { "users", "orders" }
    /// </code>
    /// </example>
    public static IReadOnlyCollection<string> ExtractTableNames(this Statement.Statement statement)
    {
        ArgumentNullException.ThrowIfNull(statement);
        var finder = new TablesNamesFinder();
#pragma warning disable CS0618 // 内部复用成熟遍历，Obsolete 面向外部调用方
        var tables = finder.GetTables(statement);
#pragma warning restore CS0618
        // 返回只读视图，避免调用方误改 finder 内部状态
        return tables;
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

    /// <summary>
    /// 深度优先遍历语句树，对每个类型为 <typeparamref name="TStatement"/> 的节点调用 <paramref name="action"/>。
    /// </summary>
    public static void Walk<TStatement>(this Statement.Statement statement, Action<TStatement> action) where TStatement : Statement.Statement
    {
        ArgumentNullException.ThrowIfNull(action);
        foreach (var node in statement.Descendants<TStatement>())
            action(node);
    }
}

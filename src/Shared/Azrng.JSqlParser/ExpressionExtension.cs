using Azrng.JSqlParser.Models;
using Azrng.JSqlParser.Util;

namespace Azrng.JSqlParser;

/// <summary>
/// 表达式 AST 的 C# 风格遍历扩展方法。
/// </summary>
/// <remarks>
/// 这些扩展方法是 visitor 体系之上的 LINQ 式外壳，用于消除
/// 「new 一个 visitor、调 Accept 传进去、再从它的字段里掏结果」的 Java 式写法。
/// 底层遍历复用 <see cref="ExpressionVisitorAdapter{T}"/> 已验证的递归逻辑。
/// 复杂自定义遍历仍可直接实现 visitor 接口。
/// </remarks>
public static class ExpressionExtension
{
    /// <summary>
    /// 按深度优先顺序收集表达式中所有类型为 <typeparamref name="T"/> 的后代节点。
    /// 替代旧的「自定义 visitor + Accept + 掏字段」三步写法。
    /// </summary>
    /// <typeparam name="T">要收集的表达式节点类型。</typeparam>
    /// <param name="expression">根表达式。</param>
    /// <returns>匹配的后代节点序列，可直接接 LINQ（<c>Select</c>/<c>Where</c>/<c>Distinct</c> 等）。</returns>
    /// <example>
    /// <code>
    /// // 旧写法：定义 ColumnCollector 类 + new + Accept + 掏 Columns
    /// // 新写法：一行
    /// var columns = whereClause.Descendants&lt;Column&gt;().ToList();
    /// var paramNames = expr.Descendants&lt;JdbcNamedParameter&gt;().Select(p =&gt; p.Name).ToList();
    /// </code>
    /// </example>
    public static IEnumerable<T> Descendants<T>(this Expression.Expression expression) where T : Expression.Expression
    {
        ArgumentNullException.ThrowIfNull(expression);
        var matched = new List<T>();
        ExpressionDescendantsWalker.Walk(expression, node =>
        {
            if (node is T t) matched.Add(t);
        });
        return matched;
    }

    /// <summary>
    /// 按深度优先顺序收集表达式中所有后代节点（不限类型）。
    /// </summary>
    public static IEnumerable<Expression.Expression> Descendants(this Expression.Expression expression)
        => expression.Descendants<Expression.Expression>();

    /// <summary>
    /// 深度优先遍历表达式，对每个类型为 <typeparamref name="T"/> 的后代节点调用 <paramref name="action"/>。
    /// 用于「遍历即可、不想要集合」的就地处理场景。
    /// </summary>
    public static void Walk<T>(this Expression.Expression expression, Action<T> action) where T : Expression.Expression
    {
        ArgumentNullException.ThrowIfNull(action);
        foreach (var node in expression.Descendants<T>())
            action(node);
    }

    /// <summary>
    /// 把 WHERE 表达式的 AND/OR 树拍平为条件列表。
    /// </summary>
    /// <remarks>
    /// <b>通用化设计</b>，不随运算符枚举膨胀：
    /// <list type="bullet">
    /// <item>逻辑连接符（AND/OR/括号）按结构递归。</item>
    /// <item>所有二元运算符（继承 <c>BinaryExpression</c>，含 =、&gt;、LIKE、!=、加减乘除等）统一提取，
    /// 新增二元运算符自动覆盖。</item>
    /// <item>IN/BETWEEN 结构特殊单独处理。</item>
    /// <item>未匹配的叶子（IS NULL/EXISTS 等单目运算符）兜底提取为单目条件（<c>RightExpression</c> 为 null），
    /// 不静默丢弃。</item>
    /// </list>
    /// 不含列归属反查、参数收集等产品逻辑——业务方按字段自行映射到业务 DTO。
    /// </remarks>
    /// <param name="where">WHERE 表达式（通常是 <c>PlainSelect.Where</c>），为 null 时返回空列表。</param>
    /// <example>
    /// <code>
    /// var conds = select.Where!.GetWhereConditions();
    /// // WHERE a = 1 AND b > 2 =>
    /// //   [ {LinkType:"",Op:"=",Left:a,Right:1}, {LinkType:"AND",Op:"&gt;",Left:b,Right:2} ]
    /// </code>
    /// </example>
    public static IReadOnlyList<WhereCondition> GetWhereConditions(this Expression.Expression where)
    {
        return WhereConditionsExtractor.Extract(where);
    }
}

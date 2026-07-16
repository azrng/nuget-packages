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
}

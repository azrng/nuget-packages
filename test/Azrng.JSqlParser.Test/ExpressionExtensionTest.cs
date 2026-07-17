using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Expression.Operators.Arithmetic;
using Azrng.JSqlParser.Expression.Operators.Conditional;
using Azrng.JSqlParser.Expression.Operators.Relational;
using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Schema;
using PlainSelect = Azrng.JSqlParser.Statement.Select.PlainSelect;
using JExpression = Azrng.JSqlParser.Expression.Expression;

namespace Azrng.JSqlParser.Test;

/// <summary>
/// 表达式扩展方法（Descendants/Walk）测试。
/// 核心目标：验证「一行扩展方法」与旧「自定义 visitor + Accept + 掏字段」结果等价，
/// 消除 LocalSqlParser 中 ColumnCollector / ParameterCollector 这类副作用式写法。
/// </summary>
public class ExpressionExtensionTest
{
    /// <summary>解析 WHERE 表达式。用 SELECT 包裹再取 Where，避免顶层表达式解析的歧义。</summary>
    private static JExpression ParseWhere(string sql)
    {
        var stmt = (PlainSelect)CCJSqlParserUtil.Parse(sql)!;
        return stmt.Where!;
    }

    // ---------- Descendants<T>：与旧 ColumnCollector 等价 ----------

    [Fact]
    public void Descendants_OfColumn_ShouldEqualLegacyColumnCollector()
    {
        var where = ParseWhere("SELECT id FROM users WHERE name = 'test' AND age > 18");

        // 新写法：一行
        var viaExtension = where.Descendants<Column>().Select(c => c.ColumnName).ToList();

        // 旧写法：定义类 + new + Accept + 掏字段
        var legacy = new LegacyColumnCollector();
        where.Accept(legacy, (object?)null);
        var viaLegacy = legacy.Columns.Select(c => c.ColumnName).ToList();

        Assert.Equal(viaLegacy, viaExtension);
        Assert.Contains("name", viaExtension);
        Assert.Contains("age", viaExtension);
    }

    [Fact]
    public void Descendants_OfColumn_ShouldCollectAllColumnsInNestedExpression()
    {
        // age + 1 这种算术表达式内的列也要能收集到（验证递归穿透 BinaryExpression）
        var where = ParseWhere("SELECT id FROM t WHERE a + b > c");
        var columns = where.Descendants<Column>().Select(c => c.ColumnName).OrderBy(n => n).ToList();
        Assert.Equal(new[] { "a", "b", "c" }, columns);
    }

    [Fact]
    public void Descendants_OfColumn_ShouldTraverseFunctionArguments()
    {
        // 函数参数内的列引用要能收集到（验证递归穿透 Function）
        var where = ParseWhere("SELECT id FROM t WHERE UPPER(name) = 'X'");
        var columns = where.Descendants<Column>().Select(c => c.ColumnName).ToList();
        Assert.Contains("name", columns);
    }

    [Fact]
    public void Descendants_OfColumn_ShouldTraverseCaseExpression()
    {
        var where = ParseWhere("SELECT id FROM t WHERE CASE WHEN a > 1 THEN b ELSE c END > 0");
        var columns = where.Descendants<Column>().Select(c => c.ColumnName).OrderBy(n => n).ToList();
        Assert.Contains("a", columns);
        Assert.Contains("b", columns);
        Assert.Contains("c", columns);
    }

    // ---------- Descendants<T>：与旧 ParameterCollector 等价（复刻 LocalSqlParser 场景）----------

    [Fact]
    public void Descendants_OfJdbcNamedParameter_ShouldEqualLegacyParameterCollector()
    {
        var where = ParseWhere("SELECT id FROM t WHERE p1 = :p1 OR p2 = :p2");

        // 新写法
        var viaExtension = where.Descendants<JdbcNamedParameter>()
            .Select(p => p.Name)
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .ToList();

        // 旧写法
        var legacy = new LegacyParameterCollector();
        where.Accept(legacy, (object?)null);
        var viaLegacy = legacy.Parameters;

        Assert.Equal(viaLegacy.OrderBy(n => n), viaExtension.OrderBy(n => n));
    }

    // ---------- Descendants（无泛型）----------

    [Fact]
    public void Descendants_AllNodes_ShouldReturnNonEmpty()
    {
        var where = ParseWhere("SELECT id FROM t WHERE a > 1");
        var all = where.Descendants().ToList();
        Assert.NotEmpty(all);
        // 至少应包含列 a 和字面量 1（GreaterThan + Column + LongValue）
        Assert.Contains(all, n => n is Column);
    }

    // ---------- Walk<T>：推送委托 ----------

    [Fact]
    public void Walk_OfColumn_ShouldInvokeActionForEachMatchedNode()
    {
        var where = ParseWhere("SELECT id FROM t WHERE a > 1 AND b < 2");
        var collected = new List<string>();
        where.Walk<Column>(c => collected.Add(c.ColumnName));
        Assert.Equal(new[] { "a", "b" }, collected.OrderBy(n => n));
    }

    [Fact]
    public void Walk_ShouldNotInvokeActionForNonMatchingTypes()
    {
        var where = ParseWhere("SELECT id FROM t WHERE a > 1");
        var columnCalls = 0;
        // 不存在 Function 节点，回调不应被触发
        where.Walk<Function>(_ => columnCalls++);
        Assert.Equal(0, columnCalls);
    }

    [Fact]
    public void Descendants_OnNullExpression_ShouldThrow()
    {
        JExpression expr = null!;
        Assert.Throws<ArgumentNullException>(() => expr.Descendants<Column>());
    }

    // ---------- 关系运算符与复合表达式分支覆盖 ----------

    [Fact]
    public void Descendants_OfColumn_ShouldTraverseBetween()
    {
        var where = ParseWhere("SELECT id FROM t WHERE age BETWEEN 18 AND 65");
        var columns = where.Descendants<Column>().Select(c => c.ColumnName).ToList();
        Assert.Equal(new[] { "age" }, columns);
    }

    [Fact]
    public void Descendants_OfColumn_ShouldTraverseInExpression()
    {
        var where = ParseWhere("SELECT id FROM t WHERE id IN (1, 2, 3)");
        var columns = where.Descendants<Column>().Select(c => c.ColumnName).ToList();
        Assert.Equal(new[] { "id" }, columns);
    }

    [Fact]
    public void Descendants_OfColumn_ShouldTraverseParenthesisAndNot()
    {
        // NOT (a = 1 AND b = 2) —— 验证 NotExpression + Parenthesis 递归穿透
        var where = ParseWhere("SELECT id FROM t WHERE NOT (a = 1 AND b = 2)");
        var columns = where.Descendants<Column>().Select(c => c.ColumnName).OrderBy(n => n).ToList();
        Assert.Equal(new[] { "a", "b" }, columns);
    }

    [Fact]
    public void Descendants_OfColumn_ShouldTraverseCastExpression()
    {
        var where = ParseWhere("SELECT id FROM t WHERE CAST(age AS INT) > 18");
        var columns = where.Descendants<Column>().Select(c => c.ColumnName).ToList();
        Assert.Equal(new[] { "age" }, columns);
    }

    [Fact]
    public void Descendants_OfColumn_ShouldTraverseExistsSubqueryExpression()
    {
        // EXISTS 内是子查询（Select），表达式层不穿透 Select body，仅收集 EXISTS 外层列
        var where = ParseWhere("SELECT id FROM t WHERE flag = 1 AND EXISTS (SELECT 1 FROM s)");
        var columns = where.Descendants<Column>().Select(c => c.ColumnName).ToList();
        Assert.Contains("flag", columns);
    }

    [Fact]
    public void Descendants_OfColumn_ShouldTraverseOrAndMixed()
    {
        var where = ParseWhere("SELECT id FROM t WHERE a = 1 OR (b = 2 AND c = 3)");
        var columns = where.Descendants<Column>().Select(c => c.ColumnName).OrderBy(n => n).ToList();
        Assert.Equal(new[] { "a", "b", "c" }, columns);
    }

    // ---------- 语义正确性：去重 / 类型过滤 / 空结果 ----------

    [Fact]
    public void Descendants_ShouldPreserveDuplicatesAsTheyAppear()
    {
        // 同一列出现多次，应按实际出现次数返回（去重由调用方用 Distinct 决定）
        var where = ParseWhere("SELECT id FROM t WHERE a = 1 OR a = 2");
        var columns = where.Descendants<Column>().Select(c => c.ColumnName).ToList();
        Assert.Equal(2, columns.Count);
        Assert.Equal(new[] { "a", "a" }, columns);
    }

    [Fact]
    public void Descendants_DistinctByCaller_ShouldDeduplicate()
    {
        var where = ParseWhere("SELECT id FROM t WHERE a = 1 OR a = 2");
        var columns = where.Descendants<Column>().Select(c => c.ColumnName).Distinct().ToList();
        Assert.Equal(new[] { "a" }, columns);
    }

    [Fact]
    public void Descendants_OfNonExistentType_ShouldReturnEmpty()
    {
        var where = ParseWhere("SELECT id FROM t WHERE a = 1");
        // WHERE 中没有 Function 节点
        Assert.Empty(where.Descendants<Function>());
    }

    [Fact]
    public void Descendants_ShouldBeEnumerableForLinqChaining()
    {
        var where = ParseWhere("SELECT id FROM t WHERE a = 1 AND b = 2");
        // 验证返回值可直接接 LINQ 且支持多次枚举（结果已物化）
        var query = where.Descendants<Column>();
        var firstCount = query.Count();
        var secondCount = query.Count();
        Assert.Equal(firstCount, secondCount);
        Assert.Equal(2, firstCount);
    }

    // ---------- Walk 边界 ----------

    [Fact]
    public void Walk_OnNullExpression_ShouldThrow()
    {
        JExpression expr = null!;
        Assert.Throws<ArgumentNullException>(() => expr.Walk<Column>(_ => { }));
    }

    [Fact]
    public void Walk_OnNullAction_ShouldThrow()
    {
        var where = ParseWhere("SELECT id FROM t WHERE a = 1");
        Assert.Throws<ArgumentNullException>(() => where.Walk<Column>(null!));
    }

    // ---------- 此前漏覆盖的节点类型（隐患2 修复回归）----------

    [Fact]
    public void Descendants_OfTrimFunction_ShouldBeCollected()
    {
        // TrimFunction 此前仅接口默认实现、Adapter 未 override，Descendants<TrimFunction> 静默返回空。
        // walker 改为直接实现接口后，应能收集到。
        var where = ParseWhere("SELECT id FROM t WHERE TRIM(a) = 'x'");
        var trims = where.Descendants<TrimFunction>().ToList();
        Assert.Single(trims);
    }

    [Fact]
    public void Descendants_OfCollateExpression_ShouldBeCollected()
    {
        var where = ParseWhere("SELECT id FROM t WHERE a COLLATE utf8_bin = 'x'");
        var collates = where.Descendants<CollateExpression>().ToList();
        Assert.Single(collates);
    }

    // ---------- 旧式 visitor（仅用于等价验证对照，非推荐写法）----------

    private sealed class LegacyColumnCollector : ExpressionVisitorAdapter<object?>
    {
        public List<Column> Columns { get; } = new();
        public override object? Visit<S>(Column column, S context)
        {
            Columns.Add(column);
            return base.Visit(column, context);
        }
    }

    private sealed class LegacyParameterCollector : ExpressionVisitorAdapter<object?>
    {
        public List<string> Parameters { get; } = new();
        public override object? Visit<S>(JdbcNamedParameter jdbcNamedParameter, S context)
        {
            if (!string.IsNullOrWhiteSpace(jdbcNamedParameter.Name))
                Parameters.Add(jdbcNamedParameter.Name);
            return base.Visit(jdbcNamedParameter, context);
        }
    }
}

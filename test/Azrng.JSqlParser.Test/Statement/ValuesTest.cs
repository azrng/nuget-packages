using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Schema;
using Azrng.JSqlParser.Statement.Select;

namespace Azrng.JSqlParser.Test.Statement;

/// <summary>
/// VALUES 表构造器测试（T097）。
/// 对齐上游 net.sf.jsqlparser.statement.select.Values（commit 2b141568）。
/// 覆盖三档：独立 SELECT 语句、集合运算操作数、FROM 子项。
/// </summary>
public class ValuesTest
{
    /// <summary>独立 VALUES 语句：VALUES (1, 'a'), (2, 'b') round-trip。</summary>
    [Fact]
    public void Values_Standalone_RoundTrip()
    {
        var sql = "VALUES (1, 'a'), (2, 'b')";
        var stmt = SqlParser.Parse(sql);

        var values = Assert.IsType<Values>(stmt);
        Assert.Equal(2, values.Rows.Count);
        Assert.Equal(2, values.Rows[0].Expressions.Count);
        Assert.Equal(sql, stmt!.ToString());
    }

    /// <summary>单行 VALUES round-trip。</summary>
    [Fact]
    public void Values_SingleRow_RoundTrip()
    {
        var sql = "VALUES (1, 2, 3)";
        var stmt = SqlParser.Parse(sql);

        var values = Assert.IsType<Values>(stmt);
        Assert.Single(values.Rows);
        Assert.Equal(3, values.Rows[0].Expressions.Count);
        Assert.Equal(sql, stmt!.ToString());
    }

    /// <summary>VALUES 作为 UNION 操作数：VALUES (1) UNION VALUES (2)。</summary>
    [Fact]
    public void Values_Union_Values_RoundTrip()
    {
        var sql = "VALUES (1) UNION VALUES (2)";
        var stmt = SqlParser.Parse(sql);

        var setOpList = Assert.IsType<SetOperationList>(stmt);
        Assert.Equal(2, setOpList.Selects.Count);
        Assert.All(setOpList.Selects, s => Assert.IsType<Values>(s));
        Assert.Equal(sql, stmt!.ToString());
    }

    /// <summary>VALUES UNION ALL VALUES round-trip。</summary>
    [Fact]
    public void Values_UnionAll_Values_RoundTrip()
    {
        var sql = "VALUES (1, 'x') UNION ALL VALUES (2, 'y')";
        var stmt = SqlParser.Parse(sql);

        var setOpList = Assert.IsType<SetOperationList>(stmt);
        Assert.Equal(2, setOpList.Selects.Count);
        Assert.Equal(sql, stmt!.ToString());
    }

    /// <summary>FROM 子项带别名（PostgreSQL 风格）：SELECT * FROM (VALUES (1, 2)) AS t。
    /// 注：ParenthesedSelect 的 Alias 序列化不含 AS（既定行为），round-trip 输出为 ") t"。</summary>
    [Fact]
    public void Values_AsFromItem_WithAlias_RoundTrip()
    {
        var stmt = SqlParser.Parse("SELECT * FROM (VALUES (1, 2)) AS t");

        var plainSelect = Assert.IsType<PlainSelect>(stmt);
        var parenthesedSelect = Assert.IsType<ParenthesedSelect>(plainSelect.IFromItem);
        Assert.IsType<Values>(parenthesedSelect.Select);
        Assert.NotNull(parenthesedSelect.Alias);
        Assert.Equal("SELECT * FROM (VALUES (1, 2)) t", stmt!.ToString());
    }

    /// <summary>FROM 子项：SELECT * FROM (VALUES (1), (2)) t。</summary>
    [Fact]
    public void Values_AsFromItem_MultiRow_RoundTrip()
    {
        var sql = "SELECT * FROM (VALUES (1), (2)) t";
        var stmt = SqlParser.Parse(sql);

        var plainSelect = Assert.IsType<PlainSelect>(stmt);
        var parenthesedSelect = Assert.IsType<ParenthesedSelect>(plainSelect.IFromItem);
        var values = Assert.IsType<Values>(parenthesedSelect.Select);
        Assert.Equal(2, values.Rows.Count);
        Assert.Equal(sql, stmt!.ToString());
    }

    /// <summary>VALUES 语句带 ORDER BY / LIMIT 修饰符。</summary>
    [Fact]
    public void Values_WithOrderByAndLimit_RoundTrip()
    {
        var sql = "VALUES (1), (2) ORDER BY 1 LIMIT 1";
        var stmt = SqlParser.Parse(sql);

        var values = Assert.IsType<Values>(stmt);
        Assert.NotNull(values.OrderByElements);
        Assert.NotNull(values.Limit);
        Assert.Equal(sql, stmt!.ToString());
    }

    /// <summary>VALUES 行内含表达式（函数、运算），非仅字面量。</summary>
    [Fact]
    public void Values_WithExpressions_RoundTrip()
    {
        var sql = "VALUES (1 + 1, UPPER('a')), (ABS(-3), 'b')";
        var stmt = SqlParser.Parse(sql);

        var values = Assert.IsType<Values>(stmt);
        Assert.Equal(2, values.Rows.Count);
        Assert.Equal(sql, stmt!.ToString());
    }

    /// <summary>NULL 值在 VALUES 行中正确解析。</summary>
    [Fact]
    public void Values_WithNull_RoundTrip()
    {
        var sql = "VALUES (1, NULL), (NULL, 2)";
        var stmt = SqlParser.Parse(sql);

        var values = Assert.IsType<Values>(stmt);
        Assert.Equal(2, values.Rows.Count);
        Assert.Equal(sql, stmt!.ToString());
    }

    // ===== 集合运算修饰符覆盖（INTERSECT / EXCEPT / MINUS / CORRESPONDING）=====

    /// <summary>VALUES INTERSECT VALUES round-trip。</summary>
    [Fact]
    public void Values_Intersect_RoundTrip()
    {
        var sql = "VALUES (1) INTERSECT VALUES (2)";
        var stmt = SqlParser.Parse(sql);

        var setOpList = Assert.IsType<SetOperationList>(stmt);
        Assert.Equal(2, setOpList.Selects.Count);
        Assert.Equal(SetOperation.OperationType.INTERSECT, setOpList.Operations[0].Type);
        Assert.Equal(sql, stmt!.ToString());
    }

    /// <summary>VALUES EXCEPT VALUES round-trip。</summary>
    [Fact]
    public void Values_Except_RoundTrip()
    {
        var sql = "VALUES (1) EXCEPT VALUES (2)";
        var stmt = SqlParser.Parse(sql);

        var setOpList = Assert.IsType<SetOperationList>(stmt);
        Assert.Equal(SetOperation.OperationType.EXCEPT, setOpList.Operations[0].Type);
        Assert.Equal(sql, stmt!.ToString());
    }

    /// <summary>VALUES MINUS VALUES round-trip（Oracle 风格集合差）。</summary>
    [Fact]
    public void Values_Minus_RoundTrip()
    {
        var sql = "VALUES (1) MINUS VALUES (2)";
        var stmt = SqlParser.Parse(sql);

        var setOpList = Assert.IsType<SetOperationList>(stmt);
        Assert.Equal(SetOperation.OperationType.MINUS, setOpList.Operations[0].Type);
        Assert.Equal(sql, stmt!.ToString());
    }

    /// <summary>UNION CORRESPONDING 修饰符 round-trip。</summary>
    [Fact]
    public void Values_UnionCorresponding_RoundTrip()
    {
        var sql = "VALUES (1) UNION CORRESPONDING VALUES (2)";
        var stmt = SqlParser.Parse(sql);

        var setOpList = Assert.IsType<SetOperationList>(stmt);
        Assert.True(setOpList.Operations[0].Corresponding);
        Assert.Equal(sql, stmt!.ToString());
    }

    /// <summary>三路 VALUES UNION round-trip（验证 setOperator 循环正确）。</summary>
    [Fact]
    public void Values_ThreeWay_Union_RoundTrip()
    {
        var sql = "VALUES (1) UNION VALUES (2) UNION VALUES (3)";
        var stmt = SqlParser.Parse(sql);

        var setOpList = Assert.IsType<SetOperationList>(stmt);
        Assert.Equal(3, setOpList.Selects.Count);
        Assert.Equal(2, setOpList.Operations.Count);
        Assert.All(setOpList.Selects, s => Assert.IsType<Values>(s));
        Assert.Equal(sql, stmt!.ToString());
    }

    // ===== 修饰符（OFFSET / FETCH）=====

    /// <summary>VALUES 带 OFFSET 修饰符（继承自 Select 基类）。
    /// 注：Offset 既有序列化不含 ROWS 关键字，此处验证 OFFSET 能正确挂到 Values 而非 round-trip。</summary>
    [Fact]
    public void Values_WithOffset_ParsedAndAttached()
    {
        var stmt = SqlParser.Parse("VALUES (1), (2) OFFSET 1 ROWS");

        var values = Assert.IsType<Values>(stmt);
        Assert.NotNull(values.Offset);
        Assert.NotNull(values.Offset!.OffsetExpression);
    }

    // ===== FROM 子项边界 =====

    /// <summary>FROM 子项无别名：SELECT * FROM (VALUES (1, 2))（裸 VALUES 子查询）。</summary>
    [Fact]
    public void Values_AsFromItem_NoAlias_RoundTrip()
    {
        var sql = "SELECT * FROM (VALUES (1, 2))";
        var stmt = SqlParser.Parse(sql);

        var plainSelect = Assert.IsType<PlainSelect>(stmt);
        var parenthesedSelect = Assert.IsType<ParenthesedSelect>(plainSelect.IFromItem);
        Assert.IsType<Values>(parenthesedSelect.Select);
        Assert.Null(parenthesedSelect.Alias);
        Assert.Equal(sql, stmt!.ToString());
    }

    /// <summary>VALUES 内含列引用（非字面量）round-trip。</summary>
    [Fact]
    public void Values_WithColumnReference_RoundTrip()
    {
        var sql = "VALUES (users.id, orders.code)";
        var stmt = SqlParser.Parse(sql);

        var values = Assert.IsType<Values>(stmt);
        Assert.Single(values.Rows);
        Assert.Equal(2, values.Rows[0].Expressions.Count);
        Assert.Equal(sql, stmt!.ToString());
    }

    // ===== 程序化构造（模型类作为 API）=====

    /// <summary>手动构造 Values 对象并序列化，验证模型类可独立于解析器使用。</summary>
    [Fact]
    public void Values_ProgrammaticConstruction_SerializesCorrectly()
    {
        var values = new Values
        {
            Rows = new()
            {
                new ExpressionList { Expressions = new() { new LongValue(1), new StringValue("a") } },
                new ExpressionList { Expressions = new() { new LongValue(2), new StringValue("b") } }
            }
        };

        Assert.Equal("VALUES (1, 'a'), (2, 'b')", values.ToString());
    }

    /// <summary>空 Rows 的 Values 序列化为 "VALUES "（边界）。</summary>
    [Fact]
    public void Values_EmptyRows_SerializesToValuesKeyword()
    {
        var values = new Values();
        Assert.Equal("VALUES ", values.ToString());
    }

    /// <summary>Values 实现 IFromItem 接口的 Alias 属性读写。</summary>
    [Fact]
    public void Values_FromItem_AliasGetSet()
    {
        var values = new Values();
        Assert.Null(values.Alias);

        var alias = new Alias("t");
        values.Alias = alias;
        Assert.Same(alias, values.Alias);
    }
}

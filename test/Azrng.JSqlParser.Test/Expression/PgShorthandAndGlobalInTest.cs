using Azrng.JSqlParser.Expression.Operators.Relational;
using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Schema;
using PlainSelect = Azrng.JSqlParser.Statement.Select.PlainSelect;

namespace Azrng.JSqlParser.Test.Expression;

/// <summary>
/// PostgreSQL 简写 NULL 检查（ISNULL/NOTNULL）+ ClickHouse GLOBAL IN 测试。
/// 对齐上游 IsNullExpression.useIsNull/useNotNull + InExpression.global。
/// </summary>
public class PgShorthandAndGlobalInTest
{
    #region PostgreSQL ISNULL / NOTNULL 简写

    [Fact]
    public void IsNullExpression_Default_ShouldRenderIsNull()
    {
        var expr = new IsNullExpression { LeftExpression = new Column { ColumnName = "x" } };
        Assert.Equal("x IS NULL", expr.ToString());
    }

    [Fact]
    public void IsNullExpression_Not_ShouldRenderIsNotNull()
    {
        var expr = new IsNullExpression { LeftExpression = new Column { ColumnName = "x" }, Not = true };
        Assert.Equal("x IS NOT NULL", expr.ToString());
    }

    [Fact]
    public void IsNullExpression_UseIsNull_ShouldRenderPgShorthand()
    {
        // PG 简写：x ISNULL
        var expr = new IsNullExpression { LeftExpression = new Column { ColumnName = "x" }, UseIsNull = true };
        Assert.Equal("x ISNULL", expr.ToString());
    }

    [Fact]
    public void IsNullExpression_UseNotNull_ShouldRenderPgShorthand()
    {
        // PG 简写：x NOTNULL（注意 NOTNULL 不带空格，对齐上游 IsNullExpression.java:74-76）
        var expr = new IsNullExpression { LeftExpression = new Column { ColumnName = "x" }, UseNotNull = true };
        Assert.Equal("x NOTNULL", expr.ToString());
    }

    [Fact]
    public void Parse_PgIsNullShortHand_ShouldRoundTrip()
    {
        // 解析 x ISNULL 应保留简写形式（此前回写成 IS NULL）
        var stmt = (PlainSelect)SqlParser.Parse("SELECT * FROM t WHERE x ISNULL")!;
        var where = stmt.Where;
        Assert.IsType<IsNullExpression>(where);
        Assert.True(((IsNullExpression)where!).UseIsNull);
        Assert.Contains("ISNULL", stmt.ToString()!);
    }

    [Fact]
    public void Parse_PgNotNullShortHand_ShouldRoundTrip()
    {
        // 解析 x NOTNULL 应保留简写形式（此前回写成 IS NOT NULL，且 Not=true）
        var stmt = (PlainSelect)SqlParser.Parse("SELECT * FROM t WHERE x NOTNULL")!;
        var where = stmt.Where;
        Assert.IsType<IsNullExpression>(where);
        Assert.True(((IsNullExpression)where!).UseNotNull);
        Assert.Contains("NOTNULL", stmt.ToString()!);
    }

    [Fact]
    public void Parse_StandardIsNull_StillWork()
    {
        // 标准 IS NULL 不受 PG 简写影响
        var stmt = (PlainSelect)SqlParser.Parse("SELECT * FROM t WHERE x IS NULL")!;
        var where = (IsNullExpression)stmt.Where!;
        Assert.False(where.UseIsNull);
        Assert.False(where.UseNotNull);
        Assert.False(where.Not);
        Assert.Equal("x IS NULL", where.ToString());
    }

    [Fact]
    public void Parse_StandardIsNotNull_StillWork()
    {
        var stmt = (PlainSelect)SqlParser.Parse("SELECT * FROM t WHERE x IS NOT NULL")!;
        var where = (IsNullExpression)stmt.Where!;
        Assert.False(where.UseIsNull);
        Assert.False(where.UseNotNull);
        Assert.True(where.Not);
        Assert.Equal("x IS NOT NULL", where.ToString());
    }

    #endregion

    #region ClickHouse GLOBAL IN / GLOBAL NOT IN

    [Fact]
    public void Parse_GlobalIn_ShouldSetGlobalFlag()
    {
        // ClickHouse GLOBAL IN：对齐上游 InExpression.global
        var stmt = (PlainSelect)SqlParser.Parse("SELECT * FROM t WHERE id GLOBAL IN (1, 2, 3)")!;
        var where = (InExpression)stmt.Where!;
        Assert.True(where.Global);
        Assert.False(where.Not);
        Assert.Contains("GLOBAL IN", stmt.ToString()!);
    }

    [Fact]
    public void Parse_GlobalNotIn_ShouldSetGlobalAndNotFlags()
    {
        var stmt = (PlainSelect)SqlParser.Parse("SELECT * FROM t WHERE id GLOBAL NOT IN (1, 2, 3)")!;
        var where = (InExpression)stmt.Where!;
        Assert.True(where.Global);
        Assert.True(where.Not);
        Assert.Contains("GLOBAL NOT IN", stmt.ToString()!);
    }

    [Fact]
    public void Parse_GlobalInSubquery_ShouldSetGlobalFlag()
    {
        // GLOBAL IN 子查询
        var stmt = (PlainSelect)SqlParser.Parse("SELECT * FROM t WHERE id GLOBAL IN (SELECT id FROM s)")!;
        var where = (InExpression)stmt.Where!;
        Assert.True(where.Global);
        Assert.Contains("GLOBAL IN", stmt.ToString()!);
    }

    [Fact]
    public void Parse_StandardIn_NotGlobal()
    {
        // 标准 IN 不受影响
        var stmt = (PlainSelect)SqlParser.Parse("SELECT * FROM t WHERE id IN (1, 2)")!;
        var where = (InExpression)stmt.Where!;
        Assert.False(where.Global);
        Assert.DoesNotContain("GLOBAL", stmt.ToString()!);
    }

    [Fact]
    public void InExpression_ToString_GlobalNotIn_Programmatic()
    {
        // 程序化构造 GLOBAL NOT IN：复用解析得到的 ExpressionList
        var parsed = (InExpression)((PlainSelect)SqlParser.Parse("SELECT * FROM t WHERE id IN (1, 2)")!).Where!;
        var expr = new InExpression
        {
            LeftExpression = parsed.LeftExpression,
            RightExpression = parsed.RightExpression,
            Global = true,
            Not = true
        };
        Assert.Contains("GLOBAL NOT IN", expr.ToString());
    }

    #endregion
}

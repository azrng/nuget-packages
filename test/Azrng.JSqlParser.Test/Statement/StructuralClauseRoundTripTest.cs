using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Statement.Select;
using Select = Azrng.JSqlParser.Statement.Select.Select;

namespace Azrng.JSqlParser.Test.Statement;

/// <summary>
/// 结构性子句 round-trip 测试 —— 覆盖 grammar 已解析、但 AstBuilderVisitor 此前沉默丢弃的子句。
/// 与 LIKE/正则运算符 bug 同型（grammar 接线了 visitor 不赋值），逐项补回归。
/// </summary>
public class StructuralClauseRoundTripTest
{
    #region ORDER BY NULLS FIRST/LAST

    [Fact]
    public void OrderBy_NullsFirst_ShouldRoundTrip()
    {
        // grammar 一直解析 NULLS，但 visitor 此前不读 context.NULLS()，
        // 导致 OrderByElement.NullOrder 恒为 null，round-trip 丢失 NULLS FIRST。
        var stmt = (PlainSelect)SqlParser.Parse("SELECT id FROM users ORDER BY name NULLS FIRST")!;
        var orderBy = Assert.Single(stmt.OrderByElements!);
        Assert.Equal(OrderByElement.NullOrdering.NULLS_FIRST, orderBy.NullOrder);
        Assert.Contains("NULLS FIRST", stmt.ToString()!);
    }

    [Fact]
    public void OrderBy_NullsLast_ShouldRoundTrip()
    {
        var stmt = (PlainSelect)SqlParser.Parse("SELECT id FROM users ORDER BY name NULLS LAST")!;
        var orderBy = Assert.Single(stmt.OrderByElements!);
        Assert.Equal(OrderByElement.NullOrdering.NULLS_LAST, orderBy.NullOrder);
        Assert.Contains("NULLS LAST", stmt.ToString()!);
    }

    [Fact]
    public void OrderBy_DescNullsFirst_ShouldRoundTrip()
    {
        // DESC + NULLS FIRST 组合
        var stmt = (PlainSelect)SqlParser.Parse("SELECT id FROM users ORDER BY name DESC NULLS FIRST")!;
        var orderBy = Assert.Single(stmt.OrderByElements!);
        Assert.False(orderBy.Asc);
        Assert.Equal(OrderByElement.NullOrdering.NULLS_FIRST, orderBy.NullOrder);
        Assert.Contains("DESC NULLS FIRST", stmt.ToString()!);
    }

    [Fact]
    public void OrderBy_NoNulls_ShouldHaveNullNullOrder()
    {
        // 未指定 NULLS 时 NullOrder 保持 null
        var stmt = (PlainSelect)SqlParser.Parse("SELECT id FROM users ORDER BY name")!;
        var orderBy = Assert.Single(stmt.OrderByElements!);
        Assert.Null(orderBy.NullOrder);
    }

    #endregion

    #region WITH RECURSIVE

    [Fact]
    public void WithRecursive_SingleCte_ShouldRoundTrip()
    {
        // PostgreSQL/SQLite 递归 CTE：WITH RECURSIVE t(n) AS (...) SELECT ...
        // grammar 一直解析 RECURSIVE，但 visitor 此前不读 context.RECURSIVE()，
        // 导致 WithItem.Recursive 恒为 false，round-trip 丢失 RECURSIVE 关键字。
        var sql = "WITH RECURSIVE t(n) AS (SELECT 1) SELECT n FROM t";
        var stmt = SqlParser.Parse(sql)!;
        Assert.All(((Select)stmt).WithItemsList!, w => Assert.True(w.Recursive));
        Assert.Contains("WITH RECURSIVE", stmt.ToString()!);
    }

    [Fact]
    public void WithRecursive_MultipleCtes_RecursiveKeywordEmittedOnce()
    {
        // SQL 标准：WITH RECURSIVE 后跟多个 CTE 时，RECURSIVE 只出现一次（紧跟 WITH 之后）。
        // 此前 WithItem.AppendSelectBodyTo 每个 CTE 都前缀 RECURSIVE，输出 WITH RECURSIVE a RECURSIVE b（错）。
        var sql = "WITH RECURSIVE t1(n) AS (SELECT 1), t2(n) AS (SELECT 2) SELECT n FROM t1";
        var stmt = SqlParser.Parse(sql)!;
        var rendered = stmt.ToString()!;
        // 只允许出现一次 RECURSIVE
        Assert.Equal(1, CountOccurrences(rendered, "RECURSIVE"));
        // 所有 WithItem 标记为 Recursive
        Assert.All(((Select)stmt).WithItemsList!, w => Assert.True(w.Recursive));
    }

    [Fact]
    public void With_NoRecursive_RecursiveFlagFalse()
    {
        var stmt = (Select)SqlParser.Parse("WITH t(n) AS (SELECT 1) SELECT n FROM t")!;
        Assert.All(stmt.WithItemsList!, w => Assert.False(w.Recursive));
        Assert.DoesNotContain("RECURSIVE", stmt.ToString()!);
    }

    private static int CountOccurrences(string haystack, string needle)
    {
        int count = 0, i = 0;
        while ((i = haystack.IndexOf(needle, i, StringComparison.Ordinal)) >= 0)
        {
            count++;
            i += needle.Length;
        }
        return count;
    }

    #endregion
}

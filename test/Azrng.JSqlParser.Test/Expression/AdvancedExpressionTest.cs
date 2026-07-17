using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Statement.Select;

namespace Azrng.JSqlParser.Test.Expression;

/// <summary>
/// 高级表达式测试 (CASE/CAST/窗口函数/EXTRACT/INTERVAL)
/// </summary>
public class AdvancedExpressionTest
{
    #region CASE 表达式

    [Fact]
    public void Case_Simple_ShouldParse()
    {
        var select = (PlainSelect)SqlParser.Parse(
            "SELECT CASE WHEN status = 1 THEN 'active' ELSE 'inactive' END FROM users")!;
        Assert.NotNull(select.SelectItems);
    }

    [Fact]
    public void Case_MultipleWhen_ShouldParse()
    {
        var select = (PlainSelect)SqlParser.Parse(
            "SELECT CASE WHEN score >= 90 THEN 'A' WHEN score >= 80 THEN 'B' ELSE 'C' END FROM students")!;
        Assert.NotNull(select.SelectItems);
    }

    [Fact]
    public void Case_WithElse_ShouldParse()
    {
        var select = (PlainSelect)SqlParser.Parse(
            "SELECT CASE WHEN id > 0 THEN id ELSE 0 END FROM users")!;
        Assert.NotNull(select.SelectItems);
    }

    #endregion

    #region CAST 表达式

    [Fact]
    public void Cast_ToInt_ShouldParse()
    {
        var select = (PlainSelect)SqlParser.Parse("SELECT CAST(id AS INT) FROM users")!;
        Assert.NotNull(select.SelectItems);
    }

    [Fact]
    public void Cast_ToVarchar_ShouldParse()
    {
        var select = (PlainSelect)SqlParser.Parse(
            "SELECT CAST(id AS VARCHAR(50)) FROM users")!;
        Assert.NotNull(select.SelectItems);
    }

    [Fact]
    public void Cast_ToDate_ShouldParse()
    {
        var select = (PlainSelect)SqlParser.Parse(
            "SELECT CAST(created_at AS DATE) FROM users")!;
        Assert.NotNull(select.SelectItems);
    }

    #endregion

    #region 窗口函数 (Analytic Expressions)

    [Fact]
    public void RowNumber_Over_ShouldParse()
    {
        var select = (PlainSelect)SqlParser.Parse(
            "SELECT ROW_NUMBER() OVER (ORDER BY id) AS rn FROM users")!;
        Assert.NotNull(select.SelectItems);
    }

    [Fact]
    public void Rank_Over_ShouldParse()
    {
        var select = (PlainSelect)SqlParser.Parse(
            "SELECT RANK() OVER (PARTITION BY dept ORDER BY salary DESC) AS rnk FROM employees")!;
        Assert.NotNull(select.SelectItems);
    }

    [Fact]
    public void Sum_Over_ShouldParse()
    {
        var select = (PlainSelect)SqlParser.Parse(
            "SELECT SUM(amount) OVER (PARTITION BY user_id ORDER BY created_at) AS running_total FROM orders")!;
        Assert.NotNull(select.SelectItems);
    }

    [Fact]
    public void Count_Over_ShouldParse()
    {
        var select = (PlainSelect)SqlParser.Parse(
            "SELECT COUNT(*) OVER (PARTITION BY dept) AS dept_count FROM employees")!;
        Assert.NotNull(select.SelectItems);
    }

    /// <summary>
    /// 窗口框架 ROWS BETWEEN ... AND ... 应正确解析并往返。
    /// </summary>
    [Fact]
    public void WindowFrame_RowsBetween_ShouldRoundTrip()
    {
        var select = (PlainSelect)SqlParser.Parse(
            "SELECT SUM(x) OVER (ORDER BY id ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) FROM t")!;
        var analytic = Assert.IsType<AnalyticExpression>(select.SelectItems![0].Expression);
        Assert.NotNull(analytic.WindowFrame);
        Assert.Equal(FrameType.Rows, analytic.WindowFrame!.Type);
        Assert.Equal(BoundType.UnboundedPreceding, analytic.WindowFrame.Start.Kind);
        Assert.NotNull(analytic.WindowFrame.End);
        Assert.Equal(BoundType.CurrentRow, analytic.WindowFrame.End!.Kind);
        // 往返
        var output = analytic.ToString()!;
        Assert.Contains("ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW", output);
    }

    [Fact]
    public void WindowFrame_RowsBetweenNumericOffset_ShouldRoundTrip()
    {
        var select = (PlainSelect)SqlParser.Parse(
            "SELECT SUM(x) OVER (ORDER BY id ROWS BETWEEN 1 PRECEDING AND 1 FOLLOWING) FROM t")!;
        var analytic = Assert.IsType<AnalyticExpression>(select.SelectItems![0].Expression);
        Assert.NotNull(analytic.WindowFrame);
        Assert.Equal(BoundType.Preceding, analytic.WindowFrame!.Start.Kind);
        Assert.NotNull(analytic.WindowFrame.Start.Offset);
        Assert.Equal(BoundType.Following, analytic.WindowFrame.End!.Kind);
        Assert.Contains("ROWS BETWEEN 1 PRECEDING AND 1 FOLLOWING", analytic.ToString()!);
    }

    [Fact]
    public void WindowFrame_RangeSingleBound_ShouldRoundTrip()
    {
        // 单边界形式：RANGE UNBOUNDED PRECEDING
        var select = (PlainSelect)SqlParser.Parse(
            "SELECT SUM(x) OVER (ORDER BY id RANGE UNBOUNDED PRECEDING) FROM t")!;
        var analytic = Assert.IsType<AnalyticExpression>(select.SelectItems![0].Expression);
        Assert.NotNull(analytic.WindowFrame);
        Assert.Equal(FrameType.Range, analytic.WindowFrame!.Type);
        Assert.Equal(BoundType.UnboundedPreceding, analytic.WindowFrame.Start.Kind);
        Assert.Null(analytic.WindowFrame.End);
    }

    [Fact]
    public void WindowFrame_GroupsBetween_ShouldRoundTrip()
    {
        var select = (PlainSelect)SqlParser.Parse(
            "SELECT SUM(x) OVER (ORDER BY id GROUPS BETWEEN 2 PRECEDING AND 2 FOLLOWING) FROM t")!;
        var analytic = Assert.IsType<AnalyticExpression>(select.SelectItems![0].Expression);
        Assert.Equal(FrameType.Groups, analytic.WindowFrame!.Type);
    }

    [Fact]
    public void WindowFrame_WithExclude_ShouldRoundTrip()
    {
        var select = (PlainSelect)SqlParser.Parse(
            "SELECT SUM(x) OVER (ORDER BY id ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW EXCLUDE TIES) FROM t")!;
        var analytic = Assert.IsType<AnalyticExpression>(select.SelectItems![0].Expression);
        Assert.Equal(ExcludeType.Ties, analytic.WindowFrame!.Exclude);
        Assert.Contains("EXCLUDE TIES", analytic.ToString()!);
    }

    #endregion

    #region EXTRACT 表达式

    [Fact]
    public void Extract_Year_ShouldParse()
    {
        var select = (PlainSelect)SqlParser.Parse(
            "SELECT EXTRACT(YEAR FROM created_at) FROM users")!;
        Assert.NotNull(select.SelectItems);
    }

    [Fact]
    public void Extract_Month_ShouldParse()
    {
        var select = (PlainSelect)SqlParser.Parse(
            "SELECT EXTRACT(MONTH FROM created_at) FROM users")!;
        Assert.NotNull(select.SelectItems);
    }

    #endregion

    #region INTERVAL 表达式

    [Fact]
    public void Interval_Day_ShouldParse()
    {
        var select = (PlainSelect)SqlParser.Parse(
            "SELECT created_at + INTERVAL '7' DAY FROM users")!;
        Assert.NotNull(select.SelectItems);
    }

    #endregion

    #region EXISTS 子查询

    [Fact]
    public void Exists_Subquery_ShouldParse()
    {
        var select = (PlainSelect)SqlParser.Parse(
            "SELECT id FROM users WHERE EXISTS (SELECT 1 FROM orders WHERE orders.user_id = users.id)")!;
        Assert.NotNull(select.Where);
    }

    [Fact]
    public void NotExists_Subquery_ShouldParse()
    {
        var select = (PlainSelect)SqlParser.Parse(
            "SELECT id FROM users WHERE NOT EXISTS (SELECT 1 FROM orders WHERE orders.user_id = users.id)")!;
        Assert.NotNull(select.Where);
    }

    #endregion

    #region IN 子查询

    [Fact]
    public void In_Subquery_ShouldParse()
    {
        var select = (PlainSelect)SqlParser.Parse(
            "SELECT id FROM users WHERE id IN (SELECT user_id FROM orders)")!;
        Assert.NotNull(select.Where);
    }

    #endregion

    #region BETWEEN

    [Fact]
    public void Between_IntRange_ShouldParse()
    {
        var select = (PlainSelect)SqlParser.Parse(
            "SELECT id FROM users WHERE id BETWEEN 1 AND 100")!;
        Assert.NotNull(select.Where);
    }

    [Fact]
    public void Between_DateRange_ShouldParse()
    {
        var select = (PlainSelect)SqlParser.Parse(
            "SELECT id FROM users WHERE created_at BETWEEN '2024-01-01' AND '2024-12-31'")!;
        Assert.NotNull(select.Where);
    }

    #endregion

    #region LIKE 模式匹配

    [Fact]
    public void Like_StartsWith_ShouldParse()
    {
        var select = (PlainSelect)SqlParser.Parse(
            "SELECT id FROM users WHERE name LIKE 'John%'")!;
        Assert.NotNull(select.Where);
    }

    [Fact]
    public void Like_Contains_ShouldParse()
    {
        var select = (PlainSelect)SqlParser.Parse(
            "SELECT id FROM users WHERE name LIKE '%test%'")!;
        Assert.NotNull(select.Where);
    }

    [Fact]
    public void NotLike_ShouldParse()
    {
        var select = (PlainSelect)SqlParser.Parse(
            "SELECT id FROM users WHERE name NOT LIKE '%test%'")!;
        Assert.NotNull(select.Where);
    }

    #endregion

    #region IS NULL / IS NOT NULL

    [Fact]
    public void IsNull_ShouldParse()
    {
        var select = (PlainSelect)SqlParser.Parse(
            "SELECT id FROM users WHERE email IS NULL")!;
        Assert.NotNull(select.Where);
    }

    [Fact]
    public void IsNotNull_ShouldParse()
    {
        var select = (PlainSelect)SqlParser.Parse(
            "SELECT id FROM users WHERE email IS NOT NULL")!;
        Assert.NotNull(select.Where);
    }

    #endregion

    #region COALESCE / NULLIF

    [Fact]
    public void Coalesce_ShouldParse()
    {
        var select = (PlainSelect)SqlParser.Parse(
            "SELECT COALESCE(name, 'unknown') FROM users")!;
        Assert.NotNull(select.SelectItems);
    }

    [Fact]
    public void NullIf_ShouldParse()
    {
        var select = (PlainSelect)SqlParser.Parse(
            "SELECT NULLIF(name, '') FROM users")!;
        Assert.NotNull(select.SelectItems);
    }

    #endregion

    #region 聚合函数

    [Fact]
    public void Count_Star_ShouldParse()
    {
        var select = (PlainSelect)SqlParser.Parse("SELECT COUNT(*) FROM users")!;
        Assert.NotNull(select.SelectItems);
    }

    [Fact]
    public void Count_Distinct_ShouldParse()
    {
        var select = (PlainSelect)SqlParser.Parse(
            "SELECT COUNT(DISTINCT name) FROM users")!;
        Assert.NotNull(select.SelectItems);
    }

    [Fact]
    public void Sum_Avg_Min_Max_ShouldParse()
    {
        var select = (PlainSelect)SqlParser.Parse(
            "SELECT SUM(amount), AVG(amount), MIN(amount), MAX(amount) FROM orders")!;
        Assert.Equal(4, select.SelectItems!.Count);
    }

    #endregion

    #region 字符串函数

    [Fact]
    public void Concat_Function_ShouldParse()
    {
        var select = (PlainSelect)SqlParser.Parse(
            "SELECT CONCAT(first_name, ' ', last_name) FROM users")!;
        Assert.NotNull(select.SelectItems);
    }

    [Fact]
    public void Substring_Function_ShouldParse()
    {
        var select = (PlainSelect)SqlParser.Parse(
            "SELECT SUBSTRING(name, 1, 3) FROM users")!;
        Assert.NotNull(select.SelectItems);
    }

    [Fact]
    public void Upper_Lower_ShouldParse()
    {
        var select = (PlainSelect)SqlParser.Parse(
            "SELECT UPPER(name), LOWER(email) FROM users")!;
        Assert.Equal(2, select.SelectItems!.Count);
    }

    #endregion

    #region 数学函数

    [Fact]
    public void Round_Function_ShouldParse()
    {
        var select = (PlainSelect)SqlParser.Parse(
            "SELECT ROUND(amount, 2) FROM orders")!;
        Assert.NotNull(select.SelectItems);
    }

    [Fact]
    public void Abs_Function_ShouldParse()
    {
        var select = (PlainSelect)SqlParser.Parse(
            "SELECT ABS(balance) FROM accounts")!;
        Assert.NotNull(select.SelectItems);
    }

    #endregion
}

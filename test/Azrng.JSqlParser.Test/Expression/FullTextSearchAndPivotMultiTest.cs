using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Expression.Operators.Relational;
using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Schema;
using PlainSelect = Azrng.JSqlParser.Statement.Select.PlainSelect;
using Pivot = Azrng.JSqlParser.Statement.Select.Pivot;

namespace Azrng.JSqlParser.Test.Expression;

/// <summary>
/// FullTextSearch 列类型结构化（List&lt;string&gt; → List&lt;Column&gt;）+ Pivot 多聚合测试（批次7）。
/// </summary>
public class FullTextSearchAndPivotMultiTest
{
    #region FullTextSearch 列类型结构化

    [Fact]
    public void FullTextSearch_MatchColumns_ShouldBeColumnList()
    {
        var sql = "SELECT MATCH (title, body) AGAINST ('db') FROM articles";
        var stmt = (PlainSelect)SqlParser.Parse(sql)!;
        // FullTextSearch 嵌在 SelectItem 表达式里
        var fts = stmt.SelectItems![0].Expression as FullTextSearch;
        Assert.NotNull(fts);
        // 此前是 List<string>，现对齐上游 ExpressionList<Column>
        Assert.IsType<List<Column>>(fts!.MatchColumns);
        Assert.Equal(2, fts.MatchColumns.Count);
        Assert.Equal("title", fts.MatchColumns[0].ColumnName);
        Assert.Equal("body", fts.MatchColumns[1].ColumnName);
    }

    [Fact]
    public void FullTextSearch_QualifiedColumnName_ShouldPreserveTablePrefix()
    {
        // 此前 List<string> 用 GetFullyQualifiedName() 拍平成 "t.title"，丢失结构。
        // 现保 Column 元信息，可重新解析并提取表前缀。
        var sql = "SELECT MATCH (t.title) AGAINST ('db') FROM articles t";
        var stmt = (PlainSelect)SqlParser.Parse(sql)!;
        var fts = (FullTextSearch)stmt.SelectItems![0].Expression!;
        Assert.Single(fts.MatchColumns);
        // round-trip 仍正确
        Assert.Equal(sql, stmt.ToString());
    }

    [Fact]
    public void FullTextSearch_Programmatic_ToString()
    {
        // 复用解析得到的 StringValue，避免手工构造的引号处理细节
        var against = (StringValue)SqlParser.ParseExpression("'db'")!;
        var fts = new FullTextSearch
        {
            MatchColumns = new()
            {
                new Column { ColumnName = "title" },
                new Column { ColumnName = "body" }
            },
            MatchExpression = against
        };
        Assert.Contains("MATCH (title, body)", fts.ToString());
        Assert.Contains("AGAINST", fts.ToString());
    }

    #endregion

    #region Pivot 多聚合

    [Fact]
    public void Pivot_SingleFunction_LegacyFunctionApi_StillWorks()
    {
        // 单聚合 PIVOT：旧 Function API 仍可访问（取 Functions 首项）
        var sql = "SELECT * FROM sales PIVOT (SUM(amount) FOR product IN ('A', 'B'))";
        var stmt = (PlainSelect)SqlParser.Parse(sql)!;
        var pivot = ((Table)stmt.FromItem!).Pivot;
        Assert.NotNull(pivot);
        Assert.Single(pivot!.Functions);
        Assert.Equal("SUM", pivot.Function.Name);  // 旧 API 取首项
    }

    [Fact]
    public void Pivot_MultiFunction_ShouldCollectAllFunctions()
    {
        // 多聚合 PIVOT (SUM(a), COUNT(b))：对齐上游 functionItems 列表
        // grammar 在 functionExpr 后加 (COMMA functionExpr)* 支持多聚合
        var sql = "SELECT * FROM sales PIVOT (SUM(amount), COUNT(qty) FOR product IN ('A', 'B'))";
        var stmt = (PlainSelect)SqlParser.Parse(sql)!;
        var pivot = ((Table)stmt.FromItem!).Pivot;
        Assert.NotNull(pivot);
        Assert.Equal(2, pivot!.Functions.Count);
        Assert.Equal("SUM", pivot.Functions[0].Name);
        Assert.Equal("COUNT", pivot.Functions[1].Name);
        // round-trip 保留两个聚合
        Assert.Contains("SUM(amount), COUNT(qty)", stmt.ToString()!);
    }

    [Fact]
    public void Pivot_Programmatic_MultiFunction_ToString()
    {
        var inA = (StringValue)SqlParser.ParseExpression("'A'")!;
        var inB = (StringValue)SqlParser.ParseExpression("'B'")!;
        var pivot = new Pivot
        {
            Functions = new()
            {
                new Function { Name = "SUM" },
                new Function { Name = "COUNT" }
            },
            ForColumns = new() { new Column { ColumnName = "p" } },
            InItems = new() { inA, inB }
        };
        var rendered = pivot.ToString();
        // 多聚合用逗号分隔；IN 项正确包装
        Assert.Contains("SUM(), COUNT()", rendered);
        Assert.Contains("FOR p", rendered);
        Assert.EndsWith(")", rendered);  // 整体以右括号结尾（PIVOT (...)）
    }

    [Fact]
    public void Pivot_Function_Get_ThrowsWhenEmpty()
    {
        // Function 取空列表首项应抛 InvalidOperationException（明确提示）
        var pivot = new Pivot();
        Assert.Throws<InvalidOperationException>(() => pivot.Function);
    }

    [Fact]
    public void Pivot_Function_Set_ReplacesFunctionsList()
    {
        // Function setter 清空并 Add，保证单值语义
        var pivot = new Pivot
        {
            Functions = new() { new Function { Name = "A" }, new Function { Name = "B" } }
        };
        pivot.Function = new Function { Name = "C" };
        Assert.Single(pivot.Functions);
        Assert.Equal("C", pivot.Functions[0].Name);
    }

    #endregion
}

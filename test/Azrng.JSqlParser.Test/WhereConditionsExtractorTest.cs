using Azrng.JSqlParser.Expression;
using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Models;
using PlainSelect = Azrng.JSqlParser.Statement.Select.PlainSelect;

namespace Azrng.JSqlParser.Test;

/// <summary>
/// GetWhereConditions 测试 — WHERE AND/OR 树拍平为条件列表。
/// </summary>
public class WhereConditionsExtractorTest
{
    private static IExpression ParseWhere(string sql) => ((PlainSelect)SqlParser.Parse(sql)!).Where!;

    [Fact]
    public void GetWhereConditions_SingleComparison_ShouldHaveEmptyLinkType()
    {
        var conds = ParseWhere("SELECT id FROM t WHERE a = 1").GetWhereConditions();
        var cond = Assert.Single(conds);
        Assert.Equal(string.Empty, cond.LinkType);
        Assert.Equal("=", cond.Operator);
        Assert.Contains("a = 1", cond.SqlInfo);
    }

    [Fact]
    public void GetWhereConditions_AndChain_ShouldLinkSecondWithAnd()
    {
        var conds = ParseWhere("SELECT id FROM t WHERE a = 1 AND b = 2").GetWhereConditions();
        Assert.Equal(2, conds.Count);
        Assert.Equal(string.Empty, conds[0].LinkType);
        Assert.Equal("AND", conds[1].LinkType);
    }

    [Fact]
    public void GetWhereConditions_OrChain_ShouldLinkSecondWithOr()
    {
        var conds = ParseWhere("SELECT id FROM t WHERE a = 1 OR b = 2").GetWhereConditions();
        Assert.Equal(2, conds.Count);
        Assert.Equal("OR", conds[1].LinkType);
    }

    [Fact]
    public void GetWhereConditions_NestedAndOr_ShouldFlattenCorrectly()
    {
        // a = 1 OR (b = 2 AND c = 3)
        var conds = ParseWhere("SELECT id FROM t WHERE a = 1 OR (b = 2 AND c = 3)").GetWhereConditions();
        Assert.Equal(3, conds.Count);
        Assert.Equal(string.Empty, conds[0].LinkType);  // a=1
        Assert.Equal("OR", conds[1].LinkType);          // b=2
        Assert.Equal("AND", conds[2].LinkType);         // c=3
    }

    [Fact]
    public void GetWhereConditions_InExpression_ShouldReturnInOperator()
    {
        var conds = ParseWhere("SELECT id FROM t WHERE id IN (1, 2, 3)").GetWhereConditions();
        var cond = Assert.Single(conds);
        Assert.Equal("IN", cond.Operator);
    }

    [Fact]
    public void GetWhereConditions_NotInExpression_ShouldReturnNotInOperator()
    {
        var conds = ParseWhere("SELECT id FROM t WHERE id NOT IN (1, 2)").GetWhereConditions();
        var cond = Assert.Single(conds);
        Assert.Equal("NOT IN", cond.Operator);
    }

    [Fact]
    public void GetWhereConditions_Between_ShouldSplitIntoTwoConditions()
    {
        var conds = ParseWhere("SELECT id FROM t WHERE age BETWEEN 18 AND 65").GetWhereConditions();
        // BETWEEN 拆成 [start, end] 两个条件
        Assert.Equal(2, conds.Count);
        Assert.All(conds, c => Assert.Equal("BETWEEN", c.Operator));
        Assert.Equal(string.Empty, conds[0].LinkType);
        Assert.Equal("AND", conds[1].LinkType);
    }

    [Fact]
    public void GetWhereConditions_GreaterThan_ShouldReturnOperator()
    {
        var conds = ParseWhere("SELECT id FROM t WHERE age > 18").GetWhereConditions();
        var cond = Assert.Single(conds);
        Assert.Equal(">", cond.Operator);
    }

    [Fact]
    public void GetWhereConditions_LikeExpression_ShouldBeExtractedAsBinary()
    {
        // LikeExpression 继承 BinaryExpression，作为二元运算符被提取
        var conds = ParseWhere("SELECT id FROM t WHERE name LIKE 'a%'").GetWhereConditions();
        var cond = Assert.Single(conds);
        Assert.Equal("LIKE", cond.Operator);
    }

    [Fact]
    public void GetWhereConditions_IsNull_ShouldBeExtractedAsUnaryFallback()
    {
        // IS NULL 是单目运算符（非 BinaryExpression），走兜底分支：不丢弃，RightExpression 为 null
        var conds = ParseWhere("SELECT id FROM t WHERE name IS NULL").GetWhereConditions();
        var cond = Assert.Single(conds);
        Assert.Null(cond.RightExpression);
        Assert.Equal("IsNullExpression", cond.Operator);
        Assert.NotNull(cond.LeftExpression);
    }

    [Fact]
    public void GetWhereConditions_Exists_ShouldBeExtractedAsUnaryFallback()
    {
        var conds = ParseWhere("SELECT id FROM t WHERE EXISTS (SELECT 1 FROM s)").GetWhereConditions();
        var cond = Assert.Single(conds);
        Assert.Null(cond.RightExpression);
        Assert.Contains("Exists", cond.Operator);
    }

    [Fact]
    public void GetWhereConditions_IsNullInAndChain_ShouldNotBeDropped()
    {
        // IS NULL 混在 AND 链里，不应被丢弃（此前会静默丢失）
        var conds = ParseWhere("SELECT id FROM t WHERE a = 1 AND b IS NULL").GetWhereConditions();
        Assert.Equal(2, conds.Count);
        Assert.Contains(conds, c => c.Operator == "=");
        Assert.Contains(conds, c => c.RightExpression == null); // IS NULL 兜底
    }

    [Fact]
    public void GetWhereConditions_NullExpression_ShouldReturnEmpty()
    {
        IExpression expr = null!;
        Assert.Empty(expr.GetWhereConditions());
    }

    [Fact]
    public void GetWhereConditions_ConditionsShouldPreserveExpressionReferences()
    {
        var conds = ParseWhere("SELECT id FROM t WHERE a = 1 AND b = 2").GetWhereConditions();
        // LeftExpression/RightExpression 保留原始 AST 引用，供业务方深挖
        Assert.All(conds, c =>
        {
            Assert.NotNull(c.LeftExpression);
            Assert.NotNull(c.RightExpression);
        });
    }
}

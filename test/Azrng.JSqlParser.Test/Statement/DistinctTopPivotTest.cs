using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Statement.Select;

namespace Azrng.JSqlParser.Test.Statement;

/// <summary>
/// DISTINCT、LIMIT/OFFSET 测试
/// </summary>
public class DistinctTopPivotTest
{
    #region DISTINCT

    [Fact]
    public void Distinct_Simple_ShouldHaveDistinct()
    {
        var select = (PlainSelect)SqlParser.Parse("SELECT DISTINCT name FROM users")!;
        Assert.NotNull(select.Distinct);
    }

    [Fact]
    public void Distinct_MultipleColumns_ShouldHaveDistinct()
    {
        var select = (PlainSelect)SqlParser.Parse(
            "SELECT DISTINCT name, status FROM users")!;
        Assert.NotNull(select.Distinct);
    }

    #endregion

    #region LIMIT / OFFSET

    [Fact]
    public void Limit_Simple_ShouldHaveLimit()
    {
        var select = (PlainSelect)SqlParser.Parse("SELECT id FROM users LIMIT 10")!;
        Assert.NotNull(select.Limit);
    }

    [Fact]
    public void Limit_WithOffset_ShouldHaveBoth()
    {
        var select = (PlainSelect)SqlParser.Parse(
            "SELECT id FROM users LIMIT 10 OFFSET 20")!;
        Assert.NotNull(select.Limit);
        Assert.NotNull(select.Offset);
    }

    [Fact]
    public void Limit_RowCountOffset_ShouldParse()
    {
        var select = (PlainSelect)SqlParser.Parse(
            "SELECT id FROM users LIMIT 10, 20")!;
        Assert.NotNull(select.Limit);
    }

    #endregion

    #region ALL

    [Fact]
    public void All_InSelect_ShouldParse()
    {
        var select = (PlainSelect)SqlParser.Parse("SELECT ALL name FROM users")!;
        Assert.NotNull(select.SelectItems);
    }

    #endregion
}

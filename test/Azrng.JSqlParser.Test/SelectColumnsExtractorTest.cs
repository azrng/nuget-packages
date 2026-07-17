using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Models;
using SelectStatement = Azrng.JSqlParser.Statement.Select.Select;

namespace Azrng.JSqlParser.Test;

/// <summary>
/// GetSelectColumns 测试 — SELECT 列结构化（区分 * / t.* / 列 / 表达式）。
/// </summary>
public class SelectColumnsExtractorTest
{
    private static SelectStatement ParseSelect(string sql) => (SelectStatement)SqlParser.Parse(sql)!;

    [Fact]
    public void GetSelectColumns_AllColumns_ShouldReturnAllKind()
    {
        var cols = ParseSelect("SELECT * FROM users").GetSelectColumns();
        var col = Assert.Single(cols);
        Assert.Equal(SelectColumnKind.All, col.Kind);
    }

    [Fact]
    public void GetSelectColumns_AllTableColumns_ShouldReturnAllTableKindWithAlias()
    {
        var cols = ParseSelect("SELECT u.* FROM users u").GetSelectColumns();
        var col = Assert.Single(cols);
        Assert.Equal(SelectColumnKind.AllTable, col.Kind);
        Assert.Equal("u", col.TableAlias);
    }

    [Fact]
    public void GetSelectColumns_PlainColumn_ShouldReturnColumnKind()
    {
        var cols = ParseSelect("SELECT u.name FROM users u").GetSelectColumns();
        var col = Assert.Single(cols);
        Assert.Equal(SelectColumnKind.Column, col.Kind);
        Assert.Equal("name", col.ColumnName);
        Assert.Equal("u", col.TableAlias);
    }

    [Fact]
    public void GetSelectColumns_ColumnWithAlias_ShouldPreserveAlias()
    {
        var cols = ParseSelect("SELECT name AS n FROM users").GetSelectColumns();
        var col = Assert.Single(cols);
        Assert.Equal(SelectColumnKind.Column, col.Kind);
        Assert.Equal("name", col.ColumnName);
        Assert.Equal("n", col.Alias);
    }

    [Fact]
    public void GetSelectColumns_FunctionExpression_ShouldReturnExpressionKind()
    {
        var cols = ParseSelect("SELECT COUNT(*) AS cnt FROM users").GetSelectColumns();
        var col = Assert.Single(cols);
        Assert.Equal(SelectColumnKind.Expression, col.Kind);
        Assert.Equal("cnt", col.Alias);
        Assert.NotNull(col.Expression);
    }

    [Fact]
    public void GetSelectColumns_ArithmeticExpression_ShouldReturnExpressionKind()
    {
        var cols = ParseSelect("SELECT price * qty AS total FROM orders").GetSelectColumns();
        var col = Assert.Single(cols);
        Assert.Equal(SelectColumnKind.Expression, col.Kind);
        Assert.Equal("total", col.Alias);
    }

    [Fact]
    public void GetSelectColumns_MixedKinds_ShouldClassifyEach()
    {
        var cols = ParseSelect("SELECT *, u.id, COUNT(*) AS c FROM users u").GetSelectColumns();
        Assert.Equal(3, cols.Count);
        Assert.Equal(SelectColumnKind.All, cols[0].Kind);
        Assert.Equal(SelectColumnKind.Column, cols[1].Kind);
        Assert.Equal("id", cols[1].ColumnName);
        Assert.Equal(SelectColumnKind.Expression, cols[2].Kind);
    }

    [Fact]
    public void GetSelectColumns_Union_ShouldOnlyReturnFirstBranchColumns()
    {
        var cols = ParseSelect(
            "SELECT a, b FROM t1 UNION SELECT x, y FROM t2").GetSelectColumns();
        // 集合运算仅取首个分支
        Assert.Equal(2, cols.Count);
        Assert.All(cols, c => Assert.Equal(SelectColumnKind.Column, c.Kind));
    }

    [Fact]
    public void GetSelectColumns_ExpressionWithoutAlias_ShouldStillReturn()
    {
        // 无别名的表达式列也返回（虚拟列必填校验是业务规则，库不拦截）
        var cols = ParseSelect("SELECT price * qty FROM orders").GetSelectColumns();
        var col = Assert.Single(cols);
        Assert.Equal(SelectColumnKind.Expression, col.Kind);
        Assert.Null(col.Alias);
    }

    [Fact]
    public void GetSelectColumns_OnNull_ShouldThrow()
    {
        Assert.Throws<ArgumentNullException>(() =>
            ((SelectStatement)null!).GetSelectColumns());
    }
}

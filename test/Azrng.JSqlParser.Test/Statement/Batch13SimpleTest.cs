using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Statement.Select;

namespace Azrng.JSqlParser.Test.Statement;

/// <summary>
/// BL-13 简单组回归测试：Table 多部分名 / SAFE_CAST / NumericBind / Informix FIRST+SKIP / OPTIMIZE FOR。
/// </summary>
public class Batch13SimpleTest
{
    #region Table 多部分名（修复 4 段截断 bug）

    [Fact]
    public void Table_FourPartName_RoundTrip()
    {
        // 修复前：4 段命名的首段（server）被静默丢弃；注意避免用关键字（如 schema/server）做标识符
        var stmt = SqlParser.Parse("SELECT * FROM s1.db1.sc1.t1");

        Assert.NotNull(stmt);
        Assert.Equal("SELECT * FROM s1.db1.sc1.t1", stmt!.ToString());
    }

    [Fact]
    public void Table_FourPartName_ShouldPreserveServerName()
    {
        var stmt = SqlParser.Parse("SELECT * FROM s1.db1.sc1.t1");
        var plainSelect = Assert.IsType<PlainSelect>(stmt);
        var table = Assert.IsType<Azrng.JSqlParser.Schema.Table>(plainSelect.IFromItem);

        Assert.Equal("s1", table.ServerName);
        Assert.Equal("db1", table.Database);
        Assert.Equal("sc1", table.SchemaName);
        Assert.Equal("t1", table.Name);
    }

    [Fact]
    public void Table_ThreePartName_ShouldNotHaveServer()
    {
        // 3 段不设 ServerName
        var stmt = SqlParser.Parse("SELECT * FROM db1.sc1.t1");
        var plainSelect = Assert.IsType<PlainSelect>(stmt);
        var table = Assert.IsType<Azrng.JSqlParser.Schema.Table>(plainSelect.IFromItem);

        Assert.Null(table.ServerName);
        Assert.Equal("db1", table.Database);
    }

    #endregion

    #region SAFE_CAST

    [Fact]
    public void SafeCast_RoundTrip()
    {
        var stmt = SqlParser.Parse("SELECT SAFE_CAST(x AS INT) FROM t");

        Assert.NotNull(stmt);
        Assert.Equal("SELECT SAFE_CAST(x AS INT) FROM t", stmt!.ToString());
    }

    [Fact]
    public void SafeCast_ShouldSetKeyword()
    {
        var stmt = SqlParser.Parse("SELECT SAFE_CAST(x AS VARCHAR) FROM t");
        Assert.NotNull(stmt);
        // 验证 round-trip 保留 SAFE_CAST 关键字
        Assert.Contains("SAFE_CAST", stmt!.ToString());
    }

    #endregion

    #region NumericBind（:1 数值绑定）

    [Fact]
    public void NumericBind_RoundTrip()
    {
        // :1 数值绑定（Oracle/MySQL 风格），此前 lexer 正则不支持数字开头
        var stmt = SqlParser.Parse("SELECT * FROM t WHERE id = :1");

        Assert.NotNull(stmt);
        Assert.Equal("SELECT * FROM t WHERE id = :1", stmt!.ToString());
    }

    [Fact]
    public void NumericBind_Multiple_RoundTrip()
    {
        var stmt = SqlParser.Parse("SELECT * FROM t WHERE id = :1 AND name = :2");

        Assert.NotNull(stmt);
        Assert.Equal("SELECT * FROM t WHERE id = :1 AND name = :2", stmt!.ToString());
    }

    #endregion

    #region Informix FIRST / SKIP

    [Fact]
    public void Informix_First_RoundTrip()
    {
        var stmt = SqlParser.Parse("SELECT FIRST 5 * FROM t");

        Assert.NotNull(stmt);
        Assert.Equal("SELECT FIRST 5 * FROM t", stmt!.ToString());
    }

    [Fact]
    public void Informix_SkipFirst_RoundTrip()
    {
        var stmt = SqlParser.Parse("SELECT SKIP 10 FIRST 5 * FROM t");

        Assert.NotNull(stmt);
        Assert.Equal("SELECT SKIP 10 FIRST 5 * FROM t", stmt!.ToString());
    }

    [Fact]
    public void Informix_First_ShouldSetField()
    {
        var stmt = SqlParser.Parse("SELECT FIRST 5 * FROM t");
        var plainSelect = Assert.IsType<PlainSelect>(stmt);

        Assert.NotNull(plainSelect.First);
        Assert.Null(plainSelect.Skip);
    }

    #endregion

    #region OPTIMIZE FOR

    [Fact]
    public void OptimizeFor_RoundTrip()
    {
        var stmt = SqlParser.Parse("SELECT * FROM t OPTIMIZE FOR 10 ROWS");

        Assert.NotNull(stmt);
        Assert.Equal("SELECT * FROM t OPTIMIZE FOR 10 ROWS", stmt!.ToString());
    }

    [Fact]
    public void OptimizeFor_ShouldSetField()
    {
        var stmt = SqlParser.Parse("SELECT * FROM t OPTIMIZE FOR 100 ROWS");
        var plainSelect = Assert.IsType<PlainSelect>(stmt);

        Assert.Equal(100, plainSelect.OptimizeFor);
    }

    #endregion
}

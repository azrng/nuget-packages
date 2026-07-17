using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Statement.Alter;
using Azrng.JSqlParser.Statement.Analyze;
using Azrng.JSqlParser.Statement.Comment;
using Azrng.JSqlParser.Statement.Execute;

namespace Azrng.JSqlParser.Test.Statement;

/// <summary>
/// BL-12 分批 4-8 回归测试：ANALYZE / COMMENT ON / EXECUTE/CALL / PURGE / ALTER VIEW。
/// </summary>
public class StatementsBatch4Test
{
    #region ANALYZE

    [Fact]
    public void Analyze_RoundTrip()
    {
        var stmt = SqlParser.Parse("ANALYZE users");

        Assert.NotNull(stmt);
        Assert.Equal("ANALYZE users", stmt!.ToString());
    }

    [Fact]
    public void Analyze_ShouldBuildCorrectNode()
    {
        var stmt = SqlParser.Parse("ANALYZE users");
        var analyze = Assert.IsType<Analyze>(stmt);

        Assert.Equal("users", analyze.Table.Name);
    }

    #endregion

    #region COMMENT ON

    [Fact]
    public void Comment_Table_RoundTrip()
    {
        var stmt = SqlParser.Parse("COMMENT ON TABLE users IS 'user table'");

        Assert.NotNull(stmt);
        Assert.Equal("COMMENT ON TABLE users IS 'user table'", stmt!.ToString());
    }

    [Fact]
    public void Comment_Column_RoundTrip()
    {
        var stmt = SqlParser.Parse("COMMENT ON COLUMN name IS 'user name'");

        Assert.NotNull(stmt);
        Assert.Equal("COMMENT ON COLUMN name IS 'user name'", stmt!.ToString());
    }

    #endregion

    #region EXECUTE / CALL

    [Fact]
    public void Execute_CallNoArgs_RoundTrip()
    {
        var stmt = SqlParser.Parse("CALL my_proc()");

        Assert.NotNull(stmt);
        Assert.Equal("CALL my_proc", stmt!.ToString());
    }

    [Fact]
    public void Execute_CallWithArgs_RoundTrip()
    {
        var stmt = SqlParser.Parse("CALL my_proc(1, 'hello')");

        Assert.NotNull(stmt);
        Assert.Equal("CALL my_proc(1, 'hello')", stmt!.ToString());
    }

    [Fact]
    public void Execute_ExecType_RoundTrip()
    {
        var stmt = SqlParser.Parse("EXECUTE my_proc(1)");

        Assert.NotNull(stmt);
        Assert.Equal("EXECUTE my_proc(1)", stmt!.ToString());
    }

    #endregion

    #region PURGE

    [Fact]
    public void Purge_Table_RoundTrip()
    {
        var stmt = SqlParser.Parse("PURGE TABLE recycle_bin_table");

        Assert.NotNull(stmt);
        Assert.Equal("PURGE TABLE recycle_bin_table", stmt!.ToString());
    }

    [Fact]
    public void Purge_Recyclebin_RoundTrip()
    {
        var stmt = SqlParser.Parse("PURGE RECYCLEBIN");

        Assert.NotNull(stmt);
        Assert.Equal("PURGE RECYCLEBIN", stmt!.ToString());
    }

    [Fact]
    public void Purge_DbaRecyclebin_RoundTrip()
    {
        var stmt = SqlParser.Parse("PURGE DBA_RECYCLEBIN");

        Assert.NotNull(stmt);
        Assert.Equal("PURGE DBA_RECYCLEBIN", stmt!.ToString());
    }

    #endregion

    #region ALTER VIEW

    [Fact]
    public void AlterView_RoundTrip()
    {
        var stmt = SqlParser.Parse("ALTER VIEW my_view AS SELECT * FROM users");

        Assert.NotNull(stmt);
        Assert.Equal("ALTER VIEW my_view AS SELECT * FROM users", stmt!.ToString());
    }

    [Fact]
    public void AlterView_ShouldBuildCorrectNode()
    {
        var stmt = SqlParser.Parse("ALTER VIEW my_view AS SELECT * FROM users");
        var alterView = Assert.IsType<AlterView>(stmt);

        Assert.False(alterView.UseReplace);
        Assert.Equal("my_view", alterView.View.Name);
    }

    [Fact]
    public void AlterView_Replace_RoundTrip()
    {
        var stmt = SqlParser.Parse("REPLACE VIEW my_view AS SELECT * FROM users");

        Assert.NotNull(stmt);
        var alterView = Assert.IsType<AlterView>(stmt);
        Assert.True(alterView.UseReplace);
    }

    #endregion
}

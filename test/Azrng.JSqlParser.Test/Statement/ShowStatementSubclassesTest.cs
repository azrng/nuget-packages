using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Statement.Show;

namespace Azrng.JSqlParser.Test.Statement;

/// <summary>
/// BL-12 SHOW 子类回归测试（BL-12 分批迁移，本文件覆盖 SHOW COLUMNS/INDEX/TABLES）。
///
/// 对齐上游 ShowColumnsStatement/ShowIndexStatement/ShowTablesStatement，
/// 此前 Azrng 仅通用 ShowStatement，无法区分三种 MySQL SHOW 子语句。
/// </summary>
public class ShowStatementSubclassesTest
{
    [Fact]
    public void ShowColumns_RoundTrip()
    {
        var stmt = SqlParser.Parse("SHOW COLUMNS FROM users");

        Assert.NotNull(stmt);
        Assert.Equal("SHOW COLUMNS FROM users", stmt!.ToString());
    }

    [Fact]
    public void ShowColumns_ShouldBuildCorrectNode()
    {
        var stmt = SqlParser.Parse("SHOW COLUMNS FROM users");
        var show = Assert.IsType<ShowColumnsStatement>(stmt);

        Assert.False(show.Full);
        Assert.Equal("users", show.Table?.Name);
    }

    [Fact]
    public void ShowColumns_Full_RoundTrip()
    {
        var stmt = SqlParser.Parse("SHOW FULL COLUMNS FROM users");

        Assert.NotNull(stmt);
        Assert.IsType<ShowColumnsStatement>(stmt);
        Assert.True(((ShowColumnsStatement)stmt).Full);
    }

    [Fact]
    public void ShowIndex_RoundTrip()
    {
        var stmt = SqlParser.Parse("SHOW INDEX FROM users");

        Assert.NotNull(stmt);
        Assert.Equal("SHOW INDEX FROM users", stmt!.ToString());
    }

    [Fact]
    public void ShowIndex_ShouldBuildCorrectNode()
    {
        var stmt = SqlParser.Parse("SHOW INDEX FROM users");
        var show = Assert.IsType<ShowIndexStatement>(stmt);

        Assert.Equal("users", show.Table?.Name);
    }

    [Fact]
    public void ShowTables_RoundTrip()
    {
        var stmt = SqlParser.Parse("SHOW TABLES");

        Assert.NotNull(stmt);
        Assert.Equal("SHOW TABLES", stmt!.ToString());
    }

    [Fact]
    public void ShowTables_ShouldBuildCorrectNode()
    {
        var stmt = SqlParser.Parse("SHOW TABLES");
        Assert.IsType<ShowTablesStatement>(stmt);
    }

    [Fact]
    public void Show_Generic_ShouldRemainShowStatement()
    {
        // 通用 SHOW identifier 保持原有 ShowStatement 类型
        var stmt = SqlParser.Parse("SHOW WARNINGS");

        Assert.NotNull(stmt);
        var generic = Assert.IsType<Azrng.JSqlParser.Statement.ShowStatement>(stmt);
        Assert.Equal("WARNINGS", generic.Name);
    }
}

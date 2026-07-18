using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Statement.Alter;

namespace Azrng.JSqlParser.Test.Statement;

/// <summary>
/// BL-12 RENAME TABLE 语句回归测试（BL-12 分批迁移，本文件覆盖 RENAME TABLE）。
///
/// 对齐上游 RenameTableStatement，支持单表/多表、TABLE 关键字、IF EXISTS、WAIT/NOWAIT。
/// </summary>
public class RenameTableStatementTest
{
    [Fact]
    public void Rename_Simple_RoundTrip()
    {
        var stmt = SqlParser.Parse("RENAME old_table TO new_table");

        Assert.NotNull(stmt);
        Assert.Equal("RENAME old_table TO new_table", stmt!.ToString());
    }

    [Fact]
    public void Rename_TableKeyword_RoundTrip()
    {
        var stmt = SqlParser.Parse("RENAME TABLE old_table TO new_table");

        Assert.NotNull(stmt);
        Assert.Equal("RENAME TABLE old_table TO new_table", stmt!.ToString());
    }

    [Fact]
    public void Rename_MultipleTables_RoundTrip()
    {
        var stmt = SqlParser.Parse("RENAME TABLE t1 TO t2, t3 TO t4");

        Assert.NotNull(stmt);
        Assert.Equal("RENAME TABLE t1 TO t2, t3 TO t4", stmt!.ToString());
    }

    [Fact]
    public void Rename_ShouldBuildCorrectNode()
    {
        var stmt = SqlParser.Parse("RENAME TABLE old_t TO new_t");
        var rename = Assert.IsType<RenameTableStatement>(stmt);

        Assert.True(rename.UsingTableKeyword);
        Assert.Single(rename.TableNames);
        Assert.Equal("old_t", rename.TableNames[0].Key.Name);
        Assert.Equal("new_t", rename.TableNames[0].Value.Name);
    }

    [Fact]
    public void Rename_MultipleTables_ShouldHaveAllPairs()
    {
        var stmt = SqlParser.Parse("RENAME t1 TO t2, t3 TO t4, t5 TO t6");
        var rename = Assert.IsType<RenameTableStatement>(stmt);

        Assert.Equal(3, rename.TableNames.Count);
    }

    [Fact]
    public void Rename_TableNamesFinder_ShouldExtractBothTables()
    {
        var stmt = SqlParser.Parse("RENAME TABLE old_t TO new_t");
        var tables = stmt!.GetTableNames();

        Assert.Contains("old_t", tables);
        Assert.Contains("new_t", tables);
    }
}

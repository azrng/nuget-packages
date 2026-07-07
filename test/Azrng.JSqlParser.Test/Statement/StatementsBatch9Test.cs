using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Statement.Alter;
using Azrng.JSqlParser.Statement.Create.Synonym;

namespace Azrng.JSqlParser.Test.Statement;

/// <summary>
/// BL-12 分批 9-10 回归测试：ALTER SESSION / ALTER SYSTEM / CREATE SYNONYM。
/// </summary>
public class StatementsBatch9Test
{
    #region ALTER SESSION

    [Fact]
    public void AlterSession_RoundTrip()
    {
        var stmt = CCJSqlParserUtil.Parse("ALTER SESSION SET NLS_DATE_FORMAT");

        Assert.NotNull(stmt);
        Assert.IsType<AlterSession>(stmt);
    }

    [Fact]
    public void AlterSession_ShouldBuildCorrectNode()
    {
        var stmt = CCJSqlParserUtil.Parse("ALTER SESSION SET NLS_DATE_FORMAT");
        var alterSession = Assert.IsType<AlterSession>(stmt);

        Assert.Equal("SET", alterSession.Operation);
    }

    #endregion

    #region ALTER SYSTEM

    [Fact]
    public void AlterSystem_RoundTrip()
    {
        var stmt = CCJSqlParserUtil.Parse("ALTER SYSTEM CHECKPOINT");

        Assert.NotNull(stmt);
        Assert.IsType<AlterSystemStatement>(stmt);
    }

    [Fact]
    public void AlterSystem_ShouldBuildCorrectNode()
    {
        var stmt = CCJSqlParserUtil.Parse("ALTER SYSTEM CHECKPOINT");
        var alterSystem = Assert.IsType<AlterSystemStatement>(stmt);

        Assert.Equal("CHECKPOINT", alterSystem.Operation);
    }

    #endregion

    #region CREATE SYNONYM

    [Fact]
    public void CreateSynonym_RoundTrip()
    {
        var stmt = CCJSqlParserUtil.Parse("CREATE SYNONYM emp FOR employees");

        Assert.NotNull(stmt);
        Assert.IsType<CreateSynonym>(stmt);
    }

    [Fact]
    public void CreateSynonym_OrReplacePublic_ShouldSetFlags()
    {
        var stmt = CCJSqlParserUtil.Parse("CREATE OR REPLACE PUBLIC SYNONYM emp FOR hr.employees");
        var synonym = Assert.IsType<CreateSynonym>(stmt);

        Assert.True(synonym.OrReplace);
        Assert.True(synonym.PublicSynonym);
        Assert.Equal("emp", synonym.Name);
        Assert.Contains("hr.employees", synonym.ForList);
    }

    [Fact]
    public void CreateSynonym_SimpleFor_RoundTrip()
    {
        var stmt = CCJSqlParserUtil.Parse("CREATE SYNONYM emp FOR employees");
        var synonym = Assert.IsType<CreateSynonym>(stmt);

        Assert.False(synonym.OrReplace);
        Assert.False(synonym.PublicSynonym);
        Assert.Equal("emp", synonym.Name);
    }

    #endregion
}

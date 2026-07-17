using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Statement;

namespace Azrng.JSqlParser.Test.Statement;

/// <summary>
/// SESSION 语句测试。
/// 对应上游 commit 6c98f10f（SessionStatement WITH options 支持 KEEP）。
/// </summary>
public class SessionStatementTest
{
    [Theory]
    [InlineData("SESSION START")]
    [InlineData("SESSION APPLY")]
    [InlineData("SESSION SHOW test")]
    [InlineData("SESSION DROP")]
    public void SessionStatement_BasicActions_ShouldParse(string sql)
    {
        var stmt = SqlParser.Parse(sql);
        Assert.IsType<SessionStatement>(stmt);
    }

    /// <summary>
    /// SESSION WITH options（persist/cleanup）应正确解析。
    /// </summary>
    [Fact]
    public void SessionStatement_WithOptions_ShouldParse()
    {
        var sql = "SESSION START mysession WITH persist=false,cleanup=on";
        var stmt = (SessionStatement)SqlParser.Parse(sql)!;
        Assert.True(stmt.HasOption("persist"));
        Assert.True(stmt.HasOption("cleanup"));
        Assert.Equal("false", stmt.GetOption("persist"));
        Assert.Equal("on", stmt.GetOption("cleanup"));
    }

    /// <summary>
    /// SESSION WITH options 中 keep 选项应正确解析（上游 commit 6c98f10f 核心场景）。
    /// KEEP 在 nonReservedKeyword 中，Azrng 天然支持作 option 名。
    /// </summary>
    [Fact]
    public void SessionStatement_WithKeepOption_ShouldParse()
    {
        var sql = "SESSION APPLY mysession WITH persist=false,keep=true";
        var stmt = (SessionStatement)SqlParser.Parse(sql)!;
        Assert.True(stmt.HasOption("keep"));
        Assert.Equal("true", stmt.GetOption("keep"));
    }
}


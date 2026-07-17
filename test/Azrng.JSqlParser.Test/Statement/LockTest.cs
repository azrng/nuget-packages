using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Schema;
using Azrng.JSqlParser.Statement.Lock;

namespace Azrng.JSqlParser.Test.Statement;

/// <summary>
/// LOCK TABLE 语句测试。
/// 移植自上游 JSqlParser commit 6697c063 的 LockTest.java，适配为 xUnit。
/// </summary>
public class LockTest
{
    /// <summary>断言 SQL 可被解析，且再次序列化后与原 SQL 一致。</summary>
    private static Azrng.JSqlParser.Statement.Statement AssertParseAndDeparse(string sql)
    {
        var stmt = SqlParser.Parse(sql);
        Assert.NotNull(stmt);
        Assert.Equal(sql, stmt!.ToString());
        return stmt;
    }

    [Theory]
    [InlineData("LOCK TABLE a IN EXCLUSIVE MODE")]
    [InlineData("LOCK TABLE a IN ROW EXCLUSIVE MODE")]
    [InlineData("LOCK TABLE a IN ROW SHARE MODE")]
    [InlineData("LOCK TABLE a IN SHARE MODE")]
    [InlineData("LOCK TABLE a IN SHARE UPDATE MODE")]
    [InlineData("LOCK TABLE a IN SHARE ROW EXCLUSIVE MODE")]
    [InlineData("LOCK TABLE a IN EXCLUSIVE MODE NOWAIT")]
    [InlineData("LOCK TABLE a IN SHARE ROW EXCLUSIVE MODE NOWAIT")]
    [InlineData("LOCK TABLE a IN SHARE ROW EXCLUSIVE MODE WAIT 10")]
    [InlineData("LOCK TABLE a IN EXCLUSIVE MODE WAIT 23")]
    public void LockStatements_ShouldParseAndDeparse(string sql)
    {
        AssertParseAndDeparse(sql);
    }

    [Fact]
    public void LockExclusiveMode_ShouldSetExclusiveMode()
    {
        var statement = SqlParser.Parse("LOCK TABLE a IN EXCLUSIVE MODE")!;
        var ls = Assert.IsType<LockStatement>(statement);
        Assert.Equal(LockMode.Exclusive, ls.LockMode);
        Assert.False(ls.NoWait);
    }

    [Fact]
    public void LockShareModeNowait_ShouldSetShareAndNoWait()
    {
        var statement = SqlParser.Parse("LOCK TABLE a IN SHARE MODE NOWAIT")!;
        var ls = Assert.IsType<LockStatement>(statement);
        Assert.Equal(LockMode.Share, ls.LockMode);
        Assert.True(ls.NoWait);
    }

    [Fact]
    public void LockShareModeWait_ShouldSetWaitSeconds()
    {
        var statement = SqlParser.Parse("LOCK TABLE a IN SHARE MODE WAIT 300")!;
        var ls = Assert.IsType<LockStatement>(statement);
        Assert.Equal(LockMode.Share, ls.LockMode);
        Assert.NotNull(ls.WaitSeconds);
        Assert.Equal(300L, ls.WaitSeconds);
    }

    [Fact]
    public void CreateLockStatement_Manual_ShouldRenderCorrectly()
    {
        var t = new Table { Name = "a" };
        var ls = new LockStatement(t, LockMode.Exclusive);
        Assert.Equal("LOCK TABLE a IN EXCLUSIVE MODE", ls.ToString());

        ls.LockMode = LockMode.Share;
        Assert.Equal("LOCK TABLE a IN SHARE MODE", ls.ToString());

        ls.NoWait = true;
        Assert.Equal("LOCK TABLE a IN SHARE MODE NOWAIT", ls.ToString());

        ls.NoWait = false;
        ls.WaitSeconds = 60L;
        Assert.Equal("LOCK TABLE a IN SHARE MODE WAIT 60", ls.ToString());

        ls.WaitSeconds = null;
        Assert.Equal("LOCK TABLE a IN SHARE MODE", ls.ToString());

        ls.Table = new Table { Name = "b" };
        Assert.Equal("LOCK TABLE b IN SHARE MODE", ls.ToString());
    }

    [Fact]
    public void CreateLockStatement_WaitAfterNoWait_ShouldThrow()
    {
        var t = new Table { Name = "a" };
        var ls = new LockStatement(t, LockMode.Exclusive) { NoWait = true };
        Assert.Throws<InvalidOperationException>(() => ls.WaitSeconds = 60L);
    }

    [Fact]
    public void CreateLockStatement_NoWaitAfterWait_ShouldThrow()
    {
        var t = new Table { Name = "a" };
        var ls = new LockStatement(t, LockMode.Exclusive) { WaitSeconds = 60L };
        Assert.Throws<InvalidOperationException>(() => ls.NoWait = true);
    }
}

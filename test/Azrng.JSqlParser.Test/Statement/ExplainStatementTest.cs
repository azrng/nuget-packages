using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Statement;

namespace Azrng.JSqlParser.Test.Statement;

/// <summary>
/// EXPLAIN 语句测试。EXPLAIN/ANALYZE 后可接任意 statement（包括 DML）。
/// 对应上游 commit b19d556e（EXPLAIN for DML）。
/// </summary>
public class ExplainStatementTest
{
    [Fact]
    public void Explain_Select_ShouldWrapInnerStatement()
    {
        var stmt = (ExplainStatement)SqlParser.Parse("EXPLAIN SELECT * FROM users")!;
        Assert.NotNull(stmt.Statement);
    }

    [Fact]
    public void Explain_Insert_ShouldWrapDml()
    {
        // EXPLAIN 后接 INSERT：上游 commit b19d556e 关注点
        var stmt = (ExplainStatement)SqlParser.Parse(
            "EXPLAIN INSERT INTO users (id) VALUES (1)")!;
        Assert.NotNull(stmt.Statement);
    }

    [Fact]
    public void Explain_Update_ShouldParse()
    {
        var stmt = (ExplainStatement)SqlParser.Parse(
            "EXPLAIN UPDATE users SET name = 'x' WHERE id = 1")!;
        Assert.NotNull(stmt.Statement);
    }

    [Fact]
    public void Explain_Delete_ShouldParse()
    {
        var stmt = (ExplainStatement)SqlParser.Parse(
            "EXPLAIN DELETE FROM users WHERE id = 1")!;
        Assert.NotNull(stmt.Statement);
    }

    [Fact]
    public void Analyze_Select_ShouldParse()
    {
        var stmt = (ExplainStatement)SqlParser.Parse("ANALYZE SELECT * FROM users")!;
        Assert.NotNull(stmt.Statement);
    }
}

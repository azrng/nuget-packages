using JSqlParser.Net.Parser;
using JSqlParser.Net.Statement;
using JSqlParser.Net.Statement.Select;
using Insert = JSqlParser.Net.Statement.Insert.Insert;
using Update = JSqlParser.Net.Statement.Update.Update;
using Delete = JSqlParser.Net.Statement.Delete.Delete;

namespace JSqlParser.Net.Test.Statement;

/// <summary>
/// StatementVisitor 模式测试
/// </summary>
public class StatementVisitorTest
{
    private class StatementTypeVisitor : StatementVisitorAdapter<object?>
    {
        public string LastStatementType { get; private set; } = "";

        public override object? Visit<S>(Select select, S context)
        {
            LastStatementType = "SELECT";
            return null;
        }

        public override object? Visit<S>(Insert insert, S context)
        {
            LastStatementType = "INSERT";
            return null;
        }

        public override object? Visit<S>(Update update, S context)
        {
            LastStatementType = "UPDATE";
            return null;
        }

        public override object? Visit<S>(Delete delete, S context)
        {
            LastStatementType = "DELETE";
            return null;
        }
    }

    [Fact]
    public void StatementVisitor_Select_ShouldVisit()
    {
        var stmt = CCJSqlParserUtil.Parse("SELECT id FROM users");
        var visitor = new StatementTypeVisitor();
        stmt!.Accept(visitor);
        Assert.Equal("SELECT", visitor.LastStatementType);
    }

    [Fact]
    public void StatementVisitor_Insert_ShouldVisit()
    {
        var stmt = CCJSqlParserUtil.Parse("INSERT INTO users (id) VALUES (1)");
        var visitor = new StatementTypeVisitor();
        stmt!.Accept(visitor);
        Assert.Equal("INSERT", visitor.LastStatementType);
    }

    [Fact]
    public void StatementVisitor_Update_ShouldVisit()
    {
        var stmt = CCJSqlParserUtil.Parse("UPDATE users SET name = 'test'");
        var visitor = new StatementTypeVisitor();
        stmt!.Accept(visitor);
        Assert.Equal("UPDATE", visitor.LastStatementType);
    }

    [Fact]
    public void StatementVisitor_Delete_ShouldVisit()
    {
        var stmt = CCJSqlParserUtil.Parse("DELETE FROM users WHERE id = 1");
        var visitor = new StatementTypeVisitor();
        stmt!.Accept(visitor);
        Assert.Equal("DELETE", visitor.LastStatementType);
    }
}

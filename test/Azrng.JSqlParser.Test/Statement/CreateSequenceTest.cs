using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Schema;
using Azrng.JSqlParser.Statement.Create.Sequence;
using CreateSequence = Azrng.JSqlParser.Statement.Create.Sequence.CreateSequence;

namespace Azrng.JSqlParser.Test.Statement;

/// <summary>
/// CREATE SEQUENCE 语句测试。
/// 移植自上游 JSqlParser 5.4 CreateSequenceTest，适配为 xUnit。
/// </summary>
public class CreateSequenceTest
{
    [Fact]
    public void CreateSequence_Simple_ShouldRoundTrip()
    {
        var sql = "CREATE SEQUENCE my_seq";
        var stmt = CCJSqlParserUtil.Parse(sql);
        var cs = Assert.IsType<CreateSequence>(stmt);
        Assert.Equal("my_seq", cs.Sequence!.Name);
        Assert.Null(cs.Sequence.Parameters);
        Assert.Equal(sql, stmt!.ToString());
    }

    [Fact]
    public void CreateSequence_QualifiedName_ShouldRoundTrip()
    {
        var sql = "CREATE SEQUENCE db.schema.my_seq";
        var stmt = CCJSqlParserUtil.Parse(sql);
        var cs = Assert.IsType<CreateSequence>(stmt);
        Assert.Equal("db", cs.Sequence!.Database);
        Assert.Equal("schema", cs.Sequence.SchemaName);
        Assert.Equal("my_seq", cs.Sequence.Name);
        Assert.Equal(sql, stmt!.ToString());
    }

    [Fact]
    public void CreateSequence_IncrementBy_ShouldRoundTrip()
    {
        var sql = "CREATE SEQUENCE db.schema.my_seq INCREMENT BY 1";
        var stmt = CCJSqlParserUtil.Parse(sql);
        Assert.Equal(sql, stmt!.ToString());
    }

    [Theory]
    [InlineData("CREATE SEQUENCE my_seq START WITH 10")]
    [InlineData("CREATE SEQUENCE my_seq MAXVALUE 5")]
    [InlineData("CREATE SEQUENCE my_seq NOMAXVALUE")]
    [InlineData("CREATE SEQUENCE my_seq MINVALUE 5")]
    [InlineData("CREATE SEQUENCE my_seq NOMINVALUE")]
    [InlineData("CREATE SEQUENCE my_seq CYCLE")]
    [InlineData("CREATE SEQUENCE my_seq NOCYCLE")]
    [InlineData("CREATE SEQUENCE my_seq CACHE 10")]
    [InlineData("CREATE SEQUENCE my_seq NOCACHE")]
    [InlineData("CREATE SEQUENCE my_seq ORDER")]
    [InlineData("CREATE SEQUENCE my_seq NOORDER")]
    [InlineData("CREATE SEQUENCE my_seq KEEP")]
    [InlineData("CREATE SEQUENCE my_seq NOKEEP")]
    public void CreateSequence_Parameters_ShouldRoundTrip(string sql)
    {
        var stmt = CCJSqlParserUtil.Parse(sql);
        Assert.NotNull(stmt);
        Assert.Equal(sql, stmt!.ToString());
    }

    [Fact]
    public void CreateSequence_PGShorthandStart_ShouldRoundTrip()
    {
        // PostgreSQL 简写：START n（不带 WITH）
        var sql = "CREATE SEQUENCE my_seq START 10";
        var stmt = CCJSqlParserUtil.Parse(sql);
        Assert.NotNull(stmt);
        Assert.Equal(sql, stmt!.ToString());
    }

    [Fact]
    public void CreateSequence_PGShorthandIncrement_ShouldRoundTrip()
    {
        // PostgreSQL 简写：INCREMENT n（不带 BY）
        var sql = "CREATE SEQUENCE my_seq INCREMENT 5";
        var stmt = CCJSqlParserUtil.Parse(sql);
        Assert.NotNull(stmt);
        Assert.Equal(sql, stmt!.ToString());
    }

    [Fact]
    public void CreateSequence_MultipleParameters_ShouldRoundTrip()
    {
        var sql = "CREATE SEQUENCE my_seq INCREMENT BY 1 START WITH 100 CACHE 20 CYCLE";
        var stmt = CCJSqlParserUtil.Parse(sql);
        var cs = Assert.IsType<CreateSequence>(stmt);
        Assert.Equal(4, cs.Sequence!.Parameters!.Count);
        Assert.Equal(sql, stmt!.ToString());
    }
}

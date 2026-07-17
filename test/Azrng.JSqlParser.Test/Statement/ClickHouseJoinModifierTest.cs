using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Statement.Select;
using Xunit;

namespace Azrng.JSqlParser.Test.Statement;

/// <summary>
/// BL-13：ClickHouse JOIN 修饰符 GLOBAL / ANY / ALL 测试。
/// 对齐上游 Join 的 isGlobal()/isAny()/isAll()。
/// </summary>
public class ClickHouseJoinModifierTest
{
    public static TheoryData<string> RoundTripCases => new()
    {
        "SELECT * FROM a GLOBAL JOIN b ON a.id = b.id",
        "SELECT * FROM a ANY JOIN b ON a.id = b.id",
        "SELECT * FROM a ALL JOIN b ON a.id = b.id",
        "SELECT * FROM a GLOBAL ANY LEFT JOIN b ON a.id = b.id",
        "SELECT * FROM a GLOBAL ALL INNER JOIN b ON a.id = b.id",
        "SELECT * FROM a ANY LEFT JOIN b ON a.id = b.id",
        "SELECT * FROM a ALL RIGHT JOIN b ON a.id = b.id",
    };

    [Theory]
    [MemberData(nameof(RoundTripCases))]
    public void ShouldRoundTrip(string sql)
    {
        var stmt = SqlParser.Parse(sql)!;
        Assert.Equal(sql, stmt.ToString());
    }

    [Fact]
    public void GlobalJoin_SetsGlobalFlag()
    {
        var stmt = SqlParser.Parse("SELECT * FROM a GLOBAL JOIN b ON a.id = b.id")!;
        var join = ExtractFirstJoin(stmt);
        Assert.True(join.Global);
        Assert.False(join.Any);
        Assert.False(join.All);
    }

    [Fact]
    public void AnyJoin_SetsAnyFlag()
    {
        var stmt = SqlParser.Parse("SELECT * FROM a ANY JOIN b ON a.id = b.id")!;
        var join = ExtractFirstJoin(stmt);
        Assert.True(join.Any);
    }

    [Fact]
    public void AllJoin_SetsAllFlag()
    {
        var stmt = SqlParser.Parse("SELECT * FROM a ALL JOIN b ON a.id = b.id")!;
        var join = ExtractFirstJoin(stmt);
        Assert.True(join.All);
    }

    [Fact]
    public void GlobalAnyLeftJoin_SetsAllFlags()
    {
        var stmt = SqlParser.Parse("SELECT * FROM a GLOBAL ANY LEFT JOIN b ON a.id = b.id")!;
        var join = ExtractFirstJoin(stmt);
        Assert.True(join.Global);
        Assert.True(join.Any);
        Assert.True(join.Left);
    }

    private static Join ExtractFirstJoin(Azrng.JSqlParser.Statement.Statement stmt)
    {
        var plain = Assert.IsType<PlainSelect>(stmt);
        Assert.NotEmpty(plain.Joins);
        return plain.Joins[0];
    }
}

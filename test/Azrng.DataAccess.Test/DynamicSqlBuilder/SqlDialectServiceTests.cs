using Azrng.Core.Model;
using Azrng.Database.DynamicSqlBuilder.Services;

namespace Azrng.DataAccess.Test.DynamicSqlBuilder;

public class SqlDialectServiceTests
{
    [Fact]
    public void PostgreSqlDialect_ShouldReturnCurrentSqlFragments()
    {
        Assert.Equal("@", SqlDialectService.GetParameterPrefix(DatabaseType.PostgresSql));
        Assert.Equal("\\", SqlDialectService.GetLikeEscapeCharacter(DatabaseType.PostgresSql));
        Assert.Equal("status = ANY(@ids)", SqlDialectService.GetInOperatorSql("status", "@ids", DatabaseType.PostgresSql));
        Assert.Equal("status != ALL(@ids)", SqlDialectService.GetNotInOperatorSql("status", "@ids", DatabaseType.PostgresSql));
        Assert.Equal("select * from users ORDER BY created_at DESC LIMIT 20 OFFSET 40",
            SqlDialectService.GetPagingSql("select * from users", 3, 20, "created_at DESC", DatabaseType.PostgresSql));
    }

    [Theory]
    [InlineData(DatabaseType.MySql)]
    [InlineData(DatabaseType.SqlServer)]
    [InlineData(DatabaseType.Sqlite)]
    public void UnsupportedDialects_ShouldThrowExplicitNotSupportedException(DatabaseType dialect)
    {
        var exception = Assert.Throws<NotSupportedException>(() =>
            SqlDialectService.GetParameterPrefix(dialect));

        Assert.Contains("DynamicSqlBuilder 当前仅支持 PostgreSQL 方言", exception.Message);
        Assert.Contains(dialect.ToString(), exception.Message);
    }
}

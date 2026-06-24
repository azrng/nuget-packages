using Azrng.Core.Model;
using Azrng.Database.DynamicSqlBuilder.Model;
using Azrng.Database.DynamicSqlBuilder.Services;

namespace Azrng.DataAccess.Test.DynamicSqlBuilder;

public class LikeEscapeHelperTests
{
    [Theory]
    [InlineData(null, null)]
    [InlineData("", "")]
    [InlineData("abc", "abc")]
    [InlineData("a%b_c\\d", "a\\%b\\_c\\\\d")]
    public void EscapeLikePattern_ShouldEscapePostgreSqlWildcardCharacters(string? pattern, string? expected)
    {
        var actual = LikeEscapeHelper.EscapeLikePattern(pattern!);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(LikeMatchType.Contains, "%a\\%b%")]
    [InlineData(LikeMatchType.StartsWith, "a\\%b%")]
    [InlineData(LikeMatchType.EndsWith, "%a\\%b")]
    [InlineData(LikeMatchType.Exact, "a\\%b")]
    public void CreateSearchPattern_ShouldApplyMatchTypeAfterEscaping(LikeMatchType matchType, string expected)
    {
        var actual = LikeEscapeHelper.CreateSearchPattern("a%b", matchType);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void CreateSearchPattern_ShouldReturnWildcardForEmptyText()
    {
        Assert.Equal("%", LikeEscapeHelper.CreateSearchPattern(""));
    }

    [Fact]
    public void CreateSafeLikeExpression_ShouldCreatePostgreSqlLikeAndNotLikeExpressions()
    {
        var likeExpression = LikeEscapeHelper.CreateSafeLikeExpression("name", "a%b");
        var notLikeExpression = LikeEscapeHelper.CreateSafeLikeExpression("name", "a_b", MatchOperator.NotLike);

        Assert.Equal("name LIKE a\\%b ESCAPE '\\'", likeExpression);
        Assert.Equal("name NOT LIKE a\\_b ESCAPE '\\'", notLikeExpression);
    }

    [Fact]
    public void LikeHelpers_ShouldRejectUnsupportedDialect()
    {
        var exception = Assert.Throws<NotSupportedException>(() =>
            LikeEscapeHelper.EscapeLikePattern("abc", DatabaseType.SqlServer));

        Assert.Contains("当前仅支持 PostgreSQL 方言", exception.Message);
    }
}

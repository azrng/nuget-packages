using Azrng.Database.DynamicSqlBuilder;
using Azrng.Database.DynamicSqlBuilder.Model;
using Dapper;

namespace Azrng.DataAccess.Test.DynamicSqlBuilder;

public class SqlWhereClauseHelperTests : IDisposable
{
    public SqlWhereClauseHelperTests()
    {
        SqlBuilderConfigurer.ResetToDefault();
    }

    public void Dispose()
    {
        SqlBuilderConfigurer.ResetToDefault();
    }

    [Fact]
    public void SplicingWhereConditionSql_ShouldBuildBetweenConditionFromDateRange()
    {
        var parameters = new DynamicParameters();
        var clause = new SqlWhereClauseInfoDto(
            "created_at",
            new List<FieldValueInfoDto>
            {
                new("2026-01-01"),
                new("2026-01-31")
            },
            MatchOperator.Between);

        var sql = SqlWhereClauseHelper.SplicingWhereConditionSql(clause, parameters);

        Assert.Contains("AND", sql);
        Assert.Contains("created_at BETWEEN", sql);
        Assert.Equal(2, parameters.ParameterNames.Count());
    }

    [Fact]
    public void SplicingWhereConditionSql_ShouldBuildBetweenConditionFromMonthOffset()
    {
        var parameters = new DynamicParameters();
        var clause = new SqlWhereClauseInfoDto(
            "created_at",
            new List<FieldValueInfoDto> { new("1") },
            MatchOperator.Between);

        var sql = SqlWhereClauseHelper.SplicingWhereConditionSql(clause, parameters);

        Assert.Contains("created_at BETWEEN", sql);
        Assert.Equal(2, parameters.ParameterNames.Count());
    }

    [Theory]
    [InlineData(MatchOperator.Like, "name LIKE", "%a\\%b\\_c%")]
    [InlineData(MatchOperator.NotLike, "name NOT LIKE", "%a\\%b\\_c%")]
    public void SplicingWhereConditionSql_ShouldParameterizeLikeOperators(
        MatchOperator matchOperator,
        string expectedSql,
        string expectedParameterValue)
    {
        var parameters = new DynamicParameters();
        var clause = new SqlWhereClauseInfoDto(
            "name",
            new List<FieldValueInfoDto> { new("a%b_c") },
            matchOperator);

        var sql = SqlWhereClauseHelper.SplicingWhereConditionSql(clause, parameters);

        var parameterName = Assert.Single(parameters.ParameterNames);
        Assert.Contains(expectedSql, sql);
        Assert.Contains("ESCAPE '\\'", sql);
        Assert.Equal(expectedParameterValue, parameters.Get<string>(parameterName));
    }

    [Fact]
    public void SplicingWhereConditionSql_ShouldUseCodeWhenLikeValueIsBlank()
    {
        var parameters = new DynamicParameters();
        var clause = new SqlWhereClauseInfoDto(
            "name",
            new List<FieldValueInfoDto> { new("alice", " ") },
            MatchOperator.Like);

        var sql = SqlWhereClauseHelper.SplicingWhereConditionSql(clause, parameters);

        var parameterName = Assert.Single(parameters.ParameterNames);
        Assert.Contains("name LIKE", sql);
        Assert.Equal("%alice%", parameters.Get<string>(parameterName));
    }

    [Fact]
    public void SplicingWhereConditionSql_ShouldThrowWhenLeafConditionHasNoFieldName()
    {
        var parameters = new DynamicParameters();
        var clause = new SqlWhereClauseInfoDto(
            string.Empty,
            new List<FieldValueInfoDto> { new("alice") },
            MatchOperator.Equal);

        var exception = Assert.Throws<ArgumentException>(() =>
            SqlWhereClauseHelper.SplicingWhereConditionSql(clause, parameters));

        Assert.Contains("叶子查询条件必须指定字段名", exception.Message);
    }

    [Fact]
    public void SplicingWhereConditionSql_ShouldReturnEmptyWhenNoValuesAreProvided()
    {
        var parameters = new DynamicParameters();
        var clause = new SqlWhereClauseInfoDto(
            "name",
            new List<FieldValueInfoDto>(),
            MatchOperator.Like);

        var sql = SqlWhereClauseHelper.SplicingWhereConditionSql(clause, parameters);

        Assert.Equal(string.Empty, sql);
        Assert.Empty(parameters.ParameterNames);
    }
}

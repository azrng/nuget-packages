using Azrng.Core.Model;
using Azrng.Database.DynamicSqlBuilder.Model;
using Azrng.Database.DynamicSqlBuilder.Validation;

namespace Azrng.Database.DynamicSqlBuilder.Test;

public class DynamicSqlBuilderUnitTest : IDisposable
{
    public DynamicSqlBuilderUnitTest()
    {
        SqlBuilderConfigurer.ResetToDefault();
    }

    public void Dispose()
    {
        SqlBuilderConfigurer.ResetToDefault();
    }

    [Fact]
    public void BuilderSqlQueryStatementGeneric_ShouldParameterizeComparisonOperators()
    {
        var queryFields = new List<string> { "age" };
        var whereClauses = new List<SqlWhereClauseInfoDto>
        {
            new("age", new List<FieldValueInfoDto> { new(18) }, MatchOperator.GreaterThan, valueType: typeof(int)),
            new("age", new List<FieldValueInfoDto> { new(30) }, MatchOperator.LessThanEqual, valueType: typeof(int))
        };

        var (sql, parameters) = DynamicSqlBuilderHelper.BuilderSqlQueryStatementGeneric(
            "users",
            string.Empty,
            queryFields,
            whereClauses);

        Assert.Contains("age >", sql);
        Assert.Contains("age <=", sql);
        Assert.DoesNotContain(" > 18", sql);
        Assert.DoesNotContain(" <= 30", sql);
        Assert.Equal(2, parameters.ParameterNames.Count());
    }

    [Fact]
    public void BuilderSqlQueryStatementGeneric_ShouldParameterizeNotEqualObjectValues()
    {
        var whereClauses = new List<SqlWhereClauseInfoDto>
        {
            new("status", new List<FieldValueInfoDto> { new(5) }, MatchOperator.NotEqual, valueType: typeof(int))
        };

        var (sql, parameters) = DynamicSqlBuilderHelper.BuilderSqlQueryStatementGeneric(
            "users",
            string.Empty,
            new List<string> { "status" },
            whereClauses);

        Assert.Contains("status <>", sql);
        Assert.Contains("status is null", sql);
        Assert.DoesNotContain("status <> 5", sql);
        Assert.Single(parameters.ParameterNames);
    }

    [Fact]
    public void BuilderSqlQueryStatementGeneric_ShouldRejectInvalidLogicalOperator()
    {
        var whereClauses = new List<SqlWhereClauseInfoDto>
        {
            new("status", new List<FieldValueInfoDto> { new(1) }, MatchOperator.Equal, logicalOperator: "OR 1=1")
        };

        Assert.Throws<ArgumentException>(() => DynamicSqlBuilderHelper.BuilderSqlQueryStatementGeneric(
            "users",
            string.Empty,
            new List<string> { "status" },
            whereClauses));
    }

    [Fact]
    public void BuilderSqlQueryStatementGeneric_ShouldSupportNestedGroupsWithoutFieldName()
    {
        var whereClauses = new List<SqlWhereClauseInfoDto>
        {
            new("status", new List<FieldValueInfoDto> { new(1) }, MatchOperator.Equal, logicalOperator: "AND", valueType: typeof(int)),
            new SqlWhereClauseInfoDto
            {
                LogicalOperator = "OR",
                NestedChildrens = new[]
                {
                    new SqlWhereClauseInfoDto("name", new List<FieldValueInfoDto> { new("alice") }, MatchOperator.Equal, logicalOperator: "AND"),
                    new SqlWhereClauseInfoDto("age", new List<FieldValueInfoDto> { new(20) }, MatchOperator.GreaterThan, logicalOperator: "OR", valueType: typeof(int))
                }
            }
        };

        var (sql, parameters) = DynamicSqlBuilderHelper.BuilderSqlQueryStatementGeneric(
            "users",
            string.Empty,
            new List<string> { "id", "name" },
            whereClauses);

        Assert.Contains("OR (", sql);
        Assert.Equal(3, parameters.ParameterNames.Count());
    }

    [Fact]
    public void BuilderSqlQueryStatementGeneric_ShouldRejectDangerousNecessaryCondition()
    {
        Assert.Throws<ArgumentException>(() => DynamicSqlBuilderHelper.BuilderSqlQueryStatementGeneric(
            "users",
            "where 1=1; drop table users",
            new List<string> { "id" },
            new List<SqlWhereClauseInfoDto>()));
    }

    [Fact]
    public void BuilderSqlQueryCountStatementGeneric_ShouldUseDialectPagingSyntax()
    {
        SqlBuilderConfigurer.Configure(options => options.Dialect = DatabaseType.SqlServer);

        var (querySql, countSql, _) = DynamicSqlBuilderHelper.BuilderSqlQueryCountStatementGeneric(
            "users",
            string.Empty,
            new List<string> { "id", "created_at" },
            Array.Empty<SqlWhereClauseInfoDto>(),
            pageIndex: 3,
            pageSize: 10,
            sortFields: new[] { new SortFieldDto("created_at", false) });

        Assert.Contains("ORDER BY created_at DESC", querySql);
        Assert.Contains("OFFSET 20 ROWS FETCH NEXT 10 ROWS ONLY", querySql);
        Assert.DoesNotContain("LIMIT", querySql);
        Assert.DoesNotContain("ORDER BY", countSql);
    }

    [Fact]
    public void BuilderSqlQueryCountStatementGeneric_ShouldRequireOrderByForSqlServerPaging()
    {
        SqlBuilderConfigurer.Configure(options => options.Dialect = DatabaseType.SqlServer);

        Assert.Throws<InvalidOperationException>(() => DynamicSqlBuilderHelper.BuilderSqlQueryCountStatementGeneric(
            "users",
            string.Empty,
            new List<string> { "id" },
            Array.Empty<SqlWhereClauseInfoDto>(),
            pageIndex: 1,
            pageSize: 10));
    }

    [Fact]
    public void SqlBuilderConfigurer_ResetToDefault_ShouldRestoreFreshOptions()
    {
        var callbackCount = 0;
        SqlBuilderConfigurer.Configure(options =>
        {
            options.Dialect = DatabaseType.SqlServer;
            options.EnableSqlLogging = true;
            options.OnSqlGenerated = (_, _) => callbackCount++;
        });

        DynamicSqlBuilderHelper.BuilderSqlQueryStatementGeneric(
            "users",
            string.Empty,
            new List<string> { "id" },
            new List<SqlWhereClauseInfoDto>());

        Assert.Equal(1, callbackCount);
        Assert.Equal(DatabaseType.SqlServer, SqlBuilderConfigurer.GetCurrentOptions().Dialect);

        SqlBuilderConfigurer.ResetToDefault();

        Assert.Equal(DatabaseType.PostgresSql, SqlBuilderConfigurer.GetCurrentOptions().Dialect);
        Assert.False(SqlBuilderConfigurer.GetCurrentOptions().EnableSqlLogging);
        Assert.Null(SqlBuilderConfigurer.GetCurrentOptions().OnSqlGenerated);
    }

    [Fact]
    public void FieldNameValidator_ShouldRejectInvalidAndDangerousFieldNames()
    {
        Assert.Throws<ArgumentException>(() => FieldNameValidator.ValidateFieldName("user name"));
        Assert.Throws<ArgumentException>(() => FieldNameValidator.ValidateFieldName("name;drop"));
        Assert.Throws<ArgumentException>(() => FieldNameValidator.ValidateFieldName("123name"));
    }
}

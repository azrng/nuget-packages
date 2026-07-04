using Azrng.JSqlParser.Parser;
using Azrng.JSqlParser.Statement.Create.Policy;

namespace Azrng.JSqlParser.Test.Statement;

/// <summary>
/// PostgreSQL CREATE POLICY 语句测试（行级安全 RLS）。
/// 移植自上游 JSqlParser commit 999cdca2 的 CreatePolicyTest，适配为 xUnit。
/// </summary>
public class CreatePolicyTest
{
    [Fact]
    public void CreatePolicy_Simple_ShouldRoundTrip()
    {
        var sql = "CREATE POLICY policy_name ON table_name";
        var stmt = CCJSqlParserUtil.Parse(sql);
        var policy = Assert.IsType<CreatePolicy>(stmt);
        Assert.Equal("policy_name", policy.PolicyName);
        Assert.Equal("table_name", policy.Table!.Name);
        Assert.Null(policy.Command);
        Assert.Empty(policy.Roles);
        Assert.Equal(sql, stmt!.ToString());
    }

    [Fact]
    public void CreatePolicy_QualifiedTable_ShouldRoundTrip()
    {
        var sql = "CREATE POLICY single_tenant_access_policy ON customer_custom_data.phone_opt_out";
        var stmt = CCJSqlParserUtil.Parse(sql);
        var policy = Assert.IsType<CreatePolicy>(stmt);
        Assert.Equal("customer_custom_data", policy.Table!.SchemaName);
        Assert.Equal("phone_opt_out", policy.Table.Name);
        Assert.Equal(sql, stmt!.ToString());
    }

    [Theory]
    [InlineData("SELECT")]
    [InlineData("INSERT")]
    [InlineData("UPDATE")]
    [InlineData("DELETE")]
    [InlineData("ALL")]
    public void CreatePolicy_ForCommand_ShouldRoundTrip(string command)
    {
        var sql = $"CREATE POLICY policy1 ON table1 FOR {command}";
        var stmt = CCJSqlParserUtil.Parse(sql);
        var policy = Assert.IsType<CreatePolicy>(stmt);
        Assert.Equal(command, policy.Command);
        Assert.Equal(sql, stmt!.ToString());
    }

    [Fact]
    public void CreatePolicy_ToSingleRole_ShouldRoundTrip()
    {
        var sql = "CREATE POLICY policy1 ON table1 TO role1";
        var stmt = CCJSqlParserUtil.Parse(sql);
        var policy = Assert.IsType<CreatePolicy>(stmt);
        Assert.Single(policy.Roles);
        Assert.Equal("role1", policy.Roles[0]);
        Assert.Equal(sql, stmt!.ToString());
    }

    [Fact]
    public void CreatePolicy_ToMultipleRoles_ShouldRoundTrip()
    {
        var sql = "CREATE POLICY policy1 ON table1 TO role1, role2, role3";
        var stmt = CCJSqlParserUtil.Parse(sql);
        var policy = Assert.IsType<CreatePolicy>(stmt);
        Assert.Equal(3, policy.Roles.Count);
        Assert.Equal(sql, stmt!.ToString());
    }

    [Fact]
    public void CreatePolicy_UsingExpression_ShouldRoundTrip()
    {
        var sql = "CREATE POLICY policy1 ON table1 USING (user_id = current_user_id())";
        var stmt = CCJSqlParserUtil.Parse(sql);
        var policy = Assert.IsType<CreatePolicy>(stmt);
        Assert.NotNull(policy.UsingExpression);
        Assert.Equal(sql, stmt!.ToString());
    }

    [Fact]
    public void CreatePolicy_WithCheckExpression_ShouldRoundTrip()
    {
        var sql = "CREATE POLICY policy1 ON table1 WITH CHECK (status = 'active')";
        var stmt = CCJSqlParserUtil.Parse(sql);
        var policy = Assert.IsType<CreatePolicy>(stmt);
        Assert.NotNull(policy.WithCheckExpression);
        Assert.Equal(sql, stmt!.ToString());
    }

    [Fact]
    public void CreatePolicy_FullSyntax_ShouldRoundTrip()
    {
        var sql = "CREATE POLICY admin_policy ON documents FOR SELECT TO admin USING (is_admin = TRUE) WITH CHECK (is_admin = TRUE)";
        var stmt = CCJSqlParserUtil.Parse(sql);
        var policy = Assert.IsType<CreatePolicy>(stmt);
        Assert.Equal("SELECT", policy.Command);
        Assert.Single(policy.Roles);
        Assert.Equal("admin", policy.Roles[0]);
        Assert.NotNull(policy.UsingExpression);
        Assert.NotNull(policy.WithCheckExpression);
        Assert.Equal(sql, stmt!.ToString());
    }
}

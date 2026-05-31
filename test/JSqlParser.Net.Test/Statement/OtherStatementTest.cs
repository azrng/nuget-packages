using JSqlParser.Net.Parser;
using JSqlParser.Net.Statement;
using JSqlParser.Net.Statement.Merge;
using JSqlParser.Net.Statement.Select;

namespace JSqlParser.Net.Test.Statement;

/// <summary>
/// 其他语句测试 (ROLLBACK/SAVEPOINT/USE/SET/TRUNCATE/MERGE + Unsupported 路径)
/// </summary>
public class OtherStatementTest
{
    #region ROLLBACK / SAVEPOINT

    [Fact]
    public void Commit_Simple_ShouldParse()
    {
        var stmt = (CommitStatement)CCJSqlParserUtil.Parse("COMMIT")!;
        Assert.Equal("COMMIT", stmt.ToString());
    }

    [Fact]
    public void Rollback_Simple_ShouldParse()
    {
        var stmt = (RollbackStatement)CCJSqlParserUtil.Parse("ROLLBACK")!;
        Assert.NotNull(stmt);
    }

    [Fact]
    public void Savepoint_Simple_ShouldParse()
    {
        var stmt = (SavepointStatement)CCJSqlParserUtil.Parse("SAVEPOINT sp1")!;
        Assert.NotNull(stmt);
        Assert.Equal("sp1", stmt.Name);
    }

    #endregion

    #region USE

    [Fact]
    public void Use_Simple_ShouldParse()
    {
        var stmt = (UseStatement)CCJSqlParserUtil.Parse("USE mydb")!;
        Assert.NotNull(stmt);
        Assert.Equal("mydb", stmt.Name);
    }

    #endregion

    #region SET

    [Fact]
    public void Set_Simple_ShouldParse()
    {
        var stmt = CCJSqlParserUtil.Parse("SET @var = 1");
        Assert.NotNull(stmt);
    }

    #endregion

    #region TRUNCATE

    [Fact]
    public void Truncate_Simple_ShouldParse()
    {
        var stmt = CCJSqlParserUtil.Parse("TRUNCATE TABLE users");
        Assert.NotNull(stmt);
    }

    #endregion

    #region MERGE — 基础解析

    [Fact]
    public void Merge_Simple_ShouldParse()
    {
        var stmt = CCJSqlParserUtil.Parse(
            "MERGE INTO users u USING new_users n ON u.id = n.id " +
            "WHEN MATCHED THEN UPDATE SET u.name = n.name " +
            "WHEN NOT MATCHED THEN INSERT (id, name) VALUES (n.id, n.name)");
        Assert.NotNull(stmt);
    }

    #endregion

    #region MERGE — 内部结构断言

    [Fact]
    public void Merge_WithMatchedUpdate_ShouldHaveUpdateOperation()
    {
        var merge = (Merge)CCJSqlParserUtil.Parse(
            "MERGE INTO users u USING new_users n ON u.id = n.id " +
            "WHEN MATCHED THEN UPDATE SET u.name = n.name")!;
        Assert.NotNull(merge.Table);
        Assert.Equal("users", merge.Table!.Name);
        Assert.NotNull(merge.OnCondition);
        Assert.Single(merge.Operations);
        Assert.IsType<MergeUpdate>(merge.Operations[0]);
        var update = (MergeUpdate)merge.Operations[0];
        Assert.False(update.Not);
        Assert.Single(update.UpdateSets);
    }

    [Fact]
    public void Merge_WithNotMatchedInsert_ShouldHaveInsertOperation()
    {
        var merge = (Merge)CCJSqlParserUtil.Parse(
            "MERGE INTO users u USING new_users n ON u.id = n.id " +
            "WHEN NOT MATCHED THEN INSERT (id, name) VALUES (n.id, n.name)")!;
        Assert.Single(merge.Operations);
        Assert.IsType<MergeInsert>(merge.Operations[0]);
        var insert = (MergeInsert)merge.Operations[0];
        Assert.True(insert.Not);
        Assert.NotNull(insert.Columns);
        Assert.Equal(2, insert.Columns!.Count);
    }

    [Fact]
    public void Merge_WithMatchedDelete_ShouldHaveDeleteOperation()
    {
        var merge = (Merge)CCJSqlParserUtil.Parse(
            "MERGE INTO users u USING old_users n ON u.id = n.id " +
            "WHEN MATCHED THEN DELETE")!;
        Assert.Single(merge.Operations);
        Assert.IsType<MergeDelete>(merge.Operations[0]);
        Assert.False(((MergeDelete)merge.Operations[0]).Not);
    }

    [Fact]
    public void Merge_WithMultipleOperations_ShouldHaveAllOperations()
    {
        var merge = (Merge)CCJSqlParserUtil.Parse(
            "MERGE INTO users u USING new_users n ON u.id = n.id " +
            "WHEN MATCHED THEN UPDATE SET u.name = n.name " +
            "WHEN NOT MATCHED THEN INSERT (id, name) VALUES (n.id, n.name)")!;
        Assert.Equal(2, merge.Operations.Count);
        Assert.IsType<MergeUpdate>(merge.Operations[0]);
        Assert.IsType<MergeInsert>(merge.Operations[1]);
    }

    #endregion

    #region DESCRIBE

    [Fact]
    public void Describe_Simple_ShouldParse()
    {
        var stmt = (DescribeStatement)CCJSqlParserUtil.Parse("DESCRIBE users")!;
        Assert.NotNull(stmt);
        Assert.Equal("users", stmt.Name);
    }

    [Fact]
    public void Describe_Desc_ShouldParse()
    {
        var stmt = (DescribeStatement)CCJSqlParserUtil.Parse("DESC users")!;
        Assert.NotNull(stmt);
    }

    [Fact]
    public void Describe_SchemaQualifiedTable_ShouldUseTableName()
    {
        var stmt = (DescribeStatement)CCJSqlParserUtil.Parse("DESCRIBE mydb.users")!;
        Assert.Equal("users", stmt.Name);
    }

    #endregion

    #region SHOW

    [Fact]
    public void Show_Tables_ShouldParse()
    {
        var stmt = (ShowStatement)CCJSqlParserUtil.Parse("SHOW TABLES")!;
        Assert.NotNull(stmt);
        Assert.Equal("TABLES", stmt.Name);
    }

    [Fact]
    public void Show_Identifier_ShouldParse()
    {
        var stmt = (ShowStatement)CCJSqlParserUtil.Parse("SHOW DATABASES")!;
        Assert.Equal("DATABASES", stmt.Name);
    }

    [Fact]
    public void Show_TwoIdentifiers_ShouldParse()
    {
        var stmt = (ShowStatement)CCJSqlParserUtil.Parse("SHOW FULL TABLES")!;
        Assert.Equal("FULL TABLES", stmt.Name);
    }

    #endregion

    #region EXPLAIN

    [Fact]
    public void Explain_Select_ShouldParse()
    {
        var stmt = (ExplainStatement)CCJSqlParserUtil.Parse("EXPLAIN SELECT * FROM users")!;
        Assert.NotNull(stmt);
        Assert.NotNull(stmt.Statement);
        Assert.IsType<PlainSelect>(stmt.Statement);
    }

    #endregion

    #region SESSION

    [Fact]
    public void Session_Start_ShouldParse()
    {
        var stmt = (SessionStatement)CCJSqlParserUtil.Parse("SESSION START sid")!;
        Assert.NotNull(stmt);
        Assert.Equal(SessionStatement.Action.START, stmt.SessionAction);
        Assert.Equal("sid", stmt.Id);
    }

    #endregion

    #region GRANT

    [Fact]
    public void Grant_Select_ShouldParse()
    {
        var stmt = (GrantStatement)CCJSqlParserUtil.Parse("GRANT SELECT ON users TO public")!;
        Assert.Single(stmt.Privileges);
        Assert.Equal("SELECT", stmt.Privileges[0]);
        Assert.Equal("users", stmt.Table!.Name);
        Assert.Equal("public", stmt.Grantee);
    }

    [Fact]
    public void Grant_MultiplePrivileges_ShouldParse()
    {
        var stmt = (GrantStatement)CCJSqlParserUtil.Parse("GRANT SELECT, INSERT, UPDATE ON users TO public")!;
        Assert.Equal(new[] { "SELECT", "INSERT", "UPDATE" }, stmt.Privileges);
    }

    [Fact]
    public void Grant_AllPrivileges_ShouldParse()
    {
        var stmt = (GrantStatement)CCJSqlParserUtil.Parse("GRANT ALL PRIVILEGES ON users TO public")!;
        Assert.Equal("ALL PRIVILEGES", stmt.Privileges[0]);
    }

    [Fact]
    public void Grant_ExecuteWithGrantOption_ShouldParse()
    {
        var stmt = (GrantStatement)CCJSqlParserUtil.Parse("GRANT EXECUTE ON procedures TO admin WITH GRANT OPTION")!;
        Assert.Equal("EXECUTE", stmt.Privileges[0]);
        Assert.True(stmt.WithGrantOption);
    }

    #endregion
}

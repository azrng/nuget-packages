using Azrng.JSqlParser.Util.Validation;

namespace Azrng.JSqlParser.Test.ValidationTests;

/// <summary>
/// Validation 框架测试
/// </summary>
public class ValidationTest
{
    #region 基础功能

    [Fact]
    public void Validation_SelectStatement_ShouldPass()
    {
        var capabilities = new List<FeaturesAllowed> { FeaturesAllowed.SELECT };
        var validation = new Validation(capabilities, "SELECT id FROM users");
        var errors = validation.Validate();
        Assert.Empty(errors);
    }

    [Fact]
    public void Validation_SelectWithJoin_ShouldPass()
    {
        var capabilities = new List<FeaturesAllowed> { FeaturesAllowed.SELECT, FeaturesAllowed.JOIN };
        var validation = new Validation(capabilities,
            "SELECT u.id, o.total FROM users u INNER JOIN orders o ON u.id = o.user_id");
        var errors = validation.Validate();
        Assert.Empty(errors);
    }

    [Fact]
    public void Validation_SelectWithSubquery_ShouldPass()
    {
        var capabilities = new List<FeaturesAllowed> { FeaturesAllowed.SELECT, FeaturesAllowed.SUBQUERY };
        var validation = new Validation(capabilities,
            "SELECT id FROM users WHERE id IN (SELECT user_id FROM orders)");
        var errors = validation.Validate();
        Assert.Empty(errors);
    }

    [Fact]
    public void Validation_InvalidSql_ShouldReturnErrors()
    {
        var capabilities = new List<FeaturesAllowed> { FeaturesAllowed.SELECT };
        var validation = new Validation(capabilities, "INVALID SQL STATEMENT");
        var errors = validation.Validate();
        Assert.NotEmpty(errors);
    }

    [Fact]
    public void Validation_EmptyCapabilities_ShouldPass()
    {
        var capabilities = new List<FeaturesAllowed>();
        var validation = new Validation(capabilities, "SELECT id FROM users");
        var errors = validation.Validate();
        Assert.Empty(errors);
    }

    [Fact]
    public void Validation_GetParsedStatements_ShouldReturnStatements()
    {
        var capabilities = new List<FeaturesAllowed> { FeaturesAllowed.SELECT };
        var validation = new Validation(capabilities, "SELECT id FROM users");
        validation.Validate();
        Assert.NotNull(validation.GetParsedStatements());
    }

    [Fact]
    public void Validation_GetErrors_ShouldReturnEmptyList()
    {
        var capabilities = new List<FeaturesAllowed> { FeaturesAllowed.SELECT };
        var validation = new Validation(capabilities, "SELECT id FROM users");
        validation.Validate();
        Assert.NotNull(validation.GetErrors());
        Assert.Empty(validation.GetErrors());
    }

    #endregion

    #region INSERT 允许/拒绝

    [Fact]
    public void Validation_InsertNotAllowed_ShouldReturnError()
    {
        var capabilities = new List<FeaturesAllowed> { FeaturesAllowed.SELECT };
        var validation = new Validation(capabilities, "INSERT INTO users (id) VALUES (1)");
        var errors = validation.Validate();
        Assert.NotEmpty(errors);
        Assert.Contains(errors, e => e.RequiredFeature == FeaturesAllowed.INSERT);
    }

    [Fact]
    public void Validation_InsertAllowed_ShouldPass()
    {
        var capabilities = new List<FeaturesAllowed> { FeaturesAllowed.INSERT };
        var validation = new Validation(capabilities, "INSERT INTO users (id) VALUES (1)");
        var errors = validation.Validate();
        Assert.Empty(errors);
    }

    #endregion

    #region UPDATE 允许/拒绝

    [Fact]
    public void Validation_UpdateNotAllowed_ShouldReturnError()
    {
        var capabilities = new List<FeaturesAllowed> { FeaturesAllowed.SELECT };
        var validation = new Validation(capabilities, "UPDATE users SET name = 'test'");
        var errors = validation.Validate();
        Assert.NotEmpty(errors);
        Assert.Contains(errors, e => e.RequiredFeature == FeaturesAllowed.UPDATE);
    }

    [Fact]
    public void Validation_UpdateAllowed_ShouldPass()
    {
        var capabilities = new List<FeaturesAllowed> { FeaturesAllowed.UPDATE };
        var validation = new Validation(capabilities, "UPDATE users SET name = 'test'");
        var errors = validation.Validate();
        Assert.Empty(errors);
    }

    #endregion

    #region DELETE 允许/拒绝

    [Fact]
    public void Validation_DeleteNotAllowed_ShouldReturnError()
    {
        var capabilities = new List<FeaturesAllowed> { FeaturesAllowed.SELECT };
        var validation = new Validation(capabilities, "DELETE FROM users WHERE id = 1");
        var errors = validation.Validate();
        Assert.NotEmpty(errors);
        Assert.Contains(errors, e => e.RequiredFeature == FeaturesAllowed.DELETE);
    }

    [Fact]
    public void Validation_DeleteAllowed_ShouldPass()
    {
        var capabilities = new List<FeaturesAllowed> { FeaturesAllowed.DELETE };
        var validation = new Validation(capabilities, "DELETE FROM users WHERE id = 1");
        var errors = validation.Validate();
        Assert.Empty(errors);
    }

    #endregion

    #region UNION 允许/拒绝

    [Fact]
    public void Validation_UnionNotAllowed_ShouldReturnError()
    {
        var capabilities = new List<FeaturesAllowed> { FeaturesAllowed.SELECT };
        var validation = new Validation(capabilities,
            "SELECT id FROM users UNION SELECT id FROM admins");
        var errors = validation.Validate();
        Assert.NotEmpty(errors);
        Assert.Contains(errors, e => e.RequiredFeature == FeaturesAllowed.UNION);
    }

    [Fact]
    public void Validation_UnionAllowed_ShouldPass()
    {
        var capabilities = new List<FeaturesAllowed> { FeaturesAllowed.SELECT, FeaturesAllowed.UNION };
        var validation = new Validation(capabilities,
            "SELECT id FROM users UNION SELECT id FROM admins");
        var errors = validation.Validate();
        Assert.Empty(errors);
    }

    #endregion

    #region JOIN 不允许

    [Fact]
    public void Validation_SelectWithJoinNotAllowed_ShouldReturnError()
    {
        var capabilities = new List<FeaturesAllowed> { FeaturesAllowed.SELECT };
        var validation = new Validation(capabilities,
            "SELECT u.id FROM users u INNER JOIN orders o ON u.id = o.user_id");
        var errors = validation.Validate();
        Assert.NotEmpty(errors);
        Assert.Contains(errors, e => e.RequiredFeature == FeaturesAllowed.JOIN);
    }

    #endregion

    #region SUBQUERY 不允许

    [Fact]
    public void Validation_SelectWithSubqueryNotAllowed_ShouldReturnError()
    {
        var capabilities = new List<FeaturesAllowed> { FeaturesAllowed.SELECT };
        var validation = new Validation(capabilities,
            "SELECT id FROM users WHERE id IN (SELECT user_id FROM orders)");
        var errors = validation.Validate();
        Assert.NotEmpty(errors);
        Assert.Contains(errors, e => e.RequiredFeature == FeaturesAllowed.SUBQUERY);
    }

    #endregion

    #region CREATE / ALTER / DROP / MERGE / TRUNCATE 校验（M7 补全）

    [Fact]
    public void Validation_DropNotAllowed_ShouldReturnError()
    {
        var capabilities = new List<FeaturesAllowed> { FeaturesAllowed.SELECT };
        var errors = new Validation(capabilities, "DROP TABLE users").Validate();
        Assert.Contains(errors, e => e.RequiredFeature == FeaturesAllowed.DROP);
    }

    [Fact]
    public void Validation_DropAllowed_ShouldPass()
    {
        var capabilities = new List<FeaturesAllowed> { FeaturesAllowed.DROP };
        Assert.Empty(new Validation(capabilities, "DROP TABLE users").Validate());
    }

    [Fact]
    public void Validation_CreateNotAllowed_ShouldReturnError()
    {
        var capabilities = new List<FeaturesAllowed> { FeaturesAllowed.SELECT };
        var errors = new Validation(capabilities, "CREATE TABLE t (a INT)").Validate();
        Assert.Contains(errors, e => e.RequiredFeature == FeaturesAllowed.CREATE);
    }

    [Fact]
    public void Validation_CreateAllowed_ShouldPass()
    {
        var capabilities = new List<FeaturesAllowed> { FeaturesAllowed.CREATE };
        Assert.Empty(new Validation(capabilities, "CREATE TABLE t (a INT)").Validate());
    }

    [Fact]
    public void Validation_AlterNotAllowed_ShouldReturnError()
    {
        var capabilities = new List<FeaturesAllowed> { FeaturesAllowed.SELECT };
        var errors = new Validation(capabilities, "ALTER TABLE t ADD COLUMN b INT").Validate();
        Assert.Contains(errors, e => e.RequiredFeature == FeaturesAllowed.ALTER);
    }

    [Fact]
    public void Validation_AlterAllowed_ShouldPass()
    {
        var capabilities = new List<FeaturesAllowed> { FeaturesAllowed.ALTER };
        Assert.Empty(new Validation(capabilities, "ALTER TABLE t ADD COLUMN b INT").Validate());
    }

    [Fact]
    public void Validation_MergeNotAllowed_ShouldReturnError()
    {
        var capabilities = new List<FeaturesAllowed> { FeaturesAllowed.SELECT };
        var errors = new Validation(capabilities,
            "MERGE INTO t USING s ON t.id = s.id WHEN MATCHED THEN DELETE").Validate();
        Assert.Contains(errors, e => e.RequiredFeature == FeaturesAllowed.MERGE);
    }

    [Fact]
    public void Validation_MergeAllowed_ShouldPass()
    {
        var capabilities = new List<FeaturesAllowed> { FeaturesAllowed.MERGE };
        Assert.Empty(new Validation(capabilities,
            "MERGE INTO t USING s ON t.id = s.id WHEN MATCHED THEN DELETE").Validate());
    }

    [Fact]
    public void Validation_TruncateNotAllowed_ShouldReturnError()
    {
        var capabilities = new List<FeaturesAllowed> { FeaturesAllowed.SELECT };
        var errors = new Validation(capabilities, "TRUNCATE TABLE t").Validate();
        Assert.Contains(errors, e => e.RequiredFeature == FeaturesAllowed.TRUNCATE);
    }

    [Fact]
    public void Validation_TruncateAllowed_ShouldPass()
    {
        var capabilities = new List<FeaturesAllowed> { FeaturesAllowed.TRUNCATE };
        Assert.Empty(new Validation(capabilities, "TRUNCATE TABLE t").Validate());
    }

    #endregion

    #region MINUS 归入 EXCEPT（M7）

    [Fact]
    public void Validation_Minus_RequiresExceptFeature()
    {
        var capabilities = new List<FeaturesAllowed> { FeaturesAllowed.SELECT };
        var errors = new Validation(capabilities, "SELECT 1 MINUS SELECT 2").Validate();
        // Oracle MINUS 归入 EXCEPT 能力
        Assert.Contains(errors, e => e.RequiredFeature == FeaturesAllowed.EXCEPT);
    }

    [Fact]
    public void Validation_Minus_WithExceptAllowed_ShouldPass()
    {
        var capabilities = new List<FeaturesAllowed> { FeaturesAllowed.SELECT, FeaturesAllowed.EXCEPT };
        Assert.Empty(new Validation(capabilities, "SELECT 1 MINUS SELECT 2").Validate());
    }

    #endregion
}

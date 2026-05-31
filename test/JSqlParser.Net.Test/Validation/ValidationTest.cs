using JSqlParser.Net.Util.Validation;

namespace JSqlParser.Net.Test.ValidationTests;

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
}

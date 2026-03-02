using Azrng.Dapper.Repository;

namespace Azrng.Dapper.Test;

/// <summary>
/// 执行功能测试
/// </summary>
public class ExecuteTests
{
    private readonly IDapperRepository _dapperRepository;

    public ExecuteTests(IDapperRepository dapperRepository)
    {
        _dapperRepository = dapperRepository;
    }

    [Fact]
    public async Task TestInsertUserAsync()
    {
        // Arrange
        var account = $"insert_{DateTime.Now.Ticks}";
        var user = new User
        {
            Account = account,
            PassWord = "password123",
            Name = "Test User",
            Sex = 1,
            Credit = 100.0,
            GroupId = 1,
            Creater = "system",
            CreateTime = DateTime.Now,
            Modifyer = "system",
            ModifyTime = DateTime.Now,
            Deleted = false,
            Disabled = false
        };

        var sql = @"
            INSERT INTO example.""user""
            (account, pass_word, name, sex, credit, group_id, creater, create_time, modifyer, modify_time, deleted, disabled)
            VALUES
            (@Account, @PassWord, @Name, @Sex, @Credit, @GroupId, @Creater, @CreateTime, @Modifyer, @ModifyTime, @Deleted, @Disabled)";

        // Act
        var result = await _dapperRepository.ExecuteAsync(sql, user);

        // Assert
        Assert.Equal(1, result);

        // Cleanup
        await _dapperRepository.ExecuteAsync(@"UPDATE example.""user"" SET deleted = true WHERE account = @Account", new { Account = account });
    }

    [Fact]
    public async Task TestUpdateUserAsync()
    {
        // Arrange
        var userId = 1L;
        var sql = @"UPDATE example.""user"" SET name = @name, modifyer = @modifyer, modify_time = @modifyTime
                    WHERE id = @id AND deleted = false";

        // Act
        var result = await _dapperRepository.ExecuteAsync(sql,
            new { id = userId, name = "Updated Name", modifyer = "test", modifyTime = DateTime.Now });

        // Assert
        Assert.True(result >= 0);
    }

    [Fact]
    public async Task TestDeleteUserAsync()
    {
        // Arrange
        var userId = 1L;
        var sql = @"UPDATE example.""user"" SET deleted = true
                    WHERE id = @id";

        // Act
        var result = await _dapperRepository.ExecuteAsync(sql, new { id = userId });

        // Assert
        Assert.True(result >= 0);
    }

    [Fact]
    public void TestExecute_Sync()
    {
        // Arrange
        var account = $"test_{DateTime.Now.Ticks}";
        var user = new User
        {
            Account = account,
            PassWord = "password123",
            Name = "Sync Test User",
            Sex = 1,
            Credit = 100.0,
            GroupId = 1,
            Creater = "system",
            CreateTime = DateTime.Now,
            Modifyer = "system",
            ModifyTime = DateTime.Now,
            Deleted = false,
            Disabled = false
        };

        var sql = @"
            INSERT INTO example.""user""
            (account, pass_word, name, sex, credit, group_id, creater, create_time, modifyer, modify_time, deleted, disabled)
            VALUES
            (@Account, @PassWord, @Name, @Sex, @Credit, @GroupId, @Creater, @CreateTime, @Modifyer, @ModifyTime, @Deleted, @Disabled)";

        // Act
        var result = _dapperRepository.Execute(sql, user);

        // Assert
        Assert.Equal(1, result);

        // Cleanup
        _dapperRepository.Execute(@"UPDATE example.""user"" SET deleted = true WHERE account = @Account", new { Account = account });
    }
}

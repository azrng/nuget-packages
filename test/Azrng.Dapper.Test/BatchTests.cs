using Azrng.Dapper.Repository;

namespace Azrng.Dapper.Test;

/// <summary>
/// 批量操作测试
/// </summary>
public class BatchTests
{
    private readonly IDapperRepository _dapperRepository;

    public BatchTests(IDapperRepository dapperRepository)
    {
        _dapperRepository = dapperRepository;
    }

    [Fact]
    public async Task TestExecuteBatchAsync()
    {
        // Arrange
        var timestamp = DateTime.Now.Ticks;
        var users = new List<object>
        {
            new { Account = $"batch1_{timestamp}", PassWord = "pass123", Name = "Batch User 1", Sex = 1, Credit = 100.0, GroupId = 1, Creater = "test", CreateTime = DateTime.Now, Modifyer = "test", ModifyTime = DateTime.Now, Deleted = false, Disabled = false },
            new { Account = $"batch2_{timestamp}", PassWord = "pass123", Name = "Batch User 2", Sex = 1, Credit = 100.0, GroupId = 1, Creater = "test", CreateTime = DateTime.Now, Modifyer = "test", ModifyTime = DateTime.Now, Deleted = false, Disabled = false },
            new { Account = $"batch3_{timestamp}", PassWord = "pass123", Name = "Batch User 3", Sex = 1, Credit = 100.0, GroupId = 1, Creater = "test", CreateTime = DateTime.Now, Modifyer = "test", ModifyTime = DateTime.Now, Deleted = false, Disabled = false }
        };

        var sql = @"INSERT INTO example.""user""
                    (account, pass_word, name, sex, credit, group_id, creater, create_time, modifyer, modify_time, deleted, disabled)
                    VALUES
                    (@Account, @PassWord, @Name, @Sex, @Credit, @GroupId, @Creater, @CreateTime, @Modifyer, @ModifyTime, @Deleted, @Disabled)";

        // Act
        var affectedRows = await _dapperRepository.ExecuteBatchAsync(sql, users);

        // Assert
        Assert.Equal(3, affectedRows);

        // Cleanup
        await _dapperRepository.ExecuteAsync(@"UPDATE example.""user"" SET deleted = true WHERE account LIKE @Pattern", new { Pattern = $"batch%_{timestamp}" });
    }

    [Fact]
    public void TestExecuteBatch_Sync()
    {
        // Arrange
        var timestamp = DateTime.Now.Ticks;
        var users = new List<object>
        {
            new { Account = $"syncbatch1_{timestamp}", PassWord = "pass123", Name = "Sync Batch User 1", Sex = 1, Credit = 100.0, GroupId = 1, Creater = "test", CreateTime = DateTime.Now, Modifyer = "test", ModifyTime = DateTime.Now, Deleted = false, Disabled = false },
            new { Account = $"syncbatch2_{timestamp}", PassWord = "pass123", Name = "Sync Batch User 2", Sex = 1, Credit = 100.0, GroupId = 1, Creater = "test", CreateTime = DateTime.Now, Modifyer = "test", ModifyTime = DateTime.Now, Deleted = false, Disabled = false }
        };

        var sql = @"INSERT INTO example.""user""
                    (account, pass_word, name, sex, credit, group_id, creater, create_time, modifyer, modify_time, deleted, disabled)
                    VALUES
                    (@Account, @PassWord, @Name, @Sex, @Credit, @GroupId, @Creater, @CreateTime, @Modifyer, @ModifyTime, @Deleted, @Disabled)";

        // Act
        var affectedRows = _dapperRepository.ExecuteBatch(sql, users);

        // Assert
        Assert.Equal(2, affectedRows);

        // Cleanup
        _dapperRepository.Execute(@"UPDATE example.""user"" SET deleted = true WHERE account LIKE @Pattern", new { Pattern = $"syncbatch%_{timestamp}" });
    }
}

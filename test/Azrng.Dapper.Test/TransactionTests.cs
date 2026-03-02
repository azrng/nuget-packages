using Azrng.Dapper.Repository;

namespace Azrng.Dapper.Test;

/// <summary>
/// 事务操作测试
/// </summary>
public class TransactionTests
{
    private readonly IDapperRepository _dapperRepository;

    public TransactionTests(IDapperRepository dapperRepository)
    {
        _dapperRepository = dapperRepository;
    }

    [Fact]
    public async Task TestExecuteInTransactionAsync_Commit()
    {
        // Arrange
        var account = $"trans_commit_{DateTime.Now.Ticks}";

        // Act
        await _dapperRepository.ExecuteInTransactionAsync(async transaction =>
        {
            var sql = @"INSERT INTO example.""user""
                        (account, pass_word, name, sex, credit, group_id, creater, create_time, modifyer, modify_time, deleted, disabled)
                        VALUES
                        (@Account, @PassWord, @Name, @Sex, @Credit, @GroupId, @Creater, @CreateTime, @Modifyer, @ModifyTime, @Deleted, @Disabled)";

            var user = new User
            {
                Account = account,
                PassWord = "password123",
                Name = "Transaction Test",
                Sex = 1,
                Credit = 100.0,
                GroupId = 1,
                Creater = "test",
                CreateTime = DateTime.Now,
                Modifyer = "test",
                ModifyTime = DateTime.Now,
                Deleted = false,
                Disabled = false
            };

            await _dapperRepository.ExecuteAsync(sql, user, transaction);
        });

        // Assert - 验证数据已插入
        var checkSql = @"SELECT COUNT(*) FROM example.""user"" WHERE account = @Account AND deleted = false";
        var count = await _dapperRepository.ExecuteScalarAsync<int>(checkSql, new { Account = account });
        Assert.Equal(1, count);

        // Cleanup
        await _dapperRepository.ExecuteAsync(@"UPDATE example.""user"" SET deleted = true WHERE account = @Account", new { Account = account });
    }

    [Fact]
    public async Task TestExecuteInTransactionAsync_WithResult()
    {
        // Arrange & Act
        var count = await _dapperRepository.ExecuteInTransactionAsync(async transaction =>
        {
            var sql = @"SELECT COUNT(*) FROM example.""user"" WHERE deleted = false";
            return await _dapperRepository.ExecuteScalarAsync<int>(sql, transaction: transaction);
        });

        // Assert
        Assert.True(count >= 0);
    }

    [Fact]
    public async Task TestExecuteInTransactionAsync_RollbackOnError()
    {
        // Arrange
        var account = $"trans_rollback_{DateTime.Now.Ticks}";
        var exceptionThrown = false;

        // Act
        try
        {
            await _dapperRepository.ExecuteInTransactionAsync(async transaction =>
            {
                var sql = @"INSERT INTO example.""user""
                            (account, pass_word, name, sex, credit, group_id, creater, create_time, modifyer, modify_time, deleted, disabled)
                            VALUES
                            (@Account, @PassWord, @Name, @Sex, @Credit, @GroupId, @Creater, @CreateTime, @Modifyer, @ModifyTime, @Deleted, @Disabled)";

                var user = new User
                {
                    Account = account,
                    PassWord = "password123",
                    Name = "Transaction Test",
                    Sex = 1,
                    Credit = 100.0,
                    GroupId = 1,
                    Creater = "test",
                    CreateTime = DateTime.Now,
                    Modifyer = "test",
                    ModifyTime = DateTime.Now,
                    Deleted = false,
                    Disabled = false
                };

                await _dapperRepository.ExecuteAsync(sql, user, transaction);

                // 故意抛出异常，测试回滚
                throw new InvalidOperationException("Test rollback");
            });
        }
        catch (InvalidOperationException)
        {
            exceptionThrown = true;
        }

        // Assert
        Assert.True(exceptionThrown);

        // 验证数据未插入（已回滚）
        var checkSql = @"SELECT COUNT(*) FROM example.""user"" WHERE account = @Account AND deleted = false";
        var count = await _dapperRepository.ExecuteScalarAsync<int>(checkSql, new { Account = account });
        Assert.Equal(0, count);
    }

    [Fact]
    public void TestExecuteInTransaction_Sync()
    {
        // Arrange
        var account = $"trans_sync_{DateTime.Now.Ticks}";

        // Act
        _dapperRepository.ExecuteInTransaction(transaction =>
        {
            var sql = @"INSERT INTO example.""user""
                        (account, pass_word, name, sex, credit, group_id, creater, create_time, modifyer, modify_time, deleted, disabled)
                        VALUES
                        (@Account, @PassWord, @Name, @Sex, @Credit, @GroupId, @Creater, @CreateTime, @Modifyer, @ModifyTime, @Deleted, @Disabled)";

            var user = new User
            {
                Account = account,
                PassWord = "password123",
                Name = "Sync Transaction Test",
                Sex = 1,
                Credit = 100.0,
                GroupId = 1,
                Creater = "test",
                CreateTime = DateTime.Now,
                Modifyer = "test",
                ModifyTime = DateTime.Now,
                Deleted = false,
                Disabled = false
            };

            _dapperRepository.Execute(sql, user, transaction);
        });

        // Assert
        var checkSql = @"SELECT COUNT(*) FROM example.""user"" WHERE account = @Account AND deleted = false";
        var count = _dapperRepository.ExecuteScalar<int>(checkSql, new { Account = account });
        Assert.Equal(1, count);

        // Cleanup
        _dapperRepository.Execute(@"UPDATE example.""user"" SET deleted = true WHERE account = @Account", new { Account = account });
    }

    [Fact]
    public void TestExecuteInTransaction_WithResult_Sync()
    {
        // Arrange & Act
        var count = _dapperRepository.ExecuteInTransaction(transaction =>
        {
            var sql = @"SELECT COUNT(*) FROM example.""user"" WHERE deleted = false";
            return _dapperRepository.ExecuteScalar<int>(sql, transaction: transaction);
        });

        // Assert
        Assert.True(count >= 0);
    }

    [Fact]
    public void TestExecuteInTransaction_WithIsolationLevel()
    {
        // Arrange
        var account = $"trans_isolation_{DateTime.Now.Ticks}";

        // Act
        _dapperRepository.ExecuteInTransaction(transaction =>
        {
            var sql = @"INSERT INTO example.""user""
                        (account, pass_word, name, sex, credit, group_id, creater, create_time, modifyer, modify_time, deleted, disabled)
                        VALUES
                        (@Account, @PassWord, @Name, @Sex, @Credit, @GroupId, @Creater, @CreateTime, @Modifyer, @ModifyTime, @Deleted, @Disabled)";

            var user = new User
            {
                Account = account,
                PassWord = "password123",
                Name = "Isolation Level Test",
                Sex = 1,
                Credit = 100.0,
                GroupId = 1,
                Creater = "test",
                CreateTime = DateTime.Now,
                Modifyer = "test",
                ModifyTime = DateTime.Now,
                Deleted = false,
                Disabled = false
            };

            _dapperRepository.Execute(sql, user, transaction);
        }, System.Data.IsolationLevel.Serializable);

        // Assert
        var checkSql = @"SELECT COUNT(*) FROM example.""user"" WHERE account = @Account AND deleted = false";
        var count = _dapperRepository.ExecuteScalar<int>(checkSql, new { Account = account });
        Assert.Equal(1, count);

        // Cleanup
        _dapperRepository.Execute(@"UPDATE example.""user"" SET deleted = true WHERE account = @Account", new { Account = account });
    }
}

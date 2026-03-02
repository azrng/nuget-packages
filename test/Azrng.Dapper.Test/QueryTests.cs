using Azrng.Dapper.Repository;

namespace Azrng.Dapper.Test;

/// <summary>
/// 查询功能测试
/// </summary>
public class QueryTests
{
    private readonly IDapperRepository _dapperRepository;

    public QueryTests(IDapperRepository dapperRepository)
    {
        _dapperRepository = dapperRepository;
    }

    [Fact]
    public async Task TestQueryAsync()
    {
        // Arrange
        var sql = @"SELECT id, account, pass_word, name, sex, credit, group_id, creater, create_time, modifyer, modify_time, deleted, disabled
                    FROM example.""user"" WHERE deleted = false";

        // Act
        var users = await _dapperRepository.QueryAsync<User>(sql);

        // Assert
        Assert.NotNull(users);
    }

    [Fact]
    public async Task TestQueryFirstOrDefaultAsync()
    {
        // Arrange
        var userId = 1L;
        var sql = @"SELECT id, account, pass_word, name, sex, credit, group_id, creater, create_time, modifyer, modify_time, deleted, disabled
                    FROM example.""user"" WHERE deleted = false";

        // Act
        var user = await _dapperRepository.QueryFirstOrDefaultAsync<User>(sql, new { userId });

        // Assert
        Assert.NotNull(user);
    }

    [Fact]
    public void TestQuery_Sync()
    {
        // Arrange
        var sql = @"SELECT id, account, pass_word, name, sex, credit, group_id, creater, create_time, modifyer, modify_time, deleted, disabled
                    FROM example.""user"" WHERE deleted = false LIMIT 10";

        // Act
        var users = _dapperRepository.Query<User>(sql);

        // Assert
        Assert.NotNull(users);
    }

    [Fact]
    public void TestQueryFirstOrDefault_Sync()
    {
        // Arrange
        var sql = @"SELECT id, account, pass_word, name, sex, credit, group_id, creater, create_time, modifyer, modify_time, deleted, disabled
                    FROM example.""user"" WHERE deleted = false ORDER BY id DESC LIMIT 1";

        // Act
        var user = _dapperRepository.QueryFirstOrDefault<User>(sql);

        // Assert
        Assert.NotNull(user);
    }
}

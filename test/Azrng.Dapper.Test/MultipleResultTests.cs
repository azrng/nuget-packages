using Azrng.Dapper.Repository;

namespace Azrng.Dapper.Test;

/// <summary>
/// 多结果集查询测试
/// </summary>
public class MultipleResultTests
{
    private readonly IDapperRepository _dapperRepository;

    public MultipleResultTests(IDapperRepository dapperRepository)
    {
        _dapperRepository = dapperRepository;
    }

    [Fact]
    public async Task TestQueryMultipleAsync_TwoResults()
    {
        // Arrange
        var sql = @"
            SELECT id, account, pass_word, name, sex, credit, group_id, creater, create_time, modifyer, modify_time, deleted, disabled
            FROM example.""user"" WHERE deleted = false LIMIT 5;
            SELECT COUNT(*) FROM example.""user"" WHERE deleted = false;";

        // Act
        var (users, count) = await _dapperRepository.QueryMultipleAsync<User, long>(sql);

        // Assert
        Assert.NotNull(users);
        Assert.True(count.Count() >= 0);
    }

    [Fact]
    public async Task TestQueryMultipleAsync_ThreeResults()
    {
        // Arrange
        var sql = @"
            SELECT id, account, pass_word, name, sex, credit, group_id, creater, create_time, modifyer, modify_time, deleted, disabled
            FROM example.""user"" WHERE deleted = false ORDER BY id DESC LIMIT 1;
            SELECT COUNT(*) FROM example.""user"" WHERE deleted = false;
            SELECT COUNT(*) FROM example.""user"" WHERE deleted = true;";

        // Act
        var (user1, totalCount, deletedCount) = await _dapperRepository.QueryMultipleAsync<User, long, long>(sql);

        // Assert
        Assert.NotNull(user1);
        Assert.True(totalCount.Count() >= 0);
        Assert.True(deletedCount.Count() >= 0);
    }
}
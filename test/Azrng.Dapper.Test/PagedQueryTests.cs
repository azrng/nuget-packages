using Azrng.Dapper.Repository;

namespace Azrng.Dapper.Test;

/// <summary>
/// 分页查询测试
/// </summary>
public class PagedQueryTests
{
    private readonly IDapperRepository _dapperRepository;

    public PagedQueryTests(IDapperRepository dapperRepository)
    {
        _dapperRepository = dapperRepository;
    }

    [Fact]
    public async Task TestQueryPagedAsync()
    {
        // Arrange
        var pageIndex = 1;
        var pageSize = 10;

        var dataSql = @"SELECT id, account, pass_word, name, sex, credit, group_id, creater, create_time, modifyer, modify_time, deleted, disabled
                        FROM example.""user"" WHERE deleted = false
                        ORDER BY id
                        OFFSET @Offset LIMIT @PageSize";

        var countSql = @"SELECT COUNT(*) FROM example.""user"" WHERE deleted = false";

        // Act
        var result = await _dapperRepository.QueryPagedAsync<User>(
            dataSql,
            countSql,
            new { Offset = (pageIndex - 1) * pageSize, PageSize = pageSize }
        );

        // Assert
        Assert.NotNull(result);
        Assert.True(result.TotalCount >= 0);
        Assert.NotNull(result.Items);
    }

    [Fact]
    public async Task TestQueryPagedAsync_EmptyResult()
    {
        // Arrange
        var pageIndex = 1;
        var pageSize = 10;

        var dataSql = @"SELECT id, account, pass_word, name, sex, credit, group_id, creater, create_time, modifyer, modify_time, deleted, disabled
                        FROM example.""user"" WHERE deleted = false AND account = 'nonexistent_account'
                        ORDER BY id
                        OFFSET @Offset LIMIT @PageSize";

        var countSql = @"SELECT COUNT(*) FROM example.""user"" WHERE deleted = false AND account = 'nonexistent_account'";

        // Act
        var result = await _dapperRepository.QueryPagedAsync<User>(
            dataSql,
            countSql,
            new { Offset = (pageIndex - 1) * pageSize, PageSize = pageSize }
        );

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.TotalCount);
        Assert.Empty(result.Items);
    }
}

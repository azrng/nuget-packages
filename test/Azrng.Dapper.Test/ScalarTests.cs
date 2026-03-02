using Azrng.Dapper.Repository;

namespace Azrng.Dapper.Test;

/// <summary>
/// 标量查询测试
/// </summary>
public class ScalarTests
{
    private readonly IDapperRepository _dapperRepository;

    public ScalarTests(IDapperRepository dapperRepository)
    {
        _dapperRepository = dapperRepository;
    }

    [Fact]
    public async Task TestExecuteScalarAsync_Long()
    {
        // Arrange
        var sql = @"SELECT COUNT(*) FROM example.""user"" WHERE deleted = false";

        // Act
        var count = await _dapperRepository.ExecuteScalarAsync<long>(sql);

        // Assert
        Assert.True(count >= 0);
    }

    [Fact]
    public async Task TestExecuteScalarAsync_String()
    {
        // Arrange
        var sql = @"SELECT account FROM example.""user"" WHERE deleted = false ORDER BY id DESC LIMIT 1";

        // Act
        var account = await _dapperRepository.ExecuteScalarAsync<string>(sql);

        // Assert
        Assert.NotNull(account);
    }

    [Fact]
    public void TestExecuteScalar_Sync()
    {
        // Arrange
        var sql = @"SELECT COUNT(*) FROM example.""user"" WHERE deleted = false";

        // Act
        var count = _dapperRepository.ExecuteScalar<long>(sql);

        // Assert
        Assert.True(count >= 0);
    }
}

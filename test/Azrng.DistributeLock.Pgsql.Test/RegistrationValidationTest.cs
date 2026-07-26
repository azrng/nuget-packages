using Azrng.DistributeLock.PostgreSql;
using Microsoft.Extensions.DependencyInjection;

namespace Azrng.DistributeLock.Pgsql.Test;

/// <summary>
/// 注册参数校验测试
/// </summary>
public class RegistrationValidationTest
{
    /// <summary>
    /// 空连接字符串注册时直接抛出异常
    /// </summary>
    [Fact]
    public void EmptyConnectionString_Throws()
    {
        Assert.Throws<ArgumentException>(() => new ServiceCollection().AddDbLockProvider(""));
        Assert.Throws<ArgumentException>(() => new ServiceCollection().AddDbLockProvider("  "));
    }

    /// <summary>
    /// 空 schema 或 table 注册时直接抛出异常
    /// </summary>
    [Fact]
    public void EmptySchemaOrTable_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            new ServiceCollection().AddDbLockProvider("Host=localhost", schema: " "));
        Assert.Throws<ArgumentException>(() =>
            new ServiceCollection().AddDbLockProvider("Host=localhost", table: ""));
    }
}

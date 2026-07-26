using Microsoft.Extensions.DependencyInjection;

namespace Azrng.DistributeLock.Redis.Test;

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
        Assert.Throws<ArgumentException>(() => new ServiceCollection().AddRedisLockProvider(""));
        Assert.Throws<ArgumentException>(() => new ServiceCollection().AddRedisLockProvider("  "));
    }

    /// <summary>
    /// 非法默认过期时间注册时直接抛出异常
    /// </summary>
    [Fact]
    public void InvalidDefaultExpireTime_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ServiceCollection().AddRedisLockProvider("localhost:6379", TimeSpan.Zero));
    }
}

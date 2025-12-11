using Azrng.DistributeLock.Core;
using Xunit.Abstractions;

namespace Azrng.DistributeLock.InMemory.Test;

public class GetAndReleaseTest
{
    private readonly ILockProvider _lockProvider;
    private readonly ITestOutputHelper _testOutputHelper;

    public GetAndReleaseTest(ILockProvider lockProvider, ITestOutputHelper testOutputHelper)
    {
        _lockProvider = lockProvider;
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async Task TestLock()
    {
        var lockKey = "testLock";
        await using var flag = await _lockProvider.LockAsync(lockKey, TimeSpan.FromSeconds(5));
        if (flag is not null)
        {
            _testOutputHelper.WriteLine("Lock acquired.");

            // 模拟持有锁的业务逻辑
            await Task.Delay(3000); // 模拟业务逻辑执行时间
        }
        else
        {
            _testOutputHelper.WriteLine("Failed to acquire lock.");
        }
    }

    [Theory]
    [InlineData(10)]
    public async Task TestConcurrentAccessAsync(int numberOfThreads)
    {
        var lockKey = "testLock";
        var tasks = new Task[numberOfThreads];
        for (var i = 0; i < numberOfThreads; i++)
        {
            tasks[i] = Task.Run(() => TestLock());
        }

        await Task.WhenAll(tasks);
    }
}
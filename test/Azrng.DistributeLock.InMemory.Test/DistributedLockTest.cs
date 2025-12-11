using Azrng.DistributeLock.Core;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Azrng.DistributeLock.InMemory.Test;

public class DistributedLockTest
{
    private readonly ILockProvider _lockProvider;
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly ILogger<DistributedLockTest> _logger;

    public DistributedLockTest(ILockProvider lockProvider, ITestOutputHelper testOutputHelper, ILogger<DistributedLockTest> logger)
    {
        _lockProvider = lockProvider;
        _testOutputHelper = testOutputHelper;
        _logger = logger;
    }

    /// <summary>
    /// 正常可以获取到锁的 示例
    /// </summary>
    [Fact]
    public async Task GetLock_ReturnOk()
    {
        _logger.LogInformation("开始获取锁");
        await using var lockObject = await _lockProvider.LockAsync("GetLock", TimeSpan.FromSeconds(5));
        if (lockObject is null)
        {
            _testOutputHelper.WriteLine("获取锁失败");
            Assert.True(false);
        }

        var result = await SumAsync();
        Assert.True(result == 1);
    }

    /// <summary>
    /// 多实例情况下一个获取到锁 另一个获取锁失败，然后任务取消
    /// </summary>
    [Fact]
    public async Task MulitCase_GetLockFail_ReturnOk()
    {
        var lockKey = Guid.NewGuid().ToString();

        var task1 = Task.Run(async () =>
        {
            await using var result = await _lockProvider.LockAsync(lockKey, TimeSpan.FromSeconds(30));

            // 模拟任务执行
            await Task.Delay(TimeSpan.FromSeconds(5));
            _logger.LogInformation("任务1被执行");
        });

        var task2 = Task.Run(async () =>
        {
            await Task.Delay(TimeSpan.FromSeconds(1)); // 确保task1先执行

            await using var result = await _lockProvider.LockAsync(lockKey, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(3));

            // 模拟任务执行
            await Task.Delay(TimeSpan.FromSeconds(5));
            _logger.LogInformation("任务2被执行");
        });

        try
        {
            _testOutputHelper.WriteLine("任务执行.");
            await Task.WhenAny(task1, task2);
        }
        catch (OperationCanceledException ex)
        {
            _testOutputHelper.WriteLine("111 A task was cancelled.");
            Assert.NotNull(ex);
        }
        catch (Exception ex)
        {
            _testOutputHelper.WriteLine($"222 An error occurred: {ex.Message}");
            Assert.Null(ex);
        }
    }

    /// <summary>
    /// 多实例情况下一个获取到锁 另一个获取等待后获取锁成功
    /// </summary>
    [Fact]
    public async Task MulitCase_GetLock_DelaySuccess_ReturnOk()
    {
        var lockKey = Guid.NewGuid().ToString();

        var task1 = Task.Run(async () =>
        {
            await using var result = await _lockProvider.LockAsync(lockKey, TimeSpan.FromSeconds(30));

            // 模拟任务执行
            await Task.Delay(TimeSpan.FromSeconds(5));
            _logger.LogInformation("任务1被执行");
        });

        var task2 = Task.Run(async () =>
        {
            await Task.Delay(TimeSpan.FromSeconds(1)); // 确保task1先执行

            await using var result = await _lockProvider.LockAsync(lockKey, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(8));

            // 模拟任务执行
            await Task.Delay(TimeSpan.FromSeconds(5));
            _logger.LogInformation("任务2被执行");
        });

        try
        {
            _testOutputHelper.WriteLine("任务执行.");
            await Task.WhenAll(task1, task2);
            _testOutputHelper.WriteLine("任务执行结束.");
        }
        catch (OperationCanceledException ex)
        {
            _testOutputHelper.WriteLine("A task was cancelled.");
            Assert.NotNull(ex);
        }
        catch (Exception ex)
        {
            _testOutputHelper.WriteLine($"An error occurred: {ex.Message}");
            Assert.Null(ex);
        }
    }

    /// <summary>
    /// 多实例情况 锁自动续期
    /// </summary>
    [Fact]
    public async Task MulitCase_Lock_AutoExtend_ReturnOk()
    {
        var lockKey = Guid.NewGuid().ToString();

        var task1 = Task.Run(async () =>
        {
            await using var result = await _lockProvider.LockAsync(lockKey, TimeSpan.FromSeconds(3));

            // 模拟任务执行
            await Task.Delay(TimeSpan.FromSeconds(8));
            _logger.LogInformation("任务1被执行");
        });

        var task2 = Task.Run(async () =>
        {
            await Task.Delay(TimeSpan.FromSeconds(1)); // 确保task1先执行

            await using var result = await _lockProvider.LockAsync(lockKey, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(10));

            // 模拟任务执行
            await Task.Delay(TimeSpan.FromSeconds(5));
            _logger.LogInformation("任务2被执行");
        });

        try
        {
            _testOutputHelper.WriteLine("任务执行.");
            await Task.WhenAll(task1, task2);
            _testOutputHelper.WriteLine("任务执行结束.");
        }
        catch (OperationCanceledException ex)
        {
            _testOutputHelper.WriteLine("A task was cancelled.");
            Assert.NotNull(ex);
        }
        catch (Exception ex)
        {
            _testOutputHelper.WriteLine($"An error occurred: {ex.Message}");
            Assert.Null(ex);
        }
    }

    /// <summary>
    /// 顺序输出
    /// </summary>
    [Fact]
    public async Task OrderOutput_ReturnOk()
    {
        var taskList = new List<Task>();
        var i = 0;
        for (var j = 0; j < 4; j++)
        {
            var currTask = Task.Run(async () =>
            {
                await using var lockObject = await _lockProvider.LockAsync("orderOutput", TimeSpan.FromSeconds(10));
                if (lockObject is null)
                {
                    _testOutputHelper.WriteLine($"线程:{Task.CurrentId} 拿不到锁");
                    return;
                }

                _testOutputHelper.WriteLine($"线程:{Task.CurrentId} 拿到了锁");
                i++;

                //模拟处理业务
                await Task.Delay(TimeSpan.FromSeconds(new Random().Next(1, 4)));
                _testOutputHelper.WriteLine($"线程:{Task.CurrentId} 输出i:{i.ToString()}");
            });
            taskList.Add(currTask);
        }

        await Task.WhenAll(taskList);
        _testOutputHelper.WriteLine("over");
    }

    /// <summary>
    /// 购买商品 测试是否会超卖
    /// </summary>
    [Fact]
    public async Task BuyShop_ReturnOk()
    {
        var stock = 2; //商品库存
        var taskCount = 5; //线程数量

        var taskList = new List<Task>();
        for (var i = 0; i < taskCount; i++)
        {
            var curr = Task.Run(async () =>
            {
                await using var lockObject = await _lockProvider.LockAsync("BuyShop");
                if (lockObject is null)
                {
                    _testOutputHelper.WriteLine($"线程:{Task.CurrentId} 拿不到锁,暂停消费");
                    return;
                }

                _testOutputHelper.WriteLine($"线程:{Task.CurrentId} 拿到了锁,开始消费了");
                if (stock <= 0)
                {
                    _testOutputHelper.WriteLine($"库存不足,线程:{Task.CurrentId} 抢购失败!");
                    return;
                }

                stock--;

                //模拟处理业务
                await Task.Delay(TimeSpan.FromSeconds(new Random().Next(1, 3)));

                _testOutputHelper.WriteLine($"线程:{Task.CurrentId} 消费完毕!剩余 {stock} 个");
            });
            taskList.Add(curr);
        }

        await Task.WhenAll(taskList);

        _testOutputHelper.WriteLine($"库存数量：{stock.ToString()}");
        Assert.True(stock >= 0);
    }

    /// <summary>
    /// 异步私有方法
    /// </summary>
    /// <returns></returns>
    private async Task<int> SumAsync()
    {
        _testOutputHelper.WriteLine("拿到锁开始执行");
        await Task.Delay(TimeSpan.FromSeconds(new Random().Next(1, 3)));
        _testOutputHelper.WriteLine("我是异步方法");

        return 1;
    }
}
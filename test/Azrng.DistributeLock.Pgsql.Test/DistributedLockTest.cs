using Azrng.DistributeLock.Core;
using Microsoft.Extensions.Logging;

namespace Azrng.DistributeLock.Pgsql.Test;

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
            await Task.WhenAll(task1, task2);
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

    /// <summary>
    /// 测试锁自动延期功能 - 验证长时间任务不会因锁过期而被中断
    /// </summary>
    [Fact]
    public async Task AutoExtend_LongRunningTask_Success()
    {
        var lockKey = Guid.NewGuid().ToString();
        var expireTime = TimeSpan.FromSeconds(3);
        var taskDuration = TimeSpan.FromSeconds(10);

        // 任务持续时间超过锁过期时间，如果自动续期失败，task2 应该能提前获取到锁
        var task1StartTime = DateTime.UtcNow;
        var task1Completed = false;

        var task1 = Task.Run(async () =>
        {
            await using var result = await _lockProvider.LockAsync(lockKey, expireTime, autoExtend: true);
            Assert.NotNull(result);

            // 模拟长时间任务（超过原始过期时间）
            await Task.Delay(taskDuration);
            task1Completed = true;
            _logger.LogInformation("任务1执行完成");
        });

        var task2 = Task.Run(async () =>
        {
            await Task.Delay(TimeSpan.FromSeconds(1));

            // 尝试获取锁，如果 task1 的锁成功续期，task2 应该获取失败直到 task1 完成
            await using var result = await _lockProvider.LockAsync(lockKey,
                TimeSpan.FromSeconds(30),
                TimeSpan.FromSeconds(2));

            // task2 不应该成功获取锁（因为 task1 的锁应该被自动续期）
            Assert.Null(result);

            var task1Elapsed = DateTime.UtcNow - task1StartTime;
            _testOutputHelper.WriteLine($"Task1 运行时长: {task1Elapsed.TotalSeconds:F2} 秒");
        });

        await Task.WhenAll(task1, task2);

        Assert.True(task1Completed, "Task1 应该成功完成");
    }

    /// <summary>
    /// 测试禁用自动延期 - 验证锁会按原始过期时间过期
    /// </summary>
    [Fact]
    public async Task DisableAutoExtend_LockExpiresOnTime()
    {
        var lockKey = Guid.NewGuid().ToString();
        var expireTime = TimeSpan.FromSeconds(2);
        var waitTime = TimeSpan.FromSeconds(3);

        var task1 = Task.Run(async () =>
        {
            await using var result = await _lockProvider.LockAsync(lockKey, expireTime, autoExtend: false);
            Assert.NotNull(result);

            // 模拟长时间任务
            await Task.Delay(waitTime);
            _logger.LogInformation("任务1执行完成");
        });

        var task2 = Task.Run(async () =>
        {
            await Task.Delay(TimeSpan.FromSeconds(1));

            // 等待 task1 的锁过期
            await Task.Delay(TimeSpan.FromSeconds(2.5));

            // task2 应该能成功获取锁（因为 task1 禁用了自动续期）
            await using var result = await _lockProvider.LockAsync(lockKey,
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(2));

            Assert.NotNull(result); // , "Task2 应该成功获取锁（task1 的锁已过期）"
            _logger.LogInformation("任务2成功获取到锁");
        });

        await Task.WhenAll(task1, task2);
    }

    /// <summary>
    /// 测试延期与手动释放 - 验证自动续期的锁可以正常手动释放
    /// </summary>
    [Fact]
    public async Task AutoExtend_ManualDispose_Success()
    {
        var lockKey = Guid.NewGuid().ToString();
        var expireTime = TimeSpan.FromSeconds(5);

        LockInstance? lockInstance = null;

        var task1 = Task.Run(async () =>
        {
            lockInstance = await _lockProvider.LockAsync(lockKey, expireTime, autoExtend: true) as LockInstance;
            Assert.NotNull(lockInstance);

            // 短暂等待以确保自动续期任务已启动
            await Task.Delay(TimeSpan.FromSeconds(2));

            // 手动释放锁
            await lockInstance.DisposeAsync();
            Assert.True(lockInstance.IsDisposed);
            _logger.LogInformation("任务1手动释放锁");
        });

        var task2 = Task.Run(async () =>
        {
            await Task.Delay(TimeSpan.FromSeconds(3));

            // task2 应该能成功获取锁（因为 task1 已经手动释放）
            await using var result = await _lockProvider.LockAsync(lockKey,
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(5));

            Assert.NotNull(result); // , "Task2 应该成功获取锁（task1 已手动释放）"
            _logger.LogInformation("任务2成功获取到锁");
        });

        await Task.WhenAll(task1, task2);
    }
}
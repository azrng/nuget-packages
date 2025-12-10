using Azrng.Core;
using Azrng.Core.Enums;
using System.Diagnostics;
using Xunit.Abstractions;

namespace Common.Core.Test.Helper;

public class LocalLogHelperTest
{
    private readonly ITestOutputHelper _testOutputHelper;

    public LocalLogHelperTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    /// <summary>
    /// 测试写入日志级别
    /// </summary>
    [Fact]
    public void WriteLogLevel_ReturnOk()
    {
        try
        {
            LocalLogHelper.WriteMyLogs("Test", "开始");

            CoreGlobalConfig.MinimumLevel = LogLevel.Information;
            LocalLogHelper.LogTrace("我是trace日志级别日志");
            LocalLogHelper.LogDebug("我是debug日志级别日志");
            LocalLogHelper.LogInformation("我是information日志级别日志");
            LocalLogHelper.LogWarning("我是warning日志级别日志");
            LocalLogHelper.LogError("我是error日志级别日志");
            LocalLogHelper.LogCritical("我是critical日志级别日志");

            LocalLogHelper.WriteMyLogs("Test", "更新日志级别");

            CoreGlobalConfig.MinimumLevel = LogLevel.Warning;
            LocalLogHelper.LogTrace("我是trace日志级别日志");
            LocalLogHelper.LogDebug("我是debug日志级别日志");
            LocalLogHelper.LogInformation("我是information日志级别日志");
            LocalLogHelper.LogWarning("我是warning日志级别日志");
            LocalLogHelper.LogError("我是error日志级别日志");
            LocalLogHelper.LogCritical("我是critical日志级别日志");
        }
        catch (Exception ex)
        {
            _testOutputHelper.WriteLine(ex.Message);
        }
    }

    /// <summary>
    /// 测试多线程写入日志
    /// </summary>
    [Fact]
    public void MultiTaskWrite_ReturnOk()
    {
        try
        {
            Parallel.For(1, 10, i =>
            {
                _testOutputHelper.WriteLine(i.ToString());
                LocalLogHelper.WriteMyLogs("Test", $"测试写入{i} {Environment.CurrentManagedThreadId}");
            });
        }
        catch (Exception ex)
        {
            _testOutputHelper.WriteLine(ex.Message);
        }
    }

    /// <summary>
    /// 测试多线程写入日志
    /// </summary>
    [Fact]
    public void ThreadWrite_ReturnOk()
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();

            // 创建多个线程来测试多线程写入日志
            var threads = new Thread[5];
            for (var i = 0; i < threads.Length; i++)
            {
                threads[i] = new Thread(() =>
                {
                    for (var j = 0; j < 10; j++)
                    {
                        Thread.Sleep(1000);
                        LocalLogHelper.WriteMyLogs("ERROR", $"线程 {Environment.CurrentManagedThreadId} 的日志消息 {j}");
                    }
                });
                threads[i].Start();
            }

            // 等待所有线程完成
            foreach (var thread in threads)
            {
                thread.Join();
            }

            var s = stopwatch.ElapsedMilliseconds;
            _testOutputHelper.WriteLine($"当前方法耗时：{s}");
        }
        catch (Exception ex)
        {
            _testOutputHelper.WriteLine(ex.Message);
        }
    }

    [Fact]
    public void ThreadWrite2_ReturnOk()
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();

            // 创建多个线程来测试多线程写入日志
            var threads = new Thread[5];
            for (var i = 0; i < threads.Length; i++)
            {
                threads[i] = new Thread(() =>
                {
                    for (var j = 0; j < 10; j++)
                    {
                        Thread.Sleep(1000);
                        LocalLogHelper.WriteMyLogs("ERROR", $"线程 {Environment.CurrentManagedThreadId} 的日志消息 {j}");
                    }
                });
                threads[i].Start();
            }

            // 等待所有线程完成
            foreach (var thread in threads)
            {
                thread.Join();
            }

            var s = stopwatch.ElapsedMilliseconds;
            _testOutputHelper.WriteLine($"当前方法耗时：{s}");
        }
        catch (Exception ex)
        {
            _testOutputHelper.WriteLine(ex.Message);
        }
    }
}
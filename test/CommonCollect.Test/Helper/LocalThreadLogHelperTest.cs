using System.Diagnostics;
using Xunit.Abstractions;

namespace CommonCollect.Test.Helper;

public class LocalThreadLogHelperTest
{
    private readonly ITestOutputHelper _testOutputHelper;

    public LocalThreadLogHelperTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void SampleWrite_ReturnOk()
    {
        var log = new LocalThreadLogHelper();
        log.WriteMyLogs("测试数据输出");
    }

    [Fact]
    public void ThreadWrite_ReturnOk()
    {
        var log = new LocalThreadLogHelper();
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
                        log.WriteMyLogs($"线程 {Environment.CurrentManagedThreadId} 的日志消息 {j}");
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

    // [Fact]
    // public void ThreadWrite2_ReturnOk()
    // {
    //     var log = new LocalThreadLogHelper();
    //     try
    //     {
    //         var stopwatch = Stopwatch.StartNew();
    //
    //         // 创建多个线程来测试多线程写入日志
    //         var threads = new Thread[5];
    //         for (var i = 0; i < threads.Length; i++)
    //         {
    //             threads[i] = new Thread(() =>
    //             {
    //                 for (var j = 0; j < 10; j++)
    //                 {
    //                     Thread.Sleep(1000);
    //                     log.WriteMyLogs("ERROR", $"线程 {Environment.CurrentManagedThreadId} 的日志消息 {j}");
    //                 }
    //             });
    //             threads[i].Start();
    //         }
    //
    //         // 等待所有线程完成
    //         foreach (var thread in threads)
    //         {
    //             thread.Join();
    //         }
    //
    //         var s = stopwatch.ElapsedMilliseconds;
    //         _testOutputHelper.WriteLine($"当前方法耗时：{s}");
    //     }
    //     catch (Exception ex)
    //     {
    //         _testOutputHelper.WriteLine(ex.Message);
    //     }
    // }
}
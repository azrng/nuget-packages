using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Azrng.Core.Helpers;

/// <summary>
/// 执行代码规范
/// </summary>
public interface IAction
{
    void Action();
}

/// <summary>
/// 性能测试工具
/// </summary>
/// <remarks>
/// 2022年6月14日 输出内容汉化
/// 来源自:老赵的性能测试工具
/// </remarks>
public static class CodeTimerHelper
{
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GetThreadTimes(nint hThread, out long lpCreationTime, out long lpExitTime,
        out long lpKernelTime, out long lpUserTime);

    [DllImport("kernel32.dll")]
    private static extern nint GetCurrentThread();

    public delegate void ActionDelegate();

    private static long GetCurrentThreadTimes()
    {
        // kernel32 仅存在于 Windows；非 Windows 平台跳过 CPU 采样（输出 0），避免 DllNotFoundException
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return 0;

        GetThreadTimes(GetCurrentThread(), out _, out _, out var kernelTime, out var userTimer);
        return kernelTime + userTimer;
    }

    /// <summary>
    /// 执行时间对比
    /// </summary>
    /// <param name="name">操作名</param>
    /// <param name="iteration">迭代次数，至少为 1</param>
    /// <param name="action">操作</param>
    public static void Time(string name, int iteration, ActionDelegate action)
    {
        if (string.IsNullOrEmpty(name))
        {
            return;
        }

        if (action == null)
        {
            return;
        }

        if (iteration < 1)
            throw new ArgumentOutOfRangeException(nameof(iteration), "迭代次数至少为 1。");

        //1. Print name
        var currentForeColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(name);

        // 2. Record the latest GC counts
        //GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
        GC.Collect(GC.MaxGeneration);
        var gcCounts = new int[GC.MaxGeneration + 1];
        for (var i = 0; i <= GC.MaxGeneration; i++)
        {
            gcCounts[i] = GC.CollectionCount(i);
        }

        // 3. Run action
        var watch = new Stopwatch();
        watch.Start();
        var ticksFst = GetCurrentThreadTimes(); //100 nanosecond one tick
        for (var i = 0; i < iteration; i++) action();
        var ticks = GetCurrentThreadTimes() - ticksFst;
        watch.Stop();

        // 4. Print CPU
        Console.ForegroundColor = currentForeColor;
        Console.WriteLine("	运行时间:		" +
                          watch.ElapsedMilliseconds.ToString("N0") + "ms");
        Console.WriteLine("	运行时间 (一次):	" +
                          (watch.ElapsedMilliseconds / iteration).ToString("N0") + "ms");
        Console.WriteLine("	CPU 时间:		" + (ticks * 100).ToString("N0")
                                       + "ns");
        Console.WriteLine("	CPU 时间 (一次):	" + (ticks * 100 /
                                              iteration).ToString("N0") + "ns");

        // 5. Print GC
        for (var i = 0; i <= GC.MaxGeneration; i++)
        {
            var count = GC.CollectionCount(i) - gcCounts[i];
            Console.WriteLine("	Gen " + i + ": 			" + count);
        }

        Console.WriteLine();
    }

    /// <summary>
    /// 执行时间对比
    /// </summary>
    /// <param name="name">操作名</param>
    /// <param name="iteration">迭代次数，至少为 1</param>
    /// <param name="action">操作</param>
    public static void Time(string name, int iteration, IAction action)
    {
        if (action == null)
            return;

        //委托给 ActionDelegate 重载，避免两份几乎相同的实现漂移
        Time(name, iteration, new ActionDelegate(action.Action));
    }
}
using Azrng.Core.Helpers;

namespace Azrng.ConsoleApp.DependencyInjection;

/// <summary>
/// 控制台工具
/// </summary>
public static class ConsoleTool
{
    /// <summary>
    /// 打印标题
    /// </summary>
    /// <param name="title"></param>
    public static void PrintTitle(string title)
    {
        PrintDivider();
        ConsoleHelper.WriteSuccessLine($"Title：{title}");
        PrintDivider();
    }

    public static void PrintDivider()
    {
        ConsoleHelper.WriteInfoLine("============================================================================");
    }
}
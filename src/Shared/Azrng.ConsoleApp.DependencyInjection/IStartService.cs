namespace Azrng.ConsoleApp.DependencyInjection;

/// <summary>
/// 服务启动接口
/// </summary>
public interface IServiceStart
{
    /// <summary>
    /// 标题
    /// </summary>
    string Title { get; }

    /// <summary>
    /// 控制台入口
    /// </summary>
    /// <returns></returns>
    Task RunAsync();
}
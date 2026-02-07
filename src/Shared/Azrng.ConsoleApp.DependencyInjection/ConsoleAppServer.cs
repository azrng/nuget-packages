using Azrng.ConsoleApp.DependencyInjection.Logger;
using Azrng.Core.Extension;
using Azrng.Core.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace Azrng.ConsoleApp.DependencyInjection;

/// <summary>
/// 控制台APP服务
/// </summary>
public class ConsoleAppServer
{
    public ConsoleAppServer(string[] args)
    {
        var configBuilder = new ConfigurationBuilder();
        configBuilder.SetBasePath(Environment.CurrentDirectory);
        configBuilder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
        // AddEnvironmentVariables("ASPNETCORE_") 会移除前缀，所以这里用 读取的时候要去掉这个前缀
        configBuilder.AddEnvironmentVariables("ASPNETCORE_");
        configBuilder.AddCommandLine(args);

        IConfigurationRoot config;
        try
        {
            config = configBuilder.Build();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"配置文件加载失败！请检查配置文件是不是哪里写错了？\n错误信息：{ex.Message}");
            throw new ArgumentException("配置文件加载失败!");
        }

        Services = new ServiceCollection();
        Configuration = config;
        Services.AddSingleton<IConfiguration>(config);

        ConfigureLogging();
    }

    /// <summary>
    /// 服务
    /// </summary>
    public IServiceCollection Services { get; private set; }

    /// <summary>
    /// 配置
    /// </summary>
    public IConfiguration Configuration { get; private set; }

    /// <summary>
    /// 配置日志
    /// </summary>
    /// <returns></returns>
    private void ConfigureLogging()
    {
        Console.WriteLine("Application Starting");
        Services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.AddConfiguration(Configuration.GetSection("Logging"));
            loggingBuilder.AddConsole();
            loggingBuilder.AddDebug();
            loggingBuilder.AddProvider(new ExtensionsLoggerProvider());
        });
    }

    /// <summary>
    /// 构建
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public ServiceProvider Build<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>()
        where T : class, IServiceStart
    {
        // 先注册服务本身
        Services.AddSingleton<IServiceStart, T>();

        return Services.BuildServiceProvider();
    }

    /// <summary>
    /// 构建 - 使用委托方式注册服务，完全AOT兼容
    /// </summary>
    /// <typeparam name="TStart"></typeparam>
    /// <param name="registerServicesAction">注册服务的委托</param>
    /// <returns></returns>
    public ServiceProvider Build<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TStart>(
        Action<IServiceCollection>? registerServicesAction) where TStart : class, IServiceStart
    {
        // 先注册服务本身
        Services.AddSingleton<IServiceStart, TStart>();

        // 使用委托注册服务
        registerServicesAction?.Invoke(Services);

        return Services.BuildServiceProvider();
    }
}

public static class ServiceProviderExtensions
{
    /// <summary>
    /// 启动入口
    /// </summary>
    /// <param name="serviceProvider"></param>
    public static async Task RunAsync(this ServiceProvider serviceProvider)
    {
        try
        {
            var service = serviceProvider.GetRequiredService<IServiceStart>();
            ConsoleTool.PrintTitle(service.Title);
            await service.RunAsync();
        }
        catch (Exception ex)
        {
            await LocalLogHelper.WriteMyLogsAsync("ERROR", "未处理异常" + ex.GetExceptionAndStack());
            throw;
        }
    }
}
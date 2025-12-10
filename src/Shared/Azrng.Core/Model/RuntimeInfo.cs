namespace Azrng.Core.Model;

public class RuntimeInfo : LibraryInfo
{
    /// <summary>
    /// 版本
    /// </summary>
    public string Version { get; set; }

    /// <summary>
    /// 框架描述
    /// </summary>
    public string FrameworkDescription { get; set; }

    /// <summary>
    /// 进程总数
    /// </summary>
    public int ProcessorCount { get; set; }

    /// <summary>
    /// os 架构
    /// </summary>
    public string OsArchitecture { get; set; }

    /// <summary>
    /// os 描述
    /// </summary>
    public string OsDescription { get; set; }

    /// <summary>
    /// os 版本
    /// </summary>
    public string OsVersion { get; set; }

    /// <summary>
    /// 机器名
    /// </summary>
    public string MachineName { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// 运行时标识符
    /// </summary>
    public string RuntimeIdentifier { get; set; }

    //
    // 摘要:
    //     Gets a value that indicates whether server garbage collection is enabled.
    //
    // 返回结果:
    //     true if server garbage collection is enabled; otherwise, false.
    public bool IsServerGc { get; set; }

    /// <summary>
    /// 工作目录
    /// </summary>
    public string WorkingDirectory { get; set; }

    ///  <summary>
    /// 进程id
    /// </summary>
    public int ProcessId { get; set; }

    ///  <summary>
    /// 进程路径
    /// </summary>
    public string ProcessPath { get; set; }

    /// <summary>
    /// 是否在容器内运行
    /// </summary>
    public bool IsInContainer { get; set; }

    /// <summary>
    /// 是否是k8s中运行
    /// </summary>
    public bool IsInKubernetes { get; set; }
}

/// <summary>
/// 库信息
/// </summary>
public class LibraryInfo
{
    /// <summary>
    /// 库版本
    /// </summary>
    public string LibraryVersion { get; set; }

    /// <summary>
    /// 库hash值
    /// </summary>
    public string LibraryHash { get; set; }

    /// <summary>
    /// 仓库地址
    /// </summary>
    public string RepositoryUrl { get; set; }
}
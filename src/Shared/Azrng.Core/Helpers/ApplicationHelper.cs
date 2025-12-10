using Azrng.Core.Extension.GuardClause;
using Azrng.Core.Model;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Runtime.InteropServices;

namespace Azrng.Core.Helpers;

public class ApplicationHelper
{
    private static readonly Lazy<RuntimeInfo> _runtimeInfoLazy = new Lazy<RuntimeInfo>(GetRuntimeInfo);

    /// <summary>
    /// 应用根目录
    /// </summary>
    public static readonly string AppRoot = AppDomain.CurrentDomain.BaseDirectory;

    /// <summary>
    /// 应用名
    /// </summary>
    public static string ApplicationName => Assembly.GetEntryAssembly()?.GetName().Name ?? AppDomain.CurrentDomain.FriendlyName;

    private static readonly string _serviceAccountPath = Path.Combine($"{Path.DirectorySeparatorChar}var", "run",
        "secrets", "kubernetes.io", "serviceaccount");

    /// <summary>
    /// 获取运行信息
    /// </summary>
    public static RuntimeInfo RuntimeInfo => _runtimeInfoLazy.Value;

    /// <summary>
    /// 获取库信息
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static LibraryInfo GetLibraryInfo(Type type)
    {
        return GetLibraryInfo(Guard.Against.Null(type, "type").Assembly);
    }

    private static RuntimeInfo GetRuntimeInfo()
    {
        var libraryInfo = GetLibraryInfo(typeof(object).Assembly);
        return new RuntimeInfo
               {
                   Version = Environment.Version.ToString(),
                   ProcessorCount = Environment.ProcessorCount,
                   FrameworkDescription = RuntimeInformation.FrameworkDescription,
                   WorkingDirectory = Environment.CurrentDirectory,
#if NET6_0_OR_GREATER
                   ProcessId = Environment.ProcessId,
                   ProcessPath = Environment.ProcessPath ?? string.Empty,
                   RuntimeIdentifier = RuntimeInformation.RuntimeIdentifier,
#endif
                   OsArchitecture = RuntimeInformation.OSArchitecture.ToString(),
                   OsDescription = RuntimeInformation.OSDescription,
                   OsVersion = Environment.OSVersion.ToString(),
                   MachineName = Environment.MachineName,
                   UserName = Environment.UserName,
                   IsServerGc = GCSettings.IsServerGC,
                   IsInContainer = IsInContainer(),
                   IsInKubernetes = IsInKubernetesCluster(),
                   LibraryVersion = libraryInfo.LibraryVersion,
                   LibraryHash = libraryInfo.LibraryHash,
                   RepositoryUrl = libraryInfo.RepositoryUrl
               };
    }

    /// <summary>
    /// 是否是容器内
    /// </summary>
    /// <returns></returns>
    private static bool IsInContainer()
    {
        return "true".Equals(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"),
            StringComparison.OrdinalIgnoreCase);
    }

    //
    // 摘要:
    //     Whether running inside a k8s cluster
    private static bool IsInKubernetesCluster()
    {
        var environmentVariable = Environment.GetEnvironmentVariable("KUBERNETES_SERVICE_HOST");
        var environmentVariable2 = Environment.GetEnvironmentVariable("KUBERNETES_SERVICE_PORT");
        if (string.IsNullOrEmpty(environmentVariable) || string.IsNullOrEmpty(environmentVariable2))
        {
            return false;
        }

        return File.Exists(Path.Combine(_serviceAccountPath, "token")) &&
               File.Exists(Path.Combine(_serviceAccountPath, "ca.crt"));
    }

    /// <summary>
    /// 成程序集获取库信息
    /// </summary>
    /// <param name="assembly"></param>
    /// <returns></returns>
    public static LibraryInfo GetLibraryInfo(Assembly assembly)
    {
        Guard.Against.Null(assembly, "assembly");
        var customAttribute =
            assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        var repositoryUrl = assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
                                    .FirstOrDefault(x => "RepositoryUrl".Equals(x.Key))
                                    ?.Value ??
                            string.Empty;
        if (customAttribute != null)
        {
            var array = customAttribute.InformationalVersion.Split('+');
            return new LibraryInfo
                   {
                       LibraryVersion = array[0], LibraryHash = array.Length > 1 ? array[1] : string.Empty, RepositoryUrl = repositoryUrl
                   };
        }

        return new LibraryInfo
               {
                   LibraryVersion = assembly.GetName().Version?.ToString() ?? string.Empty,
                   LibraryHash = string.Empty,
                   RepositoryUrl = repositoryUrl
               };
    }
}
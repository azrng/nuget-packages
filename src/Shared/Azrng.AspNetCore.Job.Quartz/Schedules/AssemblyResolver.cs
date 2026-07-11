using Azrng.AspNetCore.Job.Quartz.Options;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Azrng.AspNetCore.Job.Quartz.Schedules
{
    /// <summary>
    /// 统一的程序集解析器，供 DI 注册与自动调度共用，确保两者扫描范围一致
    /// </summary>
    internal static class AssemblyResolver
    {
        /// <summary>
        /// 解析要扫描的程序集列表
        /// </summary>
        /// <param name="options">配置选项</param>
        /// <param name="explicitAssemblies">显式指定的程序集（非空时优先）</param>
        /// <param name="entryAssembly">入口程序集</param>
        /// <param name="callingAssembly">调用程序集</param>
        /// <param name="logger">日志记录器（注册阶段可为 null）</param>
        public static List<Assembly> Resolve(
            QuartzOptions options,
            Assembly[]? explicitAssemblies,
            Assembly? entryAssembly,
            Assembly? callingAssembly,
            ILogger? logger)
        {
            // 1. 显式指定的程序集优先
            if (explicitAssemblies is { Length: > 0 })
            {
                return explicitAssemblies.Distinct().ToList();
            }

            var assemblies = new List<Assembly>();

            // 2. 按配置的程序集名称加载
            if (options.AssemblyNamesToScan.Any())
            {
                logger?.LogInformation("使用配置的程序集列表进行扫描: {Assemblies}",
                    string.Join(", ", options.AssemblyNamesToScan));

                foreach (var assemblyName in options.AssemblyNamesToScan)
                {
                    try
                    {
                        assemblies.Add(Assembly.Load(assemblyName));
                        logger?.LogDebug("添加配置程序集: {AssemblyName}", assemblyName);
                    }
                    catch (Exception ex)
                    {
                        logger?.LogWarning(ex, "无法加载程序集 {AssemblyName}", assemblyName);
                    }
                }

                return FilterExcluded(assemblies, options, logger);
            }

            // 3. 默认行为：入口程序集 + 调用程序集
            if (entryAssembly != null)
            {
                assemblies.Add(entryAssembly);
                logger?.LogDebug("添加入口程序集: {AssemblyName}", entryAssembly.GetName().Name);
            }

            if (callingAssembly != null && callingAssembly != entryAssembly)
            {
                assemblies.Add(callingAssembly);
                logger?.LogDebug("添加调用程序集: {AssemblyName}", callingAssembly.GetName().Name);
            }

            return FilterExcluded(assemblies, options, logger);
        }

        private static List<Assembly> FilterExcluded(List<Assembly> assemblies, QuartzOptions options, ILogger? logger)
        {
            if (!options.ExcludedAssemblyPatterns.Any())
            {
                return assemblies;
            }

            var filtered = assemblies.Where(assembly =>
                                     {
                                         var assemblyName = assembly.GetName().Name;
                                         if (assemblyName == null)
                                         {
                                             return true;
                                         }

                                         foreach (var pattern in options.ExcludedAssemblyPatterns)
                                         {
                                             if (WildcardMatch(assemblyName, pattern))
                                             {
                                                 logger?.LogDebug("排除程序集: {AssemblyName} (匹配模式: {Pattern})", assemblyName, pattern);
                                                 return false;
                                             }
                                         }

                                         return true;
                                     })
                                     .ToList();

            if (filtered.Count < assemblies.Count)
            {
                logger?.LogInformation("应用排除规则后剩余 {Count} 个程序集", filtered.Count);
            }

            return filtered;
        }

        /// <summary>
        /// 简单的通配符匹配（* 任意序列，? 单字符，忽略大小写）
        /// </summary>
        private static bool WildcardMatch(string input, string pattern)
        {
            var regexPattern = "^" +
                               Regex.Escape(pattern)
                                    .Replace("\\*", ".*")
                                    .Replace("\\?", ".") +
                               "$";

            return Regex.IsMatch(input, regexPattern, RegexOptions.IgnoreCase);
        }
    }
}

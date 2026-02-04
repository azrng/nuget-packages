using Azrng.Core.Extension;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
#if NETCOREAPP
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
#endif

#if NET6_0_OR_GREATER
using System.Runtime.Loader;
#endif

namespace Azrng.Core.Helpers
{
    /// <summary>
    /// 程序集帮助类
    /// </summary>
    public static class AssemblyHelper
    {
        /// <summary>
        /// 缓存符合搜索条件的程序集信息
        /// </summary>
        private static readonly ConcurrentDictionary<string, Assembly[]> _assembliesDict = new();

        /// <summary>
        /// 获取程序执行目录下的程序集(支持全匹配Common.dll和模糊匹配Common.*.dll)
        /// </summary>
        /// <param name="searchPattern"></param>
        /// <returns></returns>
        public static Assembly[] GetAssemblies(string searchPattern)
        {
            if (string.IsNullOrWhiteSpace(searchPattern))
                return Array.Empty<Assembly>();

            if (_assembliesDict.TryGetValue(searchPattern, out var assemblies))
            {
                return assemblies;
            }

            assemblies = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, searchPattern,
                    SearchOption.AllDirectories)
                .Select(Assembly.LoadFrom).ToArray();

            _assembliesDict.TryAdd(searchPattern, assemblies);

            return assemblies;
        }

        /// <summary>
        /// 获取入口程序集
        /// </summary>
        /// <returns></returns>
        public static Assembly GetEntryAssembly()
        {
            return Assembly.GetEntryAssembly();
        }

        /// <summary>
        /// 遍历所有程序集
        /// </summary>
        /// <param name="skipSystemAssemblies">是否跳过系统程序集</param>
        /// <returns></returns>
        /// <remarks>复制自杨中科老师的项目</remarks>
        public static IEnumerable<Assembly> GetAllReferencedAssemblies(bool skipSystemAssemblies = true)
        {
            //获取默认应用程序域中的进程可执行文件
            var rootAssembly = Assembly.GetEntryAssembly();
            if (rootAssembly == null)
            {
                // 返回当前执行方法的程序集
                rootAssembly = Assembly.GetCallingAssembly();
            }

            // 返回的程序集
            var returnAssemblies = new HashSet<Assembly>(new AssemblyEquality());
            var assembliesToCheck = new Queue<Assembly>();
            assembliesToCheck.Enqueue(rootAssembly);
            // 将当前执行
            if (skipSystemAssemblies && rootAssembly.IsSystemAssembly())
            {
                if (rootAssembly.IsValid())
                {
                    returnAssemblies.Add(rootAssembly);
                }
            }

            var loadedAssemblies = new HashSet<string>();
            while (assembliesToCheck.Count != 0)
            {
                var assemblyToCheck = assembliesToCheck.Dequeue();
                foreach (var reference in assemblyToCheck.GetReferencedAssemblies())
                {
                    if (loadedAssemblies.Contains(reference.FullName))
                        continue;

                    var assembly = Assembly.Load(reference);
                    if (skipSystemAssemblies && assembly.IsSystemAssembly())
                    {
                        continue;
                    }

                    assembliesToCheck.Enqueue(assembly);
                    loadedAssemblies.Add(reference.FullName);
                    if (assembly.IsValid())
                    {
                        returnAssemblies.Add(assembly);
                    }
                }
            }

            var asmsInBaseDir = Directory.EnumerateFiles(AppContext.BaseDirectory,
                "*.dll", new EnumerationOptions { RecurseSubdirectories = true });
            foreach (var asmPath in asmsInBaseDir)
            {
                // 是否是程序集
                if (!IsManagedAssembly(asmPath))
                {
                    continue;
                }

                var asmName = AssemblyName.GetAssemblyName(asmPath);
                //如果程序集已经加载过了就不再加载
                if (returnAssemblies.Any(x => AssemblyName.ReferenceMatchesDefinition(x.GetName(), asmName)))
                {
                    continue;
                }

                if (skipSystemAssemblies && IsSystemAssembly(asmPath))
                {
                    continue;
                }

                var asm = TryLoadAssembly(asmPath);
                if (asm == null)
                {
                    continue;
                }

                if (!asm.IsValid())
                {
                    continue;
                }

                if (skipSystemAssemblies && asm.IsSystemAssembly())
                {
                    continue;
                }

                returnAssemblies.Add(asm);
            }

            return returnAssemblies.ToArray();
        }


#if NET6_0_OR_GREATER
        /// <summary>
        /// 根据程序集名称获取运行时程序集
        /// </summary>
        /// <param name="assemblyName"></param>
        /// <returns></returns>
        public static Assembly GetAssembly(string assemblyName)
        {
            // 加载程序集
            return AssemblyLoadContext.Default.LoadFromAssemblyName(new AssemblyName(assemblyName));
        }

        /// <summary>
        /// 根据程序集名称、类型完整限定名获取运行时类型
        /// </summary>
        /// <param name="assemblyName"></param>
        /// <param name="typeFullName"></param>
        /// <returns></returns>
        public static Type GetType(string assemblyName, string typeFullName)
        {
            return GetAssembly(assemblyName).GetType(typeFullName);
        }

        /// <summary>
        /// 加载程序集类型，支持格式：程序集;网站类型命名空间
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static Type GetStringType(string str)
        {
            var typeDefinitions = str.Split(';');
            return GetType(typeDefinitions[0], typeDefinitions[1]);
        }
#endif

        /// <summary>
        /// 根据路径加载程序集
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static Assembly LoadAssembly(string path)
        {
            return !File.Exists(path) ? default : Assembly.LoadFrom(path);
        }

        /// <summary>
        /// 通过流加载程序集
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static Assembly LoadAssembly(MemoryStream assembly)
        {
            return Assembly.Load(assembly.ToArray());
        }

        /// <summary>
        /// 根据程序集和类型完全限定名获取运行时类型
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="typeFullName"></param>
        /// <returns></returns>
        public static Type GetType(MemoryStream assembly, string typeFullName)
        {
            return LoadAssembly(assembly).GetType(typeFullName);
        }

        /// <summary>
        /// 尝试去加载程序集
        /// </summary>
        /// <param name="asmPath"></param>
        /// <returns></returns>
        public static Assembly TryLoadAssembly(string asmPath)
        {
            var asmName = AssemblyName.GetAssemblyName(asmPath);
            Assembly asm = null;
            try
            {
                asm = Assembly.Load(asmName);
            }
            catch (BadImageFormatException ex)
            {
                Debug.WriteLine(ex);
            }
            catch (FileLoadException ex)
            {
                Debug.WriteLine(ex);
            }

            if (asm != null) return asm;

            try
            {
                asm = Assembly.LoadFile(asmPath);
            }
            catch (BadImageFormatException ex)
            {
                Debug.WriteLine(ex);
            }
            catch (FileLoadException ex)
            {
                Debug.WriteLine(ex);
            }

            return asm;
        }


        /// <summary>
        /// 判断file这个文件是否是程序集
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static bool IsManagedAssembly(string file)
        {
#if NETCOREAPP
            using var fs = File.OpenRead(file);
            using var peReader = new PEReader(fs);
            return peReader.HasMetadata && peReader.GetMetadataReader().IsAssembly;
#else
            try
            {
                Assembly.ReflectionOnlyLoadFrom(file);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
#endif
        }

        /// <summary>
        /// 是否是系统程序集
        /// </summary>
        /// <param name="asmPath"></param>
        /// <returns></returns>
        public static bool IsSystemAssembly(string asmPath)
        {
            return LoadAssembly(asmPath).IsSystemAssembly();
        }

        class AssemblyEquality : EqualityComparer<Assembly>
        {
            public override bool Equals(Assembly? x, Assembly? y)
            {
                if (x == null && y == null) return true;
                if (x == null || y == null) return false;
                return AssemblyName.ReferenceMatchesDefinition(x.GetName(), y.GetName());
            }

            public override int GetHashCode([DisallowNull] Assembly obj)
            {
                return obj.GetName().FullName.GetHashCode();
            }
        }
    }
}
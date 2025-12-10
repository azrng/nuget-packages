using System;
using System.Reflection;

namespace Azrng.Core.Extension
{
    /// <summary>
    /// 程序集扩展方法
    /// </summary>
    public static class AssemblyExtensions
    {
        /// <summary>
        /// 根据程序集和类型完全限定名获取运行时类型
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="typeFullName"></param>
        /// <returns></returns>
        public static Type GetType(this Assembly assembly, string typeFullName)
        {
            return assembly.GetType(typeFullName);
        }

        /// <summary>
        /// 获取程序集名称
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static string GetAssemblyName(this Assembly assembly)
        {
            return assembly.GetName().Name;
        }

        /// <summary>
        /// 获取程序集名称
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetAssemblyName(this Type type)
        {
            return type.GetTypeInfo().GetAssemblyName();
        }

        /// <summary>
        /// 获取程序集名称
        /// </summary>
        /// <param name="typeInfo"></param>
        /// <returns></returns>
        public static string GetAssemblyName(this TypeInfo typeInfo)
        {
            return typeInfo.Assembly.GetAssemblyName();
        }

        /// <summary>
        /// 是否是微软等的官方Assembly
        /// </summary>
        /// <param name="asm"></param>
        /// <returns></returns>
        public static bool IsSystemAssembly(this Assembly asm)
        {
            var asmCompanyAttr = asm.GetCustomAttribute<AssemblyCompanyAttribute>();
            if (asmCompanyAttr == null)
            {
                return false;
            }

            var companyName = asmCompanyAttr.Company;
            return companyName.Contains("Microsoft");
        }

        /// <summary>
        /// 验证程序集是否有效
        /// </summary>
        /// <param name="asm"></param>
        /// <returns></returns>
        public static bool IsValid(this Assembly asm)
        {
            try
            {
                asm.GetTypes();
                foreach (var typeInfo in asm.DefinedTypes)
                {
                    break;
                }

                return true;
            }
            catch (ReflectionTypeLoadException)
            {
                return false;
            }
        }
    }
}
using Azrng.EFCore.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace Azrng.EFCore.SQLServer
{
    public class BaseDbContext : DbContext
    {
        protected BaseDbContext() { }

        public BaseDbContext(DbContextOptions options)
            : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
#if !NET7_0_OR_GREATER
            optionsBuilder.UseBatchEF_MSSQL();
#endif
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()
                                              .Where(t => t.FullName?.StartsWith("Microsoft") == false))
            {
                foreach (var type in GetSafelyLoadedTypes(assembly)
                                             .Where(t =>
                                                 typeof(IdentityBaseEntity).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract))
                {
                    if (modelBuilder.Model.FindEntityType(type) == null)
                    {
                        modelBuilder.Model.AddEntityType(type);
                    }
                }
            }

            base.OnModelCreating(modelBuilder);
        }

        /// <summary>
        /// 部分程序集可能因依赖缺失无法完整加载类型，取可加载部分继续扫描，避免启动失败
        /// </summary>
        private static Type[] GetSafelyLoadedTypes(System.Reflection.Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (System.Reflection.ReflectionTypeLoadException ex)
            {
                return ex.Types.Where(t => t != null).ToArray();
            }
        }
    }
}
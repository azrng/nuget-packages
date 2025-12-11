using Azrng.EFCore.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace Azrng.EFCore.InMemory
{
    /// <summary>
    /// 基础DbContext
    /// </summary>
    public class BaseDbContext : DbContext
    {
        protected BaseDbContext()
        {
        }

        public BaseDbContext(DbContextOptions options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()
              .Where(t => !(t.FullName.StartsWith("Microsoft.") || t.FullName.StartsWith("System."))))
            {
                var exist = assembly.GetTypes().Any(t =>
                    typeof(IEntity).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract);
                if (exist)
                {
                    modelBuilder.ApplyConfigurationsFromAssembly(assembly);
                }
            }

            base.OnModelCreating(modelBuilder);
        }
    }
}
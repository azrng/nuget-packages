using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace PostgresSqlSample.Model;

public class OpenDbContext : DbContext
{
    public OpenDbContext(DbContextOptions<OpenDbContext> options)
        : base(options) { }

    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }

    // /// <summary>
    // /// pg to_char内置函数  直接加这里是可以正常使用的，但是考虑想外部引用扩展方法的形式使用
    // /// </summary>
    // /// <param name="input">要转换的值</param>
    // /// <param name="format">转换的格式</param>
    // /// <returns></returns>
    // /// <exception cref="NotImplementedException"></exception>
    // [DbFunction(Name = "to_char", IsBuiltIn = true, IsNullable = false)]
    // public static string ToChar(DateTime input, string format="yyyy-MM-dd HH:mm:ss")
    // {
    //     throw new NotImplementedException();
    // }
}
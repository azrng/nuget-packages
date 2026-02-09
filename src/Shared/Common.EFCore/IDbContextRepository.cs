using Azrng.EFCore.Entities;
using Microsoft.EntityFrameworkCore;
#if NET7_0_OR_GREATER
#endif

namespace Azrng.EFCore
{
    /// <summary>
    /// 封装公共数据库操作（带 DbContext 泛型参数）
    /// </summary>
    public interface IBaseRepository<TEntity, TDbContext> : IBaseRepository<TEntity>
        where TEntity : IEntity
        where TDbContext : DbContext
    {
        // 所有方法已继承自 IBaseRepository<TEntity>，无需重复定义
    }
}
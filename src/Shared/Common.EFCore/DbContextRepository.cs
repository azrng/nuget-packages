using Azrng.EFCore.Entities;
using Microsoft.EntityFrameworkCore;

namespace Azrng.EFCore
{
    /// <summary>
    /// 基础操作类（带 DbContext 泛型参数）
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <typeparam name="TDbContext">DbContext 类型</typeparam>
    public class BaseRepository<TEntity, TDbContext> : BaseRepository<TEntity>, IBaseRepository<TEntity, TDbContext>
        where TEntity : IEntity
        where TDbContext : DbContext
    {
        /// <summary>
        /// 强类型的 DbContext
        /// </summary>
        public readonly TDbContext TypedDbContext;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="dbContext">强类型的 DbContext</param>
        public BaseRepository(TDbContext dbContext) : base(dbContext)
        {
            TypedDbContext = dbContext;
        }

        // 所有方法已继承自 BaseRepository<TEntity>，无需重复定义
        // 如需重写某个方法，可以在这里添加 override
    }
}
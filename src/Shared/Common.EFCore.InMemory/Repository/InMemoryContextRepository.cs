using Azrng.EFCore.Entities;
using Microsoft.EntityFrameworkCore;

namespace Azrng.EFCore.InMemory.Repository
{
    /// <summary>
    /// 基础操作类（带 DbContext 泛型参数）
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TDbContext"></typeparam>
    public class InMemoryRepository<TEntity, TDbContext> : BaseRepository<TEntity, TDbContext>, IBaseRepository<TEntity, TDbContext>
        where TEntity : IEntity
        where TDbContext : DbContext
    {
        public InMemoryRepository(TDbContext dbContext) : base(dbContext) { }
    }
}

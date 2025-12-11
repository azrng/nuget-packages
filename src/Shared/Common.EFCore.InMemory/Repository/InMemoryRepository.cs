using Azrng.EFCore.Entities;
using Microsoft.EntityFrameworkCore;

namespace Azrng.EFCore.InMemory.Repository
{
    /// <summary>
    /// 基础操作类
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class InMemoryRepository<TEntity> : BaseRepository<TEntity>, IBaseRepository<TEntity>
        where TEntity : IEntity
    {
        public InMemoryRepository(DbContext dbContext) : base(dbContext)
        {
        }
    }
}
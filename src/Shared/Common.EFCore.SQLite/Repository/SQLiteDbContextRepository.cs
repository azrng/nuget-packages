using Azrng.EFCore.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Azrng.EFCore.SQLite.Repository
{
    /// <summary>
    /// 基础操作类
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TDbContext"></typeparam>
    public class SqliteRepository<TEntity, TDbContext> : BaseRepository<TEntity, TDbContext> where TEntity : IEntity
        where TDbContext : DbContext
    {
        public SqliteRepository(TDbContext dbContext) : base(dbContext) { }

        public override async Task<int> DeleteAsync(Expression<Func<TEntity, bool>> expression)
        {
#if NET7_0_OR_GREATER
            return await _dbContext.Set<TEntity>().Where(expression).ExecuteDeleteAsync();
#else
            return await _dbContext.DeleteRangeAsync(expression);
#endif
        }
    }
}
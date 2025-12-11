using Azrng.EFCore.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Azrng.EFCore.PostgresSql.Repository
{
    /// <summary>
    /// 基础操作类
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TDbContext"></typeparam>
    public class PostgreRepository<TEntity, TDbContext> : BaseRepository<TEntity, TDbContext> where TEntity : IEntity
        where TDbContext : DbContext
    {
        public PostgreRepository(TDbContext dbContext) : base(dbContext) { }

        public override async Task<int> DeleteAsync(Expression<Func<TEntity, bool>> expression)
        {
#if !NET7_0_OR_GREATER
            await _dbContext.DeleteRangeAsync(expression);
            return await _dbContext.SaveChangesAsync();
#else
            await _dbContext.Set<TEntity>().Where(expression).ExecuteDeleteAsync();
            return await _dbContext.SaveChangesAsync();
#endif
        }
    }
}
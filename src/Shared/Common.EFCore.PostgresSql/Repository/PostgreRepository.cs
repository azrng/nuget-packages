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
    public class PostgreRepository<TEntity> : BaseRepository<TEntity>, IBaseRepository<TEntity> where TEntity : IEntity
    {
        public PostgreRepository(DbContext dbContext) : base(dbContext)
        {
        }

        public override async Task<int> DeleteAsync(Expression<Func<TEntity, bool>> expression)
        {
#if !NET7_0_OR_GREATER
            // Zack.EFCore.Batch 的 DeleteRangeAsync 直接批量删除并返回行数，与 MySQL 包行为一致
            return await _dbContext.DeleteRangeAsync(expression);
#else
            // ExecuteDeleteAsync 直接返回删除行数并立即生效，无需再 SaveChanges
            // （额外 SaveChanges 会把返回值换成其他挂起更改数，还会误提交无关修改）
            return await _dbContext.Set<TEntity>().Where(expression).ExecuteDeleteAsync();
#endif
        }
    }
}
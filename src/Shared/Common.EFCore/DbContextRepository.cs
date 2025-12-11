using Azrng.Core.Extension;
using Azrng.Core.Requests;
using Azrng.Core.Results;
using Azrng.EFCore.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
#if NET7_0_OR_GREATER
#endif

namespace Azrng.EFCore
{
    /// <summary>
    /// 基础操作类
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TDbContext"></typeparam>
    public class BaseRepository<TEntity, TDbContext> : IBaseRepository<TEntity, TDbContext> where TEntity : IEntity
        where TDbContext : DbContext
    {
        protected readonly TDbContext _dbContext;

        public BaseRepository(TDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// 追踪的实体
        /// </summary>
        public IQueryable<TEntity> Entities => _dbContext.Set<TEntity>();

        /// <summary>
        /// 不追踪的实体
        /// </summary>
        public IQueryable<TEntity> EntitiesNoTacking => _dbContext.Set<TEntity>().AsNoTracking();

        #region 查询类

        public virtual async Task<TEntity> GetByIdAsync(object id)
        {
            return await _dbContext.Set<TEntity>().FindAsync(id).ConfigureAwait(false);
        }

        public virtual async Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> expression, bool isTracking = false)
        {
            return await GetQueryable(isTracking).Where(expression).FirstOrDefaultAsync().ConfigureAwait(false);
        }

        public virtual async Task<IEnumerable<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> expression,
                                                                     bool isTracking = false)
        {
            return await GetQueryable(isTracking).Where(expression).ToListAsync().ConfigureAwait(false);
        }

        public virtual async Task<GetQueryPageResult<T>> GetPageListAsync<T>(IQueryable<T> query, GetPageSortRequest vm)
            where T : IEntity
        {
            var result = await query.PagedBy(vm, out var total).ToListAsync().ConfigureAwait(false);
            return new GetQueryPageResult<T>(result, new GetQueryPageResult(vm, total));
        }

        public virtual async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> expression = null)
        {
            if (expression is null)
                return await EntitiesNoTacking.AnyAsync().ConfigureAwait(false);
            return await EntitiesNoTacking.AnyAsync(expression).ConfigureAwait(false);
        }

        public virtual async Task<int> CountAsync(Expression<Func<TEntity, bool>> expression = null)
        {
            if (expression is null)
                return await EntitiesNoTacking.CountAsync().ConfigureAwait(false);
            return await EntitiesNoTacking.CountAsync(expression).ConfigureAwait(false);
        }

        public virtual async Task<long> CountLongAsync(Expression<Func<TEntity, bool>> expression = null)
        {
            if (expression is null)
                return await EntitiesNoTacking.LongCountAsync().ConfigureAwait(false);
            return await EntitiesNoTacking.LongCountAsync(expression).ConfigureAwait(false);
        }

        #endregion

        #region 操作类

        public virtual async Task<int> AddAsync(TEntity entity, bool submit = false)
        {
            await _dbContext.Set<TEntity>().AddAsync(entity).ConfigureAwait(false);
            if (submit)
                return await _dbContext.SaveChangesAsync().ConfigureAwait(false);

            return 1;
        }

        public virtual async Task<int> AddAsync(IEnumerable<TEntity> entities, bool submit = false)
        {
            await _dbContext.Set<TEntity>().AddRangeAsync(entities).ConfigureAwait(false);

            if (submit)
                return await _dbContext.SaveChangesAsync().ConfigureAwait(false);

            return 1;
        }

        public virtual async Task<int> UpdateAsync(TEntity entity, bool submit = false)
        {
            _dbContext.Set<TEntity>().Update(entity);
            if (submit)
                return await _dbContext.SaveChangesAsync().ConfigureAwait(false);

            return 1;
        }

#if NET7_0_OR_GREATER && (!NET10_0_OR_GREATER)
        public async Task<int> UpdateAsync(Expression<Func<TEntity, bool>> predict,
            Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> setPropertyCalls)
        {
            return await _dbContext.Set<TEntity>().Where(predict).ExecuteUpdateAsync(setPropertyCalls)
                .ConfigureAwait(false);
        }
#elif NET10_0_OR_GREATER
        public async Task<int> UpdateAsync(Expression<Func<TEntity, bool>> predict, Action<UpdateSettersBuilder<TEntity>> setPropertyCalls)
        {
            return await _dbContext.Set<TEntity>().Where(predict).ExecuteUpdateAsync(setPropertyCalls)
                                   .ConfigureAwait(false);
        }
#endif

        public virtual async Task<int> UpdateAsync(IEnumerable<TEntity> entity, bool submit = false)
        {
            _dbContext.Set<TEntity>().UpdateRange(entity);
            if (submit)
                return await _dbContext.SaveChangesAsync().ConfigureAwait(false);

            return 1;
        }

        public virtual async Task<int> DeleteAsync(TEntity entity, bool submit = false)
        {
            _dbContext.Set<TEntity>().Remove(entity);
            if (submit)
                return await _dbContext.SaveChangesAsync().ConfigureAwait(false);

            return 1;
        }

        public virtual async Task<int> DeleteAsync(IEnumerable<TEntity> entity, bool submit = false)
        {
            _dbContext.Set<TEntity>().RemoveRange(entity);
            if (submit)
                return await _dbContext.SaveChangesAsync().ConfigureAwait(false);

            return 1;
        }

        public virtual async Task<int> DeleteAsync(Expression<Func<TEntity, bool>> expression)
        {
            var entities = await _dbContext.Set<TEntity>().Where(expression).ToListAsync();
            if (entities.Count == 0)
                return 0;

            _dbContext.Set<TEntity>().RemoveRange(entities);
            return await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        #endregion

        private IQueryable<TEntity> GetQueryable(bool isTracking = false)
        {
            return isTracking ? Entities : EntitiesNoTacking;
        }
    }
}
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
    /// 封装公共数据库操作
    /// </summary>
    public interface IBaseRepository<TEntity, TDbContext> where TEntity : IEntity
        where TDbContext : DbContext
    {
        /// <summary>
        /// 追踪的实体
        /// </summary>
        IQueryable<TEntity> Entities { get; }

        /// <summary>
        /// 不追踪的实体
        /// </summary>
        IQueryable<TEntity> EntitiesNoTacking { get; }

        #region 查询类

        /// <summary>
        /// 根据主键查询
        /// </summary>
        /// <param name="id">实体ID</param>
        /// <returns></returns>
        Task<TEntity> GetByIdAsync(object id);

        /// <summary>
        /// 查询实体
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="isTracking">是否追踪</param>
        /// <returns></returns>
        Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> expression, bool isTracking = false);

        /// <summary>
        /// 根据条件查询
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="isTracking">是否追踪</param>
        /// <returns></returns>
        Task<IEnumerable<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> expression, bool isTracking = false);

        /// <summary>
        /// 根据IQueryable分页查询
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="query">IQueryable</param>
        /// <param name="vm">分页请求参数</param>
        /// <returns></returns>
        Task<GetQueryPageResult<T>> GetPageListAsync<T>(IQueryable<T> query, GetPageSortRequest vm)
            where T : IEntity;

        /// <summary>
        ///是否存在
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <returns></returns>
        Task<bool> AnyAsync(Expression<Func<TEntity, bool>> expression = null);

        /// <summary>
        /// 查询满足条件的行数
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <returns></returns>
        Task<int> CountAsync(Expression<Func<TEntity, bool>> expression = null);

        /// <summary>
        /// 查询满足条件的行数
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <returns></returns>
        Task<long> CountLongAsync(Expression<Func<TEntity, bool>> expression = null);

        #endregion

        #region 操作类

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="submit">是否提交</param>
        /// <returns></returns>
        Task<int> AddAsync(TEntity entity, bool submit = false);

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="entities">实体</param>
        /// <param name="submit">是否提交</param>
        /// <returns></returns>
        Task<int> AddAsync(IEnumerable<TEntity> entities, bool submit = false);

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="submit">是否提交</param>
        /// <returns></returns>
        Task<int> UpdateAsync(TEntity entity, bool submit = false);

#if NET7_0_OR_GREATER && (!NET10_0_OR_GREATER)
        /// <summary>
        /// 批量更新
        /// </summary>
        /// <param name="predict"></param>
        /// <param name="setPropertyCalls"></param>
        /// <returns></returns>
        Task<int> UpdateAsync(Expression<Func<TEntity, bool>> predict,
            Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> setPropertyCalls);
#elif NET10_0_OR_GREATER
        /// <summary>
        /// 批量更新
        /// </summary>
        /// <param name="predict"></param>
        /// <param name="setPropertyCalls"></param>
        /// <returns></returns>
        Task<int> UpdateAsync(Expression<Func<TEntity, bool>> predict, Action<UpdateSettersBuilder<TEntity>> setPropertyCalls);
#endif

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="submit">是否提交</param>
        /// <returns></returns>
        Task<int> UpdateAsync(IEnumerable<TEntity> entity, bool submit = false);

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="submit">是否提交</param>
        /// <returns></returns>
        Task<int> DeleteAsync(TEntity entity, bool submit = false);

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <returns></returns>
        Task<int> DeleteAsync(Expression<Func<TEntity, bool>> expression);

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="submit">是否提交</param>
        /// <returns></returns>
        Task<int> DeleteAsync(IEnumerable<TEntity> entity, bool submit = false);

        #endregion
    }
}
using Azrng.EFCore.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Azrng.EFCore.SQLServer.Repository
{
    /// <summary>
    /// 基础操作类
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class SqlServerRepository<TEntity> : BaseRepository<TEntity>, IBaseRepository<TEntity> where TEntity : IEntity
    {
        public SqlServerRepository(DbContext dbContext) : base(dbContext) { }

        ///<inheritdoc cref="IBaseRepository{Tentity}.DeleteAsync(Expression{Func{Tentity, bool}})"/>
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
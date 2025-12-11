using Azrng.EFCore.Entities;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Azrng.EFCore
{
    /// <summary>
    /// 工作单元
    /// </summary>
    public interface IUnitOfWork
    {
        /// <summary>
        /// 获取数据库
        /// </summary>
        /// <returns></returns>
        DatabaseFacade GetDatabase();

        /// <summary>
        /// 获取一个仓储类
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        IBaseRepository<TEntity> GetRepository<TEntity>() where TEntity : IEntity;

        /// <summary>
        /// 提交当前单元操作的更改
        /// </summary>
        /// <returns>受影响行数</returns>
        int SaveChanges();

        /// <summary>
        /// 异步提交
        /// </summary>
        /// <returns>受影响行数</returns>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// 提交事务当前单元操作的更改
        /// </summary>
        /// <param name="action"></param>
        /// <param name="isolationLevel"></param>
        void CommitTransaction(Action action, IsolationLevel isolationLevel = IsolationLevel.Unspecified);

        /// <summary>
        /// 提交事务
        /// </summary>
        /// <param name="func"></param>
        /// <param name="isolationLevel"></param>
        Task CommitTransactionAsync(Func<Task> func, IsolationLevel isolationLevel = IsolationLevel.Unspecified);

#if NET7_0_OR_GREATER
        /// <summary>
        /// 查询标量（非实体）类型 <see cref="https://learn.microsoft.com/zh-cn/ef/core/querying/sql-queries#querying-scalar-non-entity-types"/>
        /// </summary>
        /// <param name="sql">sql</param>
        /// <returns></returns>
        IQueryable<T> SqlQuery<T>(FormattableString sql);
#endif

        /// <summary>
        /// 执行任意SQL
        /// </summary>
        /// <param name="sql">sql</param>
        /// <param name="parameters">参数</param>
        /// <returns></returns>
        int ExecuteSqlCommand(string sql, params object[] parameters);

        /// <summary>
        /// 执行任意SQL
        /// </summary>
        /// <param name="sql">sql</param>
        /// <param name="parameters">参数</param>
        /// <returns></returns>
        Task<int> ExecuteSqlCommandAsync(string sql, params object[] parameters);

        /// <summary>
        /// 执行任意SQL返回首行首列
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        object ExecuteScalar(string sql, params object[] parameters);

        /// <summary>
        /// 执行sql返回首行首列
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        Task<object> ExecuteScalarAsync(string sql, params object[] parameters);

        /// <summary>
        /// 执行任意SQL查询并返回DataTable
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        DataTable SqlQueryDataTable(string sql, params object[] parameters);

        /// <summary>
        /// 执行任意SQL查询并返回DataTable
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        Task<DataTable> SqlQueryDataTableAsync(string sql, params object[] parameters);

        /// <summary>
        /// 执行任意SQL查询并返回List
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        List<T> SqlQueryList<T>(string sql, params object[] parameters) where T : class, new();

        /// <summary>
        /// 执行任意SQL查询并返回List
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<List<T>> SqlQueryListAsync<T>(string sql, params object[] parameters) where T : class, new();
    }
}
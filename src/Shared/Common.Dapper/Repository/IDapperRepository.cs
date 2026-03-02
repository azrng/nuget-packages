using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Azrng.Dapper.Repository
{
    /// <summary>
    /// dapper接口
    /// </summary>
    public interface IDapperRepository
    {
        /// <summary>
        /// 查询
        /// </summary>
        List<T>? Query<T>(string sql,
                          object? param = null,
                          IDbTransaction? transaction = null,
                          int? commandTimeout = null,
                          CommandType? commandType = null);

        /// <summary>
        /// 异步查询
        /// </summary>
        Task<List<T>?> QueryAsync<T>(string sql,
                                     object? param = null,
                                     IDbTransaction? transaction = null,
                                     int? commandTimeout = null,
                                     CommandType? commandType = null,
                                     CancellationToken cancellationToken = default);

        /// <summary>
        /// 查询第一条
        /// </summary>
        T? QueryFirstOrDefault<T>(string sql,
                                  object? param = null,
                                  IDbTransaction? transaction = null,
                                  int? commandTimeout = null,
                                  CommandType? commandType = null);

        /// <summary>
        /// 查询第一条
        /// </summary>
        Task<T?> QueryFirstOrDefaultAsync<T>(string sql,
                                             object? param = null,
                                             IDbTransaction? transaction = null,
                                             int? commandTimeout = null,
                                             CommandType? commandType = null,
                                             CancellationToken cancellationToken = default);

        /// <summary>
        /// 查询两个结果集
        /// </summary>
        Task<Tuple<IEnumerable<T1>, IEnumerable<T2>>> QueryMultipleAsync<T1, T2>(string sql,
                                                                                 object? param = null,
                                                                                 IDbTransaction? transaction = null,
                                                                                 int? commandTimeout = null,
                                                                                 CommandType? commandType = null,
                                                                                 CancellationToken cancellationToken = default);

        /// <summary>
        /// 查询三个结果集
        /// </summary>
        Task<Tuple<IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>>> QueryMultipleAsync<T1, T2, T3>(
            string sql,
            object? param = null,
            IDbTransaction? transaction = null,
            int? commandTimeout = null,
            CommandType? commandType = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 查询多条返回到一个集合
        /// </summary>
        Task<IEnumerable<T>> QueryMultipleSameResultSetAsync<T>(string sql,
                                                                object? param = null,
                                                                IDbTransaction? transaction = null,
                                                                int? commandTimeout = null,
                                                                CommandType? commandType = null,
                                                                CancellationToken cancellationToken = default);

        /// <summary>
        /// 执行sql
        /// </summary>
        int Execute(string sql,
                    object? param = null,
                    IDbTransaction? transaction = null,
                    int? commandTimeout = null,
                    CommandType? commandType = null);

        /// <summary>
        /// 执行sql
        /// </summary>
        Task<int> ExecuteAsync(string sql,
                               object? param = null,
                               IDbTransaction? transaction = null,
                               int? commandTimeout = null,
                               CommandType? commandType = null,
                               CancellationToken cancellationToken = default);

        /// <summary>
        /// 批量执行sql
        /// </summary>
        int ExecuteBatch(string sql,
                         IEnumerable<object> param,
                         IDbTransaction? transaction = null,
                         int? commandTimeout = null,
                         CommandType? commandType = null);

        /// <summary>
        /// 异步批量执行sql
        /// </summary>
        Task<int> ExecuteBatchAsync(string sql,
                                    IEnumerable<object> param,
                                    IDbTransaction? transaction = null,
                                    int? commandTimeout = null,
                                    CommandType? commandType = null,
                                    CancellationToken cancellationToken = default);

        /// <summary>
        /// 返回首行首列
        /// </summary>
        T ExecuteScalar<T>(string sql,
                           object? param = null,
                           IDbTransaction? transaction = null,
                           int? commandTimeout = null,
                           CommandType? commandType = null);

        /// <summary>
        /// 返回首行首列
        /// </summary>
        Task<T> ExecuteScalarAsync<T>(string sql,
                                      object? param = null,
                                      IDbTransaction? transaction = null,
                                      int? commandTimeout = null,
                                      CommandType? commandType = null,
                                      CancellationToken cancellationToken = default);

        /// <summary>
        /// 分页查询
        /// </summary>
        Task<PagedResult<T>> QueryPagedAsync<T>(string dataSql,
                                                string countSql,
                                                object? param = null,
                                                IDbTransaction? transaction = null,
                                                int? commandTimeout = null,
                                                CommandType? commandType = null,
                                                CancellationToken cancellationToken = default);

        /// <summary>
        /// 在事务中执行操作
        /// </summary>
        void ExecuteInTransaction(Action<IDbTransaction> action,
                                  IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);

        /// <summary>
        /// 在事务中执行操作
        /// </summary>
        TResult ExecuteInTransaction<TResult>(Func<IDbTransaction, TResult> action,
                                              IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);

        /// <summary>
        /// 在事务中执行操作
        /// </summary>
        Task ExecuteInTransactionAsync(Func<IDbTransaction, Task> action,
                                       IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
                                       CancellationToken cancellationToken = default);

        /// <summary>
        /// 在事务中执行操作
        /// </summary>
        Task<TResult> ExecuteInTransactionAsync<TResult>(Func<IDbTransaction, Task<TResult>> action,
                                                         IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
                                                         CancellationToken cancellationToken = default);
    }
}
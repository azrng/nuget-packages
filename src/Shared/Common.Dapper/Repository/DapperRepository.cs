using Dapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Azrng.Dapper.Repository
{
    /// <summary>
    /// dapper仓储的基类
    /// </summary>
    public class DapperRepository : IDapperRepository
    {
        /// <summary>
        /// 数据库链接
        /// </summary>
        private readonly IDbConnection _dbConnection;

        /// <summary>
        /// 日志记录器
        /// </summary>
        private readonly ILogger<DapperRepository>? _logger;

        private readonly DapperRepositoryOptions _options;

        public DapperRepository(IDbConnection dbConnection,
                                ILogger<DapperRepository>? logger = null,
                                DapperRepositoryOptions? options = null)
        {
            _dbConnection = dbConnection;
            _logger = logger;
            _options = options ?? new DapperRepositoryOptions();
        }

        public List<T>? Query<T>(string sql,
                                 object? param = null,
                                 IDbTransaction? transaction = null,
                                 int? commandTimeout = null,
                                 CommandType? commandType = null)
        {
            var command = CreateCommandDefinition(sql, param, transaction, commandTimeout, commandType);
            return ExecuteWithLogging(sql,
                () => _dbConnection.Query<T>(command)?.ToList(),
                result => result?.Count ?? 0);
        }

        public async Task<List<T>?> QueryAsync<T>(string sql,
                                                   object? param = null,
                                                   IDbTransaction? transaction = null,
                                                   int? commandTimeout = null,
                                                   CommandType? commandType = null,
                                                   CancellationToken cancellationToken = default)
        {
            var command = CreateCommandDefinition(sql, param, transaction, commandTimeout, commandType, cancellationToken);
            return await ExecuteWithLoggingAsync(sql,
                async () => (await _dbConnection.QueryAsync<T>(command))?.ToList(),
                result => result?.Count ?? 0);
        }

        public T? QueryFirstOrDefault<T>(string sql,
                                         object? param = null,
                                         IDbTransaction? transaction = null,
                                         int? commandTimeout = null,
                                         CommandType? commandType = null)
        {
            var command = CreateCommandDefinition(sql, param, transaction, commandTimeout, commandType);
            return ExecuteWithLogging(sql,
                () => _dbConnection.QueryFirstOrDefault<T>(command),
                result => result != null ? 1 : 0);
        }

        public async Task<T?> QueryFirstOrDefaultAsync<T>(string sql,
                                                           object? param = null,
                                                           IDbTransaction? transaction = null,
                                                           int? commandTimeout = null,
                                                           CommandType? commandType = null,
                                                           CancellationToken cancellationToken = default)
        {
            var command = CreateCommandDefinition(sql, param, transaction, commandTimeout, commandType, cancellationToken);
            return await ExecuteWithLoggingAsync(sql,
                async () => await _dbConnection.QueryFirstOrDefaultAsync<T>(command),
                result => result != null ? 1 : 0);
        }

        public async Task<Tuple<IEnumerable<T1>, IEnumerable<T2>>> QueryMultipleAsync<T1, T2>(string sql,
                                                                                                object? param = null,
                                                                                                IDbTransaction? transaction = null,
                                                                                                int? commandTimeout = null,
                                                                                                CommandType? commandType = null,
                                                                                                CancellationToken cancellationToken = default)
        {
            var command = CreateCommandDefinition(sql, param, transaction, commandTimeout, commandType, cancellationToken);
            return await ExecuteWithLoggingAsync(sql, async () =>
            {
                using var result = await _dbConnection.QueryMultipleAsync(command);
                var set1 = (await result.ReadAsync<T1>()).ToList();
                var set2 = (await result.ReadAsync<T2>()).ToList();
                return new Tuple<IEnumerable<T1>, IEnumerable<T2>>(set1, set2);
            }, result => result.Item1.Count() + result.Item2.Count());
        }

        public async Task<Tuple<IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>>> QueryMultipleAsync<T1, T2, T3>(
            string sql,
            object? param = null,
            IDbTransaction? transaction = null,
            int? commandTimeout = null,
            CommandType? commandType = null,
            CancellationToken cancellationToken = default)
        {
            var command = CreateCommandDefinition(sql, param, transaction, commandTimeout, commandType, cancellationToken);
            return await ExecuteWithLoggingAsync(sql, async () =>
            {
                using var result = await _dbConnection.QueryMultipleAsync(command);
                var set1 = (await result.ReadAsync<T1>()).ToList();
                var set2 = (await result.ReadAsync<T2>()).ToList();
                var set3 = (await result.ReadAsync<T3>()).ToList();
                return new Tuple<IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>>(set1, set2, set3);
            }, result => result.Item1.Count() + result.Item2.Count() + result.Item3.Count());
        }

        public async Task<IEnumerable<T>> QueryMultipleSameResultSetAsync<T>(string sql,
                                                                              object? param = null,
                                                                              IDbTransaction? transaction = null,
                                                                              int? commandTimeout = null,
                                                                              CommandType? commandType = null,
                                                                              CancellationToken cancellationToken = default)
        {
            var command = CreateCommandDefinition(sql, param, transaction, commandTimeout, commandType, cancellationToken);
            return await ExecuteWithLoggingAsync(sql, async () =>
            {
                var resultList = new List<T>();
                using var multi = await _dbConnection.QueryMultipleAsync(command);

                while (!multi.IsConsumed)
                {
                    var result = (await multi.ReadAsync<T>()).ToList();
                    if (result.Count > 0)
                    {
                        resultList.AddRange(result);
                    }
                }

                return (IEnumerable<T>)resultList;
            }, result => result.Count());
        }

        public int Execute(string sql,
                           object? param = null,
                           IDbTransaction? transaction = null,
                           int? commandTimeout = null,
                           CommandType? commandType = null)
        {
            var command = CreateCommandDefinition(sql, param, transaction, commandTimeout, commandType);
            return ExecuteWithLogging(sql,
                () => _dbConnection.Execute(command),
                result => result);
        }

        public async Task<int> ExecuteAsync(string sql,
                                            object? param = null,
                                            IDbTransaction? transaction = null,
                                            int? commandTimeout = null,
                                            CommandType? commandType = null,
                                            CancellationToken cancellationToken = default)
        {
            var command = CreateCommandDefinition(sql, param, transaction, commandTimeout, commandType, cancellationToken);
            return await ExecuteWithLoggingAsync(sql,
                async () => await _dbConnection.ExecuteAsync(command),
                result => result);
        }

        public int ExecuteBatch(string sql,
                                IEnumerable<object> param,
                                IDbTransaction? transaction = null,
                                int? commandTimeout = null,
                                CommandType? commandType = null)
        {
            ArgumentNullException.ThrowIfNull(param);
            var command = CreateCommandDefinition(sql, param, transaction, commandTimeout, commandType);
            return ExecuteWithLogging(sql,
                () => _dbConnection.Execute(command),
                result => result);
        }

        public async Task<int> ExecuteBatchAsync(string sql,
                                                 IEnumerable<object> param,
                                                 IDbTransaction? transaction = null,
                                                 int? commandTimeout = null,
                                                 CommandType? commandType = null,
                                                 CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(param);
            var command = CreateCommandDefinition(sql, param, transaction, commandTimeout, commandType, cancellationToken);
            return await ExecuteWithLoggingAsync(sql,
                async () => await _dbConnection.ExecuteAsync(command),
                result => result);
        }

        public T ExecuteScalar<T>(string sql,
                                  object? param = null,
                                  IDbTransaction? transaction = null,
                                  int? commandTimeout = null,
                                  CommandType? commandType = null)
        {
            var command = CreateCommandDefinition(sql, param, transaction, commandTimeout, commandType);
            return ExecuteWithLogging(sql,
                () => _dbConnection.ExecuteScalar<T>(command),
                result => result != null ? 1 : 0);
        }

        public async Task<T> ExecuteScalarAsync<T>(string sql,
                                                   object? param = null,
                                                   IDbTransaction? transaction = null,
                                                   int? commandTimeout = null,
                                                   CommandType? commandType = null,
                                                   CancellationToken cancellationToken = default)
        {
            var command = CreateCommandDefinition(sql, param, transaction, commandTimeout, commandType, cancellationToken);
            return await ExecuteWithLoggingAsync(sql,
                async () => await _dbConnection.ExecuteScalarAsync<T>(command),
                result => result != null ? 1 : 0);
        }

        public async Task<PagedResult<T>> QueryPagedAsync<T>(string dataSql,
                                                              string countSql,
                                                              object? param = null,
                                                              IDbTransaction? transaction = null,
                                                              int? commandTimeout = null,
                                                              CommandType? commandType = null,
                                                              CancellationToken cancellationToken = default)
        {
            var logSql = $"{dataSql}; {countSql}";
            return await ExecuteWithLoggingAsync(logSql, async () =>
            {
                var dataCommand = CreateCommandDefinition(dataSql,
                    param,
                    transaction,
                    commandTimeout,
                    commandType,
                    cancellationToken);
                var countCommand = CreateCommandDefinition(countSql,
                    param,
                    transaction,
                    commandTimeout,
                    commandType,
                    cancellationToken);

                var items = (await _dbConnection.QueryAsync<T>(dataCommand)).ToList();
                var totalCount = await _dbConnection.ExecuteScalarAsync<long>(countCommand);

                return new PagedResult<T>
                {
                    Items = items,
                    TotalCount = totalCount
                };
            }, result => result.Items.Count);
        }

        public void ExecuteInTransaction(Action<IDbTransaction> action,
                                         IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            ArgumentNullException.ThrowIfNull(action);
            ExecuteWithLogging("ExecuteInTransaction", () =>
            {
                EnsureConnectionOpen();
                using var transaction = _dbConnection.BeginTransaction(isolationLevel);
                try
                {
                    action(transaction);
                    transaction.Commit();
                    return 0;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            });
        }

        public TResult ExecuteInTransaction<TResult>(Func<IDbTransaction, TResult> action,
                                                     IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            ArgumentNullException.ThrowIfNull(action);
            return ExecuteWithLogging("ExecuteInTransaction", () =>
            {
                EnsureConnectionOpen();
                using var transaction = _dbConnection.BeginTransaction(isolationLevel);
                try
                {
                    var result = action(transaction);
                    transaction.Commit();
                    return result;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }, result => result != null ? 1 : 0);
        }

        public async Task ExecuteInTransactionAsync(Func<IDbTransaction, Task> action,
                                                    IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
                                                    CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(action);
            await ExecuteWithLoggingAsync("ExecuteInTransactionAsync", async () =>
            {
                await EnsureConnectionOpenAsync(cancellationToken);
                using var transaction = await BeginTransactionAsync(isolationLevel, cancellationToken);
                try
                {
                    await action(transaction);
                    await CommitTransactionAsync(transaction, cancellationToken);
                }
                catch
                {
                    await RollbackTransactionAsync(transaction, cancellationToken);
                    throw;
                }

                return 0;
            }, result => result);
        }

        public async Task<TResult> ExecuteInTransactionAsync<TResult>(Func<IDbTransaction, Task<TResult>> action,
                                                                      IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
                                                                      CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(action);
            return await ExecuteWithLoggingAsync("ExecuteInTransactionAsync", async () =>
            {
                await EnsureConnectionOpenAsync(cancellationToken);
                using var transaction = await BeginTransactionAsync(isolationLevel, cancellationToken);
                try
                {
                    var result = await action(transaction);
                    await CommitTransactionAsync(transaction, cancellationToken);
                    return result;
                }
                catch
                {
                    await RollbackTransactionAsync(transaction, cancellationToken);
                    throw;
                }
            }, result => result != null ? 1 : 0);
        }

        /// <summary>
        /// 执行查询并记录日志
        /// </summary>
        private TResult ExecuteWithLogging<TResult>(string sql, Func<TResult> executeFunc, Func<TResult, int>? getRowCount = null)
        {
            _logger?.LogInformation("Executing SQL: {CommandText}", sql);
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var result = executeFunc();
                var rowCount = getRowCount?.Invoke(result) ?? 0;
                _logger?.LogInformation("Executed SQL ({ElapsedMs}ms) RowsReturned: {RowsReturned}, CommandText: {CommandText}",
                    stopwatch.ElapsedMilliseconds, rowCount, sql);
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex,
                    "Execute SQL failed ({ElapsedMs}ms), CommandText: {CommandText}",
                    stopwatch.ElapsedMilliseconds,
                    sql);
                throw;
            }
        }

        /// <summary>
        /// 异步执行查询并记录日志
        /// </summary>
        private async Task<TResult> ExecuteWithLoggingAsync<TResult>(string sql,
                                                                     Func<Task<TResult>> executeFunc,
                                                                     Func<TResult, int>? getRowCount = null)
        {
            _logger?.LogInformation("Executing SQL: {CommandText}", sql);
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var result = await executeFunc();
                var rowCount = getRowCount?.Invoke(result) ?? 0;
                _logger?.LogInformation("Executed SQL ({ElapsedMs}ms) RowsReturned: {RowsReturned}, CommandText: {CommandText}",
                    stopwatch.ElapsedMilliseconds, rowCount, sql);
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex,
                    "Execute SQL failed ({ElapsedMs}ms), CommandText: {CommandText}",
                    stopwatch.ElapsedMilliseconds,
                    sql);
                throw;
            }
        }

        private CommandDefinition CreateCommandDefinition(string sql,
                                                          object? param = null,
                                                          IDbTransaction? transaction = null,
                                                          int? commandTimeout = null,
                                                          CommandType? commandType = null,
                                                          CancellationToken cancellationToken = default)
        {
            var timeout = commandTimeout ?? _options.DefaultCommandTimeout;
            return new CommandDefinition(sql,
                param,
                transaction,
                timeout,
                commandType,
                cancellationToken: cancellationToken);
        }

        private void EnsureConnectionOpen()
        {
            if (_dbConnection.State != ConnectionState.Open)
            {
                _dbConnection.Open();
            }
        }

        private async Task EnsureConnectionOpenAsync(CancellationToken cancellationToken)
        {
            if (_dbConnection.State == ConnectionState.Open)
            {
                return;
            }

            if (_dbConnection is DbConnection dbConnection)
            {
                await dbConnection.OpenAsync(cancellationToken);
                return;
            }

            _dbConnection.Open();
        }

        private async Task<IDbTransaction> BeginTransactionAsync(IsolationLevel isolationLevel,
                                                                 CancellationToken cancellationToken)
        {
            if (_dbConnection is DbConnection dbConnection)
            {
                return await dbConnection.BeginTransactionAsync(isolationLevel, cancellationToken);
            }

            return _dbConnection.BeginTransaction(isolationLevel);
        }

        private static async Task CommitTransactionAsync(IDbTransaction transaction,
                                                         CancellationToken cancellationToken)
        {
            if (transaction is DbTransaction dbTransaction)
            {
                await dbTransaction.CommitAsync(cancellationToken);
                return;
            }

            transaction.Commit();
        }

        private static async Task RollbackTransactionAsync(IDbTransaction transaction,
                                                           CancellationToken cancellationToken)
        {
            if (transaction is DbTransaction dbTransaction)
            {
                await dbTransaction.RollbackAsync(cancellationToken);
                return;
            }

            transaction.Rollback();
        }
    }
}

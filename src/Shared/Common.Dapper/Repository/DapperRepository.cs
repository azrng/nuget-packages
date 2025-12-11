using Dapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
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

        public DapperRepository(IDbConnection dbConnection, ILogger<DapperRepository>? logger = null)
        {
            _dbConnection = dbConnection;
            _logger = logger;
        }

        public List<T>? Query<T>(string sql, object? param = null)
        {
            return ExecuteWithLogging(sql,
                () => _dbConnection.Query<T>(sql, param)?.ToList(),
                result => result?.Count ?? 0);
        }

        public async Task<List<T>?> QueryAsync<T>(string sql, object? param = null)
        {
            return await ExecuteWithLoggingAsync(sql,
                async () => (await _dbConnection.QueryAsync<T>(sql, param))?.ToList(),
                result => result?.Count ?? 0);
        }

        public T QueryFirstOrDefault<T>(string sql, object? param = null)
        {
            return ExecuteWithLogging(sql,
                () => _dbConnection.QueryFirstOrDefault<T>(sql, param),
                result => result != null ? 1 : 0);
        }

        public async Task<T> QueryFirstOrDefaultAsync<T>(string sql, object? param = null)
        {
            return await ExecuteWithLoggingAsync(sql,
                async () => await _dbConnection.QueryFirstOrDefaultAsync<T>(sql, param),
                result => result != null ? 1 : 0);
        }

        public async Task<Tuple<IEnumerable<T1>, IEnumerable<T2>>> QueryMultipleAsync<T1, T2>(string sql, object param)
        {
            return await ExecuteWithLoggingAsync(sql, async () =>
            {
                var result = await _dbConnection.QueryMultipleAsync(sql, param);
                return new Tuple<IEnumerable<T1>, IEnumerable<T2>>(await result.ReadAsync<T1>(), await result.ReadAsync<T2>());
            }, result => result.Item1.Count() + result.Item2.Count());
        }

        public async Task<IEnumerable<T>> QueryMultipleSameResultSetAsync<T>(string sql, object param)
        {
            return await ExecuteWithLoggingAsync(sql, async () =>
            {
                var resultList = new List<T>();
                var multi = await _dbConnection.QueryMultipleAsync(sql, param);

                //遍历结果集
                while (!multi.IsConsumed)
                {
                    var result = await multi.ReadAsync<T>();
                    if (result != null && result.Any())
                    {
                        resultList.AddRange(result);
                    }
                }

                return (IEnumerable<T>)resultList;
            }, result => result.Count());
        }

        public int Execute(string sql, object? param = null)
        {
            return ExecuteWithLogging(sql,
                () => _dbConnection.Execute(sql, param),
                result => result);
        }

        public async Task<int> ExecuteAsync(string sql, object? param = null)
        {
            return await ExecuteWithLoggingAsync(sql,
                async () => await _dbConnection.ExecuteAsync(sql, param),
                result => result);
        }

        public T ExecuteScalar<T>(string sql, object? param = null)
        {
            return ExecuteWithLogging(sql,
                () => _dbConnection.ExecuteScalar<T>(sql, param),
                result => result != null ? 1 : 0);
        }

        public async Task<T> ExecuteScalarAsync<T>(string sql, object? param = null)
        {
            return await ExecuteWithLoggingAsync(sql,
                async () => await _dbConnection.ExecuteScalarAsync<T>(sql, param),
                result => result != null ? 1 : 0);
        }

        /// <summary>
        /// 执行查询并记录日志
        /// </summary>
        private TResult ExecuteWithLogging<TResult>(string sql, Func<TResult> executeFunc, Func<TResult, int>? getRowCount = null)
        {
            _logger?.LogInformation("Executing SQL: {CommandText}", sql);
            var stopwatch = Stopwatch.StartNew();
            var result = executeFunc();
            var rowCount = getRowCount?.Invoke(result) ?? 0;
            _logger?.LogInformation("Executed SQL ({ElapsedMs}ms) RowsReturned: {RowsReturned}, CommandText: {CommandText}",
                stopwatch.ElapsedMilliseconds, rowCount, sql);
            return result;
        }

        /// <summary>
        /// 异步执行查询并记录日志
        /// </summary>
        private async Task<TResult> ExecuteWithLoggingAsync<TResult>(string sql, Func<Task<TResult>> executeFunc,
                                                                     Func<TResult, int>? getRowCount = null)
        {
            _logger?.LogInformation("Executing SQL: {CommandText}", sql);
            var stopwatch = Stopwatch.StartNew();
            var result = await executeFunc();
            var rowCount = getRowCount?.Invoke(result) ?? 0;
            _logger?.LogInformation("Executed SQL ({ElapsedMs}ms) RowsReturned: {RowsReturned}, CommandText: {CommandText}",
                stopwatch.ElapsedMilliseconds, rowCount, sql);
            return result;
        }
    }
}
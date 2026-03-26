using Azrng.DataAccess.Validation;
using Dapper;
using System;
using System.Data.Common;
using System.Linq;

namespace Azrng.DataAccess.Helper
{
    public abstract class DbHelperBase : IDbHelper
    {
        protected DbHelperBase(string connectionString)
        {
            ConnectionString = connectionString;
            DataSourceConfig = new DataSourceConfig();
        }

        protected DbHelperBase(DataSourceConfig dataSourceConfig)
        {
            DataSourceConfig = dataSourceConfig;
        }

        /// <summary>
        /// 数据源配置
        /// </summary>
        public DataSourceConfig DataSourceConfig { get; private set; }

        /// <summary>
        /// 连接字符串
        /// </summary>
        public string ConnectionString { get; protected set; } = string.Empty;

        /// <summary>
        /// 获取数据库连接（由子类实现）
        /// </summary>
        protected abstract DbConnection GetConnection();

        public async Task<bool> ConnectionTestAsync()
        {
            try
            {
                await using var conn = GetConnection();
                await conn.OpenAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async Task<T?> QueryFirstBaseAsync<T>(string sql, object? parameter, bool orDefault)
        {
            await using var conn = GetConnection();
            return orDefault
                ? await conn.QueryFirstOrDefaultAsync<T>(sql, parameter)
                : await conn.QueryFirstAsync<T>(sql, parameter);
        }

        public Task<T?> QueryFirstAsync<T>(string sql, object? parameter = null) => QueryFirstBaseAsync<T>(sql, parameter, false);

        public Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? parameter = null) => QueryFirstBaseAsync<T>(sql, parameter, true);

        public abstract Task<object[][]> QueryArrayAsync(string sql, object? parameters = null, bool header = true);

        public async Task<T?> QueryScalarAsync<T>(string sql, object? parameter = null)
        {
            await using var conn = GetConnection();
            return await conn.ExecuteScalarAsync<T>(sql, parameter);
        }

        public async Task<int> ExecuteAsync(string sql, object? parameter = null)
        {
            await using var conn = GetConnection();
            return await conn.ExecuteAsync(sql, parameter);
        }

        public async Task<List<T>> QueryAsync<T>(string sql, object? parameter = null)
        {
            await using var conn = GetConnection();
            return (await conn.QueryAsync<T>(sql, parameter)).ToList();
        }

        public virtual async Task<int> GetDataCountAsync(string sourceSql, object? param = null)
        {
            var countSql = BuildDataCountSql(sourceSql);
            return await QueryScalarAsync<int>(countSql, param);
        }

        public abstract DbParameter SetParameter(string key, object value);

        public virtual string BuildDataCountSql(string sourceSql) => $"select count(*) from ({sourceSql}) ret";

        public async Task<IEnumerable<T>> GetSplitPageDataAsync<T>(
            string sourceSql,
            int pageIndex,
            int pageSize,
            object? param = null,
            string? orderColumn = null,
            string? orderDirection = null)
        {
            var dataSql = BuildSplitPageSql(sourceSql, pageIndex, pageSize, orderColumn, orderDirection);
            return await QueryAsync<T>(dataSql, param);
        }

        public virtual string BuildSplitPageSql(
            string sourceSql,
            int pageIndex,
            int pageSize,
            string? orderColumn = null,
            string? orderDirection = null)
        {
            SqlPaginationValidator.ValidatePageArguments(pageIndex, pageSize);

            var normalizedOrderColumn = SqlPaginationValidator.NormalizeOrderColumn(orderColumn);
            var normalizedOrderDirection = SqlPaginationValidator.NormalizeOrderDirection(orderDirection);
            if (!string.IsNullOrWhiteSpace(normalizedOrderColumn) && !string.IsNullOrWhiteSpace(normalizedOrderDirection))
            {
                sourceSql += $" order by {normalizedOrderColumn} {normalizedOrderDirection}";
            }

            sourceSql += $" limit {pageSize} offset {(pageIndex - 1) * pageSize}";
            return sourceSql;
        }

        public virtual object? ConvertReaderValueObject(object? readerValue, Type readerValueType)
        {
            if (readerValue == null)
                return null;

            if (DataSourceConfig == null)
                return readerValue;

            if (readerValueType == typeof(DateTime))
            {
                if (DataSourceConfig.TimeIsUtc)
                {
                    var timeZone = TimeZoneInfo.FindSystemTimeZoneById(DataSourceConfig.TimeZoneId);
                    return TimeZoneInfo
                           .ConvertTimeFromUtc(Convert.ToDateTime(readerValue), timeZone)
                           .ToStandardString();
                }

                return readerValue.ToString()!.ToDateTime().ToStandardString();
            }

            if (DataSourceConfig.DecimalIsTwo &&
                (readerValueType == typeof(decimal) ||
                 readerValueType == typeof(double) ||
                 readerValueType == typeof(float)))
            {
                return Math.Round(Convert.ToDouble(readerValue), 2);
            }

            return readerValue;
        }
    }
}

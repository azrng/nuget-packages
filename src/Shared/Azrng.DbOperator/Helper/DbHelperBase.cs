using Dapper;
using System.Data.Common;

namespace Azrng.DbOperator.Helper
{
    public abstract class DbHelperBase : IDbHelper
    {
        protected DbHelperBase(string connectionString)
        {
            ConnectionString = connectionString;
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
        public string ConnectionString { get; protected set; }

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
            catch
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

        /// <summary>
        /// 由子类实现
        /// </summary>
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

        /// <summary>
        /// 获取数据总数
        /// </summary>
        public virtual async Task<int> GetDataCountAsync(string sourceSql, object? param = null)
        {
            var countSql = BuildDataCountSql(sourceSql);
            return await QueryScalarAsync<int>(countSql, param);
        }

        /// <summary>
        /// 设置参数（由子类实现）
        /// </summary>
        public abstract DbParameter SetParameter(string key, object value);

        /// <summary>
        /// 构建统计SQL
        /// </summary>
        public virtual string BuildDataCountSql(string sourceSql) => $"select count(*) from ({sourceSql}) ret";

        /// <summary>
        /// 分页查询
        /// </summary>
        public async Task<IEnumerable<T>> GetSplitPageDataAsync<T>(string sourceSql, int pageIndex, int pageSize,
                                                                   object? param = null, string? orderColumn = null,
                                                                   string? orderDirection = null)
        {
            var dataSql = BuildSplitPageSql(sourceSql, pageIndex, pageSize, orderColumn, orderDirection);
            return await QueryAsync<T>(dataSql, param);
        }

        /// <summary>
        /// 构建分页SQL
        /// </summary>
        public virtual string BuildSplitPageSql(string sourceSql, int pageIndex, int pageSize,
                                                string? orderColumn = null, string? orderDirection = null)
        {
            if (orderColumn.IsNotNullOrWhiteSpace() && orderDirection.IsNotNullOrWhiteSpace())
                sourceSql += $" order by {orderColumn} {orderDirection}";
            sourceSql += $" limit {pageSize} offset {(pageIndex - 1) * pageSize}";
            return sourceSql;
        }

        public virtual object? ConvertReaderValueObject(object? readerValue, Type readerValueType)
        {
            if (readerValue == null)
                return null;

            if (readerValueType == typeof(DateTime))
            {
                if (DataSourceConfig.TimeIsUtc)
                {
                    return TimeZoneInfo
                           .ConvertTimeFromUtc(Convert.ToDateTime(readerValue), TimeZoneInfo.FindSystemTimeZoneById("Asia/Shanghai"))
                           .ToStandardString();
                }
                else
                {
                    return readerValue.ToString().ToDateTime().ToStandardString();
                }
            }
            else if (DataSourceConfig.DecimalIsTwo &&
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
using System.Data;
using Dapper;

namespace Azrng.SettingConfig.Repository
{
    /// <summary>
    /// dapper仓储的基类
    /// </summary>
    internal class DapperRepository : IDapperRepository
    {
        /// <summary>
        /// 数据库链接
        /// </summary>
        private readonly IDbConnection _dbConnection;

        public DapperRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<List<T>> QueryAsync<T>(string sql, object param = null)
        {
            return (await _dbConnection.QueryAsync<T>(sql, param))?.ToList();
        }

        public async Task<T> QueryFirstOrDefaultAsync<T>(string sql, object param = null)
        {
            return await _dbConnection.QueryFirstOrDefaultAsync<T>(sql, param);
        }

        public async Task<int> ExecuteAsync(string sql, object param = null)
        {
            return await _dbConnection.ExecuteAsync(sql, param);
        }

        public async Task<T> ExecuteScalarAsync<T>(string sql, object param = null)
        {
            return await _dbConnection.ExecuteScalarAsync<T>(sql, param);
        }
    }
}
namespace Azrng.SettingConfig.Repository
{
    /// <summary>
    /// dapper接口
    /// </summary>
    internal interface IDapperRepository
    {
        /// <summary>
        /// 异步查询
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        Task<List<T>> QueryAsync<T>(string sql, object param = null);

        /// <summary>
        /// 查询第一条
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        Task<T> QueryFirstOrDefaultAsync<T>(string sql, object param = null);

        /// <summary>
        /// 执行sql
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        Task<int> ExecuteAsync(string sql, object param = null);

        /// <summary>
        /// 返回首行首列
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        Task<T> ExecuteScalarAsync<T>(string sql, object param = null);
    }
}
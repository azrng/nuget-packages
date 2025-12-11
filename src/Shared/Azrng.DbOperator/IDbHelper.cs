using Azrng.DbOperator.Dto;
using System.Data.Common;

namespace Azrng.DbOperator
{
    public interface IDbHelper
    {
        /// <summary>
        /// 连接字符串
        /// </summary>
        string ConnectionString { get; }

        /// <summary>
        /// 连接测试
        /// </summary>
        /// <returns></returns>
        Task<bool> ConnectionTestAsync();

        /// <summary>
        /// 查询第一条数据
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameter"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<T?> QueryFirstAsync<T>(string sql, object? parameter = null);

        /// <summary>
        /// 查询第一条数据
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameter"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? parameter = null);

        /// <summary>
        /// 执行sql返回二维数组
        /// </summary>
        /// <param name="sql">sql</param>
        /// <param name="parameters">参数</param>
        /// <param name="header">是否包含标题</param>
        /// <returns></returns>
        Task<object[][]> QueryArrayAsync(string sql, object? parameters = null, bool header = true);

        /// <summary>
        /// 查询首行首列
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameter"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<T?> QueryScalarAsync<T>(string sql, object? parameter = null);

        /// <summary>
        /// 执行sql
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameter">参数</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<List<T>> QueryAsync<T>(string sql, object? parameter = null);

        /// <summary>
        /// 执行操作
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        Task<int> ExecuteAsync(string sql, object? parameter = null);

        /// <summary>
        /// 根据sql获取总页数
        /// </summary>
        /// <param name="sourceSql">不带count统计的原始sql</param>
        /// <param name="param"></param>
        /// <returns></returns>
        Task<int> GetDataCountAsync(string sourceSql, object? param = null);

        /// <summary>
        /// 设置参数
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        DbParameter SetParameter(string key, object value);

        /// <summary>
        /// 根据sql分页
        /// </summary>
        /// <param name="sourceSql"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="param"></param>
        /// <param name="orderColumn"></param>
        /// <param name="orderDirection"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<IEnumerable<T>> GetSplitPageDataAsync<T>(string sourceSql, int pageIndex, int pageSize,
                                                      object? param = null, string? orderColumn = null, string? orderDirection = null);

        /// <summary>
        /// 构建分页sql
        /// </summary>
        /// <param name="sourceSql"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="orderColumn"></param>
        /// <param name="orderDirection"></param>
        /// <returns></returns>
        string BuildSplitPageSql(string sourceSql, int pageIndex, int pageSize, string? orderColumn = null,
                                 string? orderDirection = null);
    }
}
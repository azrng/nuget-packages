using Azrng.NMaxCompute.Models;

namespace Azrng.NMaxCompute.Adapter;

/// <summary>
/// 查询执行器接口
/// </summary>
public interface IQueryExecutor
{
    /// <summary>
    /// 执行查询并返回结果
    /// </summary>
    /// <param name="config">配置</param>
    /// <param name="sql">SQL 语句</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>查询结果</returns>
    Task<QueryResult> ExecuteQueryAsync(MaxComputeConfig config, string sql, CancellationToken cancellationToken = default);

    /// <summary>
    /// 测试连接
    /// </summary>
    /// <param name="config">配置</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>是否连接成功</returns>
    Task<bool> TestConnectionAsync(MaxComputeConfig config, CancellationToken cancellationToken = default);
}

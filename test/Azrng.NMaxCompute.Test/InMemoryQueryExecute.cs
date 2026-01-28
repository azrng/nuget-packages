using Azrng.NMaxCompute.Adapter;
using Azrng.NMaxCompute.Models;

namespace Azrng.NMaxCompute.Test;

public class InMemoryQueryExecute : IQueryExecutor
{
    public async Task<QueryResult> ExecuteQueryAsync(MaxComputeConfig config, string sql, CancellationToken cancellationToken = default)
    {
        // 返回测试数据
        return new QueryResult
               {
                   Columns = new[]
                             {
                                 "id",
                                 "name"
                             },
                   Rows = new object[][]
                          {
                              new object[]
                              {
                                  1,
                                  "Test User"
                              }
                          },
                   RowCount = 1
               };
    }

    public async Task<bool> TestConnectionAsync(MaxComputeConfig config, CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(true);
    }
}
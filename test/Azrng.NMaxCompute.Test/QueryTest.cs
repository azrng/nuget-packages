using Azrng.NMaxCompute.Adapter;
using Azrng.NMaxCompute.Models;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Azrng.NMaxCompute.Test
{
    public class QueryTest
    {
        [Fact]
        public async Task Connect_Test()
        {
            var service = new ServiceCollection();
            service.AddScoped<IQueryExecutor, InMemoryQueryExecute>();
            var provider = service.BuildServiceProvider();

            var queryExecutor = provider.GetRequiredService<IQueryExecutor>();
            var config = new MaxComputeConfig
                         {
                             ServerUrl = "http://mc-job-5426-main.sy",
                             AccessId = "xxx",
                             SecretKey = "xxx",
                             JdbcUrl =
                                 "jdbc:odps:https://service.cn-shanghai.maxcompute.aliyun.com/api?project=test&tunnelEndpoint=https://dt.cn-shanghai.maxcompute.aliyun.com",
                             Project = "sample",
                             MaxRows = 10000
                         };
            var connection = MaxComputeConnectionFactory.CreateConnection(queryExecutor, config);

            await connection.OpenAsync();
        }
    }
}
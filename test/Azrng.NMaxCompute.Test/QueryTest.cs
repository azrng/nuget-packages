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
                Endpoint = "http://service.cn-hangzhou.maxcompute.aliyun.com/api",
                AccessId = "test_ak",
                SecretAccessKey = "test_sk",
                Project = "sample",
                Region = "cn-hangzhou",
                MaxRows = 10000
            };
            var connection = MaxComputeConnectionFactory.CreateConnection(queryExecutor, config);

            await connection.OpenAsync();
            Assert.Equal(System.Data.ConnectionState.Open, connection.State);
            Assert.Equal("sample", connection.Database);
            Assert.Equal(config.Endpoint, connection.DataSource);
        }
    }
}

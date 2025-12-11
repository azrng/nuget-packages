using Microsoft.Data.SqlClient;
using System.Data.Common;

namespace Azrng.DbOperator.Helper
{
    public class SqlServerDbHelper : DbHelperBase
    {
        /// <summary>
        /// 连接字符串格式
        /// </summary>
        private static string ConnectionStringFormat => "Server={0},{1};Database={2};Uid={3};Pwd={4};Encrypt=no;";

        public SqlServerDbHelper(DataSourceConfig dataSourceConfig) : base(dataSourceConfig)
        {
            ConnectionString = string.Format(ConnectionStringFormat, dataSourceConfig.Host, dataSourceConfig.Port,
                dataSourceConfig.DbName, dataSourceConfig.User, dataSourceConfig.Password);
        }

        protected override DbConnection GetConnection()
        {
            return new SqlConnection(ConnectionString);
        }

        public override Task<object[][]> QueryArrayAsync(string sql, object? parameters = null, bool header = true)
        {
            throw new NotImplementedException();
        }

        public override DbParameter SetParameter(string key, object value)
        {
            return new SqlParameter($"@{key}", value);
        }

        public override string BuildSplitPageSql(string sourceSql, int pageIndex, int pageSize,
                                                 string? orderColumn = null, string? orderDirection = null)
        {
            var orderData = "(select NULL)";
            if (orderColumn.IsNotNullOrWhiteSpace() && orderDirection.IsNotNullOrWhiteSpace())
                orderData = $"{orderColumn} {orderDirection}";

            var sql = $"select *,row_number() over(order by {orderData}) as rownumber  from ({sourceSql}) t ";

            var ret =
                $"select * from ({sql}) t2  where rownumber between {(pageIndex - 1) * pageSize + 1} and {pageIndex * pageSize} ";
            return ret;
        }
    }
}
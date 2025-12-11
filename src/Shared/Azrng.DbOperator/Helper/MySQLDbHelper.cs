using MySqlConnector;
using System.Data.Common;

namespace Azrng.DbOperator.Helper
{
    public class MySqlDbHelper : DbHelperBase
    {
        /// <summary>
        /// 连接字符串格式
        /// </summary>
        private static string ConnectionStringFormat =>
            "DataSource={0};port={1};Database={2};UserId={3};Password={4};pooling=false;CharSet=utf8;";

        public MySqlDbHelper(string connectionString) : base(connectionString) { }

        public MySqlDbHelper(DataSourceConfig dataSourceConfig) : base(dataSourceConfig)
        {
            ConnectionString = string.Format(ConnectionStringFormat, dataSourceConfig.Host, dataSourceConfig.Port,
                dataSourceConfig.DbName, dataSourceConfig.User, dataSourceConfig.Password);
        }

        protected override DbConnection GetConnection()
        {
            return new MySqlConnection(ConnectionString);
        }

        public override Task<object[][]> QueryArrayAsync(string sql, object? parameters = null, bool header = true)
        {
            throw new NotImplementedException();
        }

        public override DbParameter SetParameter(string key, object value)
        {
            return new MySqlParameter($"@{key}", value);
        }

        public override string BuildSplitPageSql(string sourceSql, int pageIndex, int pageSize,
                                                 string? orderColumn = null,
                                                 string? orderDirection = null)
        {
            if (orderColumn.IsNotNullOrWhiteSpace() && orderDirection.IsNotNullOrWhiteSpace())
                sourceSql += " order by " + orderColumn + " " + orderDirection;
            sourceSql += $"  limit {(pageIndex - 1) * pageSize},{pageSize} ";
            return sourceSql;
        }
    }
}
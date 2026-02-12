using Dapper;
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

        public override async Task<object[][]> QueryArrayAsync(string sql, object? parameters = null, bool header = true)
        {
            var rows = new List<object[]>();

            await using var conn = GetConnection();
            await using var reader = await conn.ExecuteReaderAsync(sql, parameters).ConfigureAwait(false);

            if (header)
            {
                var columns = new List<string>();
                for (var i = 0; i < reader.FieldCount; i++)
                {
                    columns.Add(reader.GetName(i));
                }
                rows.Add(columns.ToArray());
            }

            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                var row = new object[reader.FieldCount];
                for (var i = 0; i < reader.FieldCount; i++)
                {
                    row[i] = reader[i];
                }
                rows.Add(row);
            }

            return rows.ToArray();
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
                sourceSql += $" ORDER BY {orderColumn} {orderDirection}";
            sourceSql += $" LIMIT {pageSize} OFFSET {(pageIndex - 1) * pageSize}";
            return sourceSql;
        }
    }
}
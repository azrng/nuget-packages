using Dapper;
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
            return new SqlParameter($"@{key}", value);
        }

        public override string BuildSplitPageSql(string sourceSql, int pageIndex, int pageSize,
                                                 string? orderColumn = null, string? orderDirection = null)
        {
            var orderData = "(SELECT NULL)";
            if (orderColumn.IsNotNullOrWhiteSpace() && orderDirection.IsNotNullOrWhiteSpace())
                orderData = $"{orderColumn} {orderDirection}";

            var sql = $"SELECT *, ROW_NUMBER() OVER (ORDER BY {orderData}) AS RowNumber FROM ({sourceSql}) t ";
            var ret = $"SELECT * FROM ({sql}) t2 WHERE RowNumber BETWEEN {(pageIndex - 1) * pageSize + 1} AND {pageIndex * pageSize}";
            return ret;
        }
    }
}
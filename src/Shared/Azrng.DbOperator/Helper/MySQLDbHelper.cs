using Azrng.DbOperator.Validation;
using Dapper;
using MySqlConnector;
using System.Data.Common;

namespace Azrng.DbOperator.Helper
{
    public class MySqlDbHelper : DbHelperBase
    {
        public MySqlDbHelper(string connectionString) : base(connectionString) { }

        public MySqlDbHelper(DataSourceConfig dataSourceConfig) : base(dataSourceConfig)
        {
            var builder = new MySqlConnectionStringBuilder
            {
                Server = dataSourceConfig.Host,
                Port = (uint)dataSourceConfig.Port,
                Database = dataSourceConfig.DbName,
                UserID = dataSourceConfig.User,
                Password = dataSourceConfig.Password,
                Pooling = dataSourceConfig.Pooling,
                MinimumPoolSize = (uint)Math.Max(0, dataSourceConfig.MinPoolSize),
                MaximumPoolSize = (uint)Math.Max(dataSourceConfig.MinPoolSize, dataSourceConfig.MaxPoolSize),
                CharacterSet = "utf8"
            };

            ConnectionString = builder.ConnectionString;
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

        public override string BuildSplitPageSql(
            string sourceSql,
            int pageIndex,
            int pageSize,
            string? orderColumn = null,
            string? orderDirection = null)
        {
            SqlPaginationValidator.ValidatePageArguments(pageIndex, pageSize);

            var normalizedOrderColumn = SqlPaginationValidator.NormalizeOrderColumn(orderColumn);
            var normalizedOrderDirection = SqlPaginationValidator.NormalizeOrderDirection(orderDirection);
            if (normalizedOrderColumn.IsNotNullOrWhiteSpace() && normalizedOrderDirection.IsNotNullOrWhiteSpace())
            {
                sourceSql += $" ORDER BY {normalizedOrderColumn} {normalizedOrderDirection}";
            }

            sourceSql += $" LIMIT {pageSize} OFFSET {(pageIndex - 1) * pageSize}";
            return sourceSql;
        }
    }
}
